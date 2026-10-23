using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CubeTower.Application.Localization;
using CubeTower.Application.Services;
using CubeTower.Application.StateMachine;
using CubeTower.Application.States;
using CubeTower.Core.Tower;
using CubeTower.Infrastructure.Save;
using CubeTower.Presentation.Layout;
using CubeTower.Presentation.Views;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Zenject;

namespace CubeTower.Tests.PlayMode
{
    /// <summary>
    /// End-to-end check of the game against the technical task: runs the real bootstrap flow
    /// and plays with simulated pointer gestures sent through the EventSystem.
    /// Requires the generated scenes: run Tools/CubeTower/Build Scene first.
    /// </summary>
    public sealed class GameplayTests
    {
        private const float AnimationWait = 0.9f;

        private string _savePath;
        private string _saveBackupPath;

        private IReadOnlyTower _tower;
        private TowerLayout _layout;
        private PaletteView _palette;
        private TowerZoneView _towerZone;
        private HoleView _hole;
        private TMP_Text _message;
        private ILocalizationService _localization;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            if (!UnityEngine.Application.CanStreamedLevelBeLoaded(SceneAddress.Bootstrap))
                Assert.Ignore("Generated scenes are missing. Run Tools/CubeTower/Build Scene first.");

            // Play on a clean save; the user's own save is restored after the test.
            _savePath = SaveFileLocation.DefaultPath;
            _saveBackupPath = _savePath + ".testbackup";
            
            if (File.Exists(_savePath))
            {
                File.Copy(_savePath, _saveBackupPath, true);
                File.Delete(_savePath);
            }

            // The ProjectContext lives for the whole test run: start the game only once.
            if (!IsInGameplay() || SceneManager.GetActiveScene().name != SceneAddress.Game)
            {
                SceneManager.LoadScene(SceneAddress.Bootstrap);
                yield return WaitUntil(
                    () => IsInGameplay() && SceneManager.GetActiveScene().name == SceneAddress.Game,
                    15f,
                    "The game did not reach the gameplay state.");
            }

            DiContainer project = ProjectContext.Instance.Container;
            _tower = project.Resolve<IReadOnlyTower>();
            _localization = project.Resolve<ILocalizationService>();

            SceneContext gameContext = UnityEngine.Object.FindObjectsOfType<SceneContext>()
                .First(context => context.gameObject.scene.name == SceneAddress.Game);
            _layout = gameContext.Container.Resolve<TowerLayout>();
            _palette = UnityEngine.Object.FindObjectOfType<PaletteView>();
            _towerZone = UnityEngine.Object.FindObjectOfType<TowerZoneView>();
            _hole = UnityEngine.Object.FindObjectOfType<HoleView>();
            _message = UnityEngine.Object.FindObjectOfType<MessageView>().GetComponentInChildren<TMP_Text>(true);

            // The save may come from a previous run in this play session: start with an empty tower.
            project.Resolve<ITower>().Clear();
            yield return new WaitForSeconds(0.2f);
        }

        [TearDown]
        public void TearDown()
        {
            // Leave the user's save exactly as it was: restore it, or remove the one the test created.
            if (File.Exists(_saveBackupPath))
            {
                File.Copy(_saveBackupPath, _savePath, true);
                File.Delete(_saveBackupPath);
            }
            else if (File.Exists(_savePath))
            {
                File.Delete(_savePath);
            }
        }

        [UnityTest]
        public IEnumerator Game_PlaysAccordingToTheTask()
        {
            // --- Bottom scroll: 20 cubes from the config.
            List<CubeView> cells = PaletteCells();
            Assert.That(cells.Count, Is.EqualTo(20), "The bottom scroll must contain the configured cubes.");

            // --- Horizontal gesture scrolls the strip and does not pick up a cube (TZ 4).
            float contentX = _palette.Content.anchoredPosition.x;
            yield return Drag(cells[3].gameObject, Center(cells[3].Rect), Center(cells[3].Rect) + new Vector2(-400f, 20f));
            yield return new WaitForSeconds(0.3f);
            Assert.That(_palette.Content.anchoredPosition.x, Is.Not.EqualTo(contentX).Within(1f), "Horizontal drag must scroll.");
            Assert.That(_tower.Count, Is.EqualTo(0), "A scroll must not place cubes.");

            // --- First cube goes anywhere in the right top part.
            yield return DragFromPalette(0, Center(_towerZone.Area));
            Assert.That(_tower.Count, Is.EqualTo(1), "The first cube must be placed anywhere in the tower zone.");
            Assert.That(_message.text, Is.EqualTo(_localization.Get(LocalizationKeys.CubePlaced)));
            AssertVisualsMatchModel("first cube");

            // --- Next cubes: only when dropped over placed cubes; they jump on top with offset <= 50%.
            yield return DragFromPalette(1, CubeScreenPoint(0));
            yield return DragFromPalette(2, CubeScreenPoint(0));
            Assert.That(_tower.Count, Is.EqualTo(3));
            
            for (int i = 1; i < _tower.Count; i++)
                Assert.That(Mathf.Abs(_tower.Cubes[i].X - _tower.Cubes[i - 1].X), Is.LessThanOrEqualTo(TowerGeometry.MaxHorizontalOffset + 0.001f));

            AssertVisualsMatchModel("three cubes");

            // --- The palette is infinite: cubes stay in the scroll.
            Assert.That(PaletteCells().Count, Is.EqualTo(20));

            // --- Miss: the cube disappears with an animation and a message.
            yield return DragFromPalette(4, Center(_hole.Hole));
            Assert.That(_tower.Count, Is.EqualTo(3), "A cube dropped outside the tower must not be placed.");
            Assert.That(_message.text, Is.EqualTo(_localization.Get(LocalizationKeys.CubeMissed)));
            AssertVisualsMatchModel("miss");

            // --- Empty place in the tower zone, far from the tower: not "over placed cubes", so it is a miss too.
            Rect area = _towerZone.Area.rect;
            Vector2 emptyCorner = RectTransformUtility.WorldToScreenPoint(
                null, _towerZone.Area.TransformPoint(new Vector3(area.xMin + 60f, area.yMax - 60f, 0f)));
            yield return DragFromPalette(6, emptyCorner);
            Assert.That(_tower.Count, Is.EqualTo(3), "A cube dropped on an empty place of the tower zone must not be placed.");
            Assert.That(_message.text, Is.EqualTo(_localization.Get(LocalizationKeys.CubeMissed)));
            AssertVisualsMatchModel("empty place");

            // --- Tower cube dropped outside the hole returns to its place.
            string middleId = _tower.Cubes[1].Id;
            yield return DragTowerCube(1, Center(_towerZone.Area) + new Vector2(-200f, -150f));
            Assert.That(_tower.Count, Is.EqualTo(3));
            Assert.That(_message.text, Is.EqualTo(_localization.Get(LocalizationKeys.CubeReturned)));
            AssertVisualsMatchModel("returned");

            // --- The oval: a corner of the hole's rectangle is outside the ellipse.
            yield return DragTowerCube(1, HoleCorner());
            Assert.That(_tower.Count, Is.EqualTo(3), "A drop into the rectangle corner outside the oval must not count.");
            AssertVisualsMatchModel("hole corner");

            // --- Throw the middle cube into the hole: the cubes above go down smoothly.
            string topId = _tower.Cubes[2].Id;
            yield return DragTowerCube(1, Center(_hole.Hole));
            Assert.That(_tower.Count, Is.EqualTo(2));
            Assert.That(_tower.Cubes.Any(cube => cube.Id == middleId), Is.False);
            Assert.That(_message.text, Is.EqualTo(_localization.Get(LocalizationKeys.CubeRemoved)));
            Assert.That(_tower.Cubes[1].Id, Is.EqualTo(topId));
            yield return new WaitForSeconds(AnimationWait);
            Assert.That(Vector2.Distance(TopCubeView().Rect.anchoredPosition, _layout.GetCubePosition(_tower, 1)), Is.LessThan(1f),
                "The cube above must move down to the freed place.");
            AssertVisualsMatchModel("thrown into the hole");

            // --- Height limit: the cube crossing the top of the screen is placed, the next one is not.
            int guard = 0;
            
            while (!_tower.IsHeightLimitReached(_layout.AreaHeight) && guard++ < 60)
                yield return DragFromPalette(guard % 20, CubeScreenPoint(0), waitForAnimation: false);

            yield return new WaitForSeconds(AnimationWait);
            Assert.That(_tower.IsHeightLimitReached(_layout.AreaHeight), Is.True, "The tower must reach the top of the screen.");
            int countAtLimit = _tower.Count;
            yield return DragFromPalette(5, CubeScreenPoint(0));
            Assert.That(_tower.Count, Is.EqualTo(countAtLimit), "Above the limit nothing is placed.");
            Assert.That(_message.text, Is.EqualTo(_localization.Get(LocalizationKeys.HeightLimit)));
            AssertVisualsMatchModel("height limit");

            // --- Progress is saved (debounced) to an encrypted file.
            yield return WaitUntil(() => File.Exists(_savePath), 5f, "The save file was not written.");
            yield return new WaitForSeconds(1f);
            string raw = File.ReadAllText(_savePath);
            Assert.That(raw.Contains(_tower.Cubes[0].Id), Is.False, "The save must be encrypted.");
        }

        [UnityTest]
        public IEnumerator RepeatedThrowsIntoHole_EachCubeIsVisibleWhileDragged_AndDisappearsAfterDrop()
        {
            // Regression: cube views are pooled, so every next throw reuses a view that has
            // already exploded. It must come back fully visible and vanish again every time.
            DragLayerView dragLayer = UnityEngine.Object.FindObjectOfType<DragLayerView>();

            for (int attempt = 0; attempt < 5; attempt++)
            {
                CubeView cell = PaletteCells()[attempt];
                Vector2 start = Center(cell.Rect);
                Vector2 target = Center(_hole.Hole);

                yield return Drag(
                    cell.gameObject,
                    () =>
                    {
                        CubeView ghost = dragLayer.Layer.GetComponentsInChildren<CubeView>(false).SingleOrDefault();
                        Assert.That(ghost, Is.Not.Null, $"Throw {attempt + 1}: the dragged cube must be on screen.");
                        Assert.That(ghost.Rect.localScale.x, Is.EqualTo(1f).Within(0.01f), $"Throw {attempt + 1}: scale.");
                        Assert.That(ghost.GetComponent<CanvasGroup>().alpha, Is.EqualTo(1f).Within(0.01f), $"Throw {attempt + 1}: alpha.");
                        Assert.That(Vector2.Distance(Center(ghost.Rect), target), Is.LessThan(5f), $"Throw {attempt + 1}: the cube must follow the finger.");
                    },
                    start,
                    start + new Vector2(0f, 160f),
                    target);

                yield return new WaitForSeconds(AnimationWait);
                Assert.That(_tower.Count, Is.EqualTo(0), $"Throw {attempt + 1}: a palette cube in the hole is a miss.");
                Assert.That(_message.text, Is.EqualTo(_localization.Get(LocalizationKeys.CubeMissed)));
                AssertVisualsMatchModel($"throw {attempt + 1}");
            }
        }

        /// <summary>
        /// What the player sees must match the model: one visible view per tower cube,
        /// and no dragged or exploding cube left on the drag layer.
        /// </summary>
        private void AssertVisualsMatchModel(string step)
        {
            int towerViews = _towerZone.CubesRoot.GetComponentsInChildren<CubeView>(false)
                .Count(view => view.gameObject.activeInHierarchy);
            Assert.That(towerViews, Is.EqualTo(_tower.Count), $"{step}: visible tower cubes must match the model.");

            DragLayerView dragLayer = UnityEngine.Object.FindObjectOfType<DragLayerView>();
            int leftovers = dragLayer.Layer.GetComponentsInChildren<CubeView>(false)
                .Count(view => view.gameObject.activeInHierarchy);
            Assert.That(leftovers, Is.EqualTo(0), $"{step}: no cube may stay on the drag layer after the animation.");
        }

        private static bool IsInGameplay() =>
            ProjectContext.HasInstance
            && ProjectContext.Instance.Container.Resolve<IGameStateMachine>().CurrentState is LevelLoopState;

        private List<CubeView> PaletteCells() => 
            _palette.Content.GetComponentsInChildren<CubeView>().ToList();

        private CubeView TopCubeView()
        {
            Vector2 target = _layout.GetCubePosition(_tower, _tower.Count - 1);
            return _towerZone.CubesRoot.GetComponentsInChildren<CubeView>()
                .OrderBy(view => Vector2.Distance(view.Rect.anchoredPosition, target))
                .First();
        }

        private CubeView TowerCubeView(int index)
        {
            Vector2 target = _layout.GetCubePosition(_tower, index);
            return _towerZone.CubesRoot.GetComponentsInChildren<CubeView>()
                .OrderBy(view => Vector2.Distance(view.Rect.anchoredPosition, target))
                .First();
        }

        private Vector2 CubeScreenPoint(int index)
        {
            Vector2 anchored = _layout.GetCubePosition(_tower, index);
            RectTransform root = _towerZone.CubesRoot;
            Vector3 world = root.TransformPoint(anchored + root.rect.min);
            return RectTransformUtility.WorldToScreenPoint(null, world);
        }

        private Vector2 HoleCorner()
        {
            Rect rect = _hole.Hole.rect;
            Vector3 world = _hole.Hole.TransformPoint(new Vector3(rect.xMax - 4f, rect.yMax - 4f, 0f));
            return RectTransformUtility.WorldToScreenPoint(null, world);
        }

        private static Vector2 Center(RectTransform rect) => 
            RectTransformUtility.WorldToScreenPoint(null, rect.TransformPoint(rect.rect.center));

        private IEnumerator DragFromPalette(int cellIndex, Vector2 target, bool waitForAnimation = true)
        {
            CubeView cell = PaletteCells()[cellIndex];
            Vector2 start = Center(cell.Rect);

            // A real finger first moves up out of the strip, then to the target.
            yield return Drag(cell.gameObject, start, start + new Vector2(0f, 160f), target);
            yield return new WaitForSeconds(waitForAnimation ? AnimationWait : 0.05f);
        }

        private IEnumerator DragTowerCube(int index, Vector2 target)
        {
            CubeView view = TowerCubeView(index);
            yield return Drag(view.gameObject, Center(view.Rect), target);
            yield return new WaitForSeconds(AnimationWait);
        }

        /// <summary>Sends the same events the EventSystem sends for a real drag along the given points.</summary>
        private static IEnumerator Drag(GameObject target, params Vector2[] points) => 
            Drag(target, null, points);

        /// <summary>Same as Drag, with a check run while the finger is still down at the last point.</summary>
        private static IEnumerator Drag(GameObject target, Action beforeRelease, params Vector2[] points)
        {
            PointerEventData data = new(EventSystem.current)
            {
                pointerId = 0,
                button = PointerEventData.InputButton.Left,
                position = points[0],
                pressPosition = points[0],
            };

            ExecuteEvents.Execute(target, data, ExecuteEvents.initializePotentialDrag);
            yield return null;

            const int stepsPerSegment = 6;
            bool started = false;
            
            for (int segment = 1; segment < points.Length; segment++)
            {
                for (int step = 1; step <= stepsPerSegment; step++)
                {
                    Vector2 next = Vector2.Lerp(points[segment - 1], points[segment], (float)step / stepsPerSegment);
                    data.delta = next - data.position;
                    data.position = next;

                    if (!started)
                    {
                        started = true;
                        data.dragging = true;
                        ExecuteEvents.Execute(target, data, ExecuteEvents.beginDragHandler);
                    }

                    ExecuteEvents.Execute(target, data, ExecuteEvents.dragHandler);
                    yield return null;
                }
            }

            beforeRelease?.Invoke();
            ExecuteEvents.Execute(target, data, ExecuteEvents.endDragHandler);
            data.dragging = false;
            yield return null;
        }

        private static IEnumerator WaitUntil(Func<bool> condition, float timeout, string failMessage)
        {
            float deadline = Time.realtimeSinceStartup + timeout;
            
            while (!condition())
            {
                if (Time.realtimeSinceStartup > deadline)
                    Assert.Fail(failMessage);

                yield return null;
            }
        }
    }
}
