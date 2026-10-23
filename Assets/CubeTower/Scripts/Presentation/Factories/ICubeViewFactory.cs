using CubeTower.Presentation.Views;
using UnityEngine;

namespace CubeTower.Presentation.Factories
{
    public interface ICubeViewFactory
    {
        CubeView Create(RectTransform parent);

        /// <summary>Returns a view to the pool. The caller must not use it afterwards.</summary>
        void Release(CubeView view);
    }
}
