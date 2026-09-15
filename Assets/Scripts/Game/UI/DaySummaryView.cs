using System.Linq;
using MakeMeHero.Core;
using UnityEngine;
using UnityEngine.UI;

namespace MakeMeHero.Game
{
    /// <summary>Shows the just-completed day after the aggregate returns to Standby.</summary>
    public sealed class DaySummaryView : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;
        [SerializeField] private GameObject summaryRoot;
        [SerializeField] private Text summaryText;
        private int _lastRenderedDay;

        private void OnEnable()
        {
            if (gameManager == null) gameManager = GameManager.Instance;
            if (gameManager != null) gameManager.RunChanged += OnRunChanged;
        }
        private void OnDisable() { if (gameManager != null) gameManager.RunChanged -= OnRunChanged; }
        public void Hide() { if (summaryRoot != null) summaryRoot.SetActive(false); }

        private void OnRunChanged(Run run)
        {
            if (run == null || run.Phase != RunPhase.Standby || run.Day <= 1 || run.Day == _lastRenderedDay) return;
            var previous = gameManager.CurrentResearchLog.DailySnapshots.LastOrDefault();
            if (previous == null) return;
            _lastRenderedDay = run.Day;
            if (summaryRoot != null) summaryRoot.SetActive(true);
            if (summaryText != null)
            {
                var fallen = previous.UnitCombatResults.Count(x => x.Kind == UnitKind.Hero && !x.Survived);
                summaryText.text = string.Format("Day {0} complete\nGold: {1}\nHeroes fallen: {2}", previous.DayNumber, run.Gold, fallen);
            }
        }
    }
}
