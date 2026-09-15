using UnityEngine;

namespace MakeMeHero.Game
{
    /// <summary>Wire UI Buttons to StartDay, Pause, and speed controls without putting rules in UI code.</summary>
    public sealed class BattleControlsView : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;

        private void Awake() { if (gameManager == null) gameManager = GameManager.Instance; }
        public void StartDay() { if (gameManager != null) gameManager.StartDay(); }
        public void Pause() { if (gameManager != null) gameManager.SetBattlePaused(true); }
        public void Resume() { if (gameManager != null) gameManager.SetBattlePaused(false); }
        public void SetSpeed(float multiplier) { if (gameManager != null) gameManager.SetBattleSpeed(multiplier); }
        public void EndRun() { if (gameManager != null) gameManager.EndRun(); }
    }
}
