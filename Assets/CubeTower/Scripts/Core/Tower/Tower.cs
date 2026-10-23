using System;
using System.Collections.Generic;
using System.Linq;
using CubeTower.Core.Common;
using CubeTower.Core.Tower.Rules;
using R3;

namespace CubeTower.Core.Tower
{
    /// <summary>
    /// Domain model of the tower: the single source of truth about which cubes stand where.
    /// It owns its invariants (rules, max 50% offset, height limit) and announces changes
    /// through R3 streams. It knows nothing about views, animations or save files.
    /// </summary>
    public sealed class Tower : ITower, IDisposable
    {
        private readonly IReadOnlyList<IPlacementRule> _rules;
        private readonly IRandomService _random;
        private readonly IIdGenerator _idGenerator;
        private readonly List<TowerCube> _cubes = new();
        private readonly Subject<CubeAdded> _added = new();
        private readonly Subject<CubeRemoved> _removed = new();
        private readonly Subject<Unit> _reset = new();

        private bool _isNotifying;
        private bool _isDisposed;

        public Tower(IEnumerable<IPlacementRule> rules, IRandomService random, IIdGenerator idGenerator)
        {
            _rules = rules?.ToArray() ?? throw new ArgumentNullException(nameof(rules));
            _random = random ?? throw new ArgumentNullException(nameof(random));
            _idGenerator = idGenerator ?? throw new ArgumentNullException(nameof(idGenerator));
        }

        public int Count => _cubes.Count;
        public float BaseY { get; private set; }
        public float TopEdge => BaseY + _cubes.Count;
        
        public IReadOnlyList<TowerCube> Cubes => _cubes;
        public Observable<CubeAdded> Added => _added;
        public Observable<CubeRemoved> Removed => _removed;
        public Observable<Unit> Reset => _reset;

        public bool TryGetIndex(string cubeId, out int index)
        {
            index = _cubes.FindIndex(cube => cube.Id == cubeId);
            return index >= 0;
        }

        public bool IsHeightLimitReached(float areaHeight) => 
            _cubes.Count > 0 && TopEdge > areaHeight;

        public PlacementResult TryPlace(PlacementRequest request)
        {
            EnsureCanMutate();

            if (string.IsNullOrEmpty(request.ColorId)
                || request.AreaWidth <= 0f
                || request.AreaHeight <= 0f
                || !TowerGeometry.IsFinite(request.DropX)
                || !TowerGeometry.IsFinite(request.DropY))
            {
                return PlacementResult.Fail(PlacementFailReasons.InvalidRequest);
            }

            foreach (IPlacementRule rule in _rules)
            {
                PlacementResult check = rule.Check(this, request);
                
                if (!check.IsSuccess)
                    return check;
            }

            float x = _cubes.Count == 0 ? PlaceBase(request) : NextCubeX(request.AreaWidth);
            TowerCube cube = new(_idGenerator.Next(), request.ColorId, x);
            _cubes.Add(cube);

            Notify(_added, new CubeAdded(_cubes.Count - 1, cube));
            return PlacementResult.Placed(cube);
        }

        public bool TryRemove(string cubeId)
        {
            EnsureCanMutate();

            if (!TryGetIndex(cubeId, out int index))
                return false;

            TowerCube removed = _cubes[index];
            _cubes.RemoveAt(index);
            
            NormalizeOffsetsFrom(Math.Max(index, 1));

            Notify(_removed, new CubeRemoved(index, removed));
            return true;
        }

        public void FitToArea(float areaWidth, float areaHeight)
        {
            EnsureCanMutate();

            if (_cubes.Count == 0 || areaWidth <= 0f || areaHeight <= 0f)
                return;

            float left = _cubes.Min(cube => cube.X) - TowerGeometry.HalfCube;
            float right = _cubes.Max(cube => cube.X) + TowerGeometry.HalfCube;
            float shift = 0f;

            if (left < 0f)
                shift = -left;
            else if (right > areaWidth)
                shift = Math.Max(areaWidth - right, -left);

            float baseY = TowerGeometry.Clamp(BaseY, 0f, Math.Max(0f, areaHeight - 1f));
            bool changed = Math.Abs(shift) > TowerGeometry.Epsilon || Math.Abs(baseY - BaseY) > TowerGeometry.Epsilon;

            if (!changed)
                return;

            for (int i = 0; i < _cubes.Count; i++)
                _cubes[i] = _cubes[i].WithX(_cubes[i].X + shift);

            BaseY = baseY;
            Notify(_reset, Unit.Default);
        }

        public void Clear()
        {
            EnsureCanMutate();

            _cubes.Clear();
            BaseY = 0f;
            Notify(_reset, Unit.Default);
        }

        public TowerData CreateData()
        {
            TowerCubeData[] cubes = _cubes
                .Select(cube => new TowerCubeData(cube.Id, cube.ColorId, cube.X))
                .ToArray();

            return new TowerData(TowerData.CurrentVersion, BaseY, cubes);
        }

        public bool Restore(TowerData data)
        {
            EnsureCanMutate();

            if (!IsValid(data))
                return false;

            _cubes.Clear();
            
            foreach (TowerCubeData cube in data.Cubes)
                _cubes.Add(new TowerCube(cube.Id, cube.ColorId, cube.X));

            BaseY = data.BaseY;
            Notify(_reset, Unit.Default);
            return true;
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;
            _added.Dispose();
            _removed.Dispose();
            _reset.Dispose();
        }

        private static bool IsValid(TowerData data)
        {
            if (data == null
                || data.Version != TowerData.CurrentVersion
                || data.Cubes == null
                || !TowerGeometry.IsFinite(data.BaseY)
                || data.BaseY < 0f)
            {
                return false;
            }

            HashSet<string> ids = new();
            
            for (int i = 0; i < data.Cubes.Count; i++)
            {
                TowerCubeData cube = data.Cubes[i];
                if (cube == null
                    || string.IsNullOrEmpty(cube.Id)
                    || string.IsNullOrEmpty(cube.ColorId)
                    || !TowerGeometry.IsFinite(cube.X)
                    || !ids.Add(cube.Id))
                {
                    return false;
                }

                if (i > 0 && 
                    Math.Abs(cube.X - data.Cubes[i - 1].X) > TowerGeometry.MaxHorizontalOffset + TowerGeometry.Epsilon)
                    return false;
            }

            return true;
        }

        private float PlaceBase(PlacementRequest request)
        {
            BaseY = TowerGeometry.Clamp(request.DropY - TowerGeometry.HalfCube, 0f, Math.Max(0f, request.AreaHeight - 1f));
            return ClampToArea(request.DropX, request.AreaWidth);
        }

        private float NextCubeX(float areaWidth)
        {
            float offset = _random.Range(-TowerGeometry.MaxHorizontalOffset, TowerGeometry.MaxHorizontalOffset);
            float x = ClampToArea(_cubes[^1].X + offset, areaWidth);

            return x;
        }

        private static float ClampToArea(float x, float areaWidth) => 
            TowerGeometry.Clamp(x, TowerGeometry.HalfCube, areaWidth - TowerGeometry.HalfCube);

        private void NormalizeOffsetsFrom(int startIndex)
        {
            for (int i = startIndex; i < _cubes.Count; i++)
            {
                float below = _cubes[i - 1].X;
                float clamped = TowerGeometry.Clamp(
                    _cubes[i].X,
                    below - TowerGeometry.MaxHorizontalOffset,
                    below + TowerGeometry.MaxHorizontalOffset);

                if (Math.Abs(clamped - _cubes[i].X) > TowerGeometry.Epsilon)
                    _cubes[i] = _cubes[i].WithX(clamped);
            }
        }

        private void Notify<T>(Subject<T> subject, T value)
        {
            _isNotifying = true;
            
            try
            {
                subject.OnNext(value);
            }
            finally
            {
                _isNotifying = false;
            }
        }

        private void EnsureCanMutate()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(Tower));

            if (_isNotifying)
                throw new InvalidOperationException(
                    "The tower cannot be changed while it is notifying subscribers. Defer the change instead.");
        }
    }
}
