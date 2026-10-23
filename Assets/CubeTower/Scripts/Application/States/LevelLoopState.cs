using CubeTower.Application.Save;
using CubeTower.Application.Services;
using CubeTower.Application.StateMachine;

namespace CubeTower.Application.States
{
    /// <summary>
    /// Gameplay. The scene is loaded and its presenters are running; this state only hides
    /// the curtain and turns autosave on. There is no per-frame logic: the game is event-driven
    /// (input events, DOTween, UniTask continuations and R3 timers are driven by Unity's player loop).
    /// </summary>
    public sealed class LevelLoopState : IState
    {
        private readonly ILoadingCurtain _curtain;
        private readonly AutoSaveService _autoSave;

        public LevelLoopState(ILoadingCurtain curtain, AutoSaveService autoSave)
        {
            _curtain = curtain;
            _autoSave = autoSave;
        }

        public void Enter()
        {
            _autoSave.Start();
            _curtain.Hide();
        }

        public void Exit() => 
            _autoSave.Stop();
    }
}
