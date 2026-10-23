using System;
using System.Collections.Generic;
using CubeTower.Presentation.Settings;
using CubeTower.Presentation.Tweening;
using CubeTower.Presentation.Views;
using UnityEngine;

namespace CubeTower.Presentation.Factories
{
    /// <summary>
    /// Creates cube views with a simple pool: cubes appear and disappear all the time
    /// (palette is infinite, every drag spawns a cube), so reusing them avoids GC spikes.
    /// The factory, not the DI container, constructs views - Zenject stays in the composition root.
    /// </summary>
    public sealed class CubeViewFactory : ICubeViewFactory
    {
        private readonly CubeView _prefab;
        private readonly RectTransform _poolRoot;
        private readonly CubeTweener _tweener;
        private readonly PresentationSettings _settings;
        private readonly Stack<CubeView> _pool = new();

        public CubeViewFactory(CubeView prefab, RectTransform poolRoot, CubeTweener tweener, PresentationSettings settings)
        {
            _prefab = prefab ? prefab : throw new ArgumentNullException(nameof(prefab));
            _poolRoot = poolRoot ? poolRoot : throw new ArgumentNullException(nameof(poolRoot));
            _tweener = tweener ?? throw new ArgumentNullException(nameof(tweener));
            _settings = settings ? settings : throw new ArgumentNullException(nameof(settings));
        }

        public CubeView Create(RectTransform parent)
        {
            CubeView view = null;

            while (_pool.Count > 0 && view == null)
                view = _pool.Pop();

            if (view == null)
            {
                view = UnityEngine.Object.Instantiate(_prefab, parent, false);
                view.Initialize(_tweener, _settings.CubeSprite);
            }
            else
            {
                view.transform.SetParent(parent, false);
            }

            view.ResetState();
            view.gameObject.SetActive(true);
            return view;
        }

        public void Release(CubeView view)
        {
            if (view == null)
                return;

            view.ResetState();
            view.gameObject.SetActive(false);
            view.transform.SetParent(_poolRoot, false);
            _pool.Push(view);
        }
    }
}
