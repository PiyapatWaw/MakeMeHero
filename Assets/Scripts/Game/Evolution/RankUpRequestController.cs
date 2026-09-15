using MakeMeHero.Core;
using UnityEngine;

namespace MakeMeHero.Game
{
    /// <summary>UI bridge for a selected Hero's RankUp request. Applying JSON remains in GameManager.</summary>
    public sealed class RankUpRequestController : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;
        [SerializeField] private string selectedHeroId;
        [TextArea(4, 20)] [SerializeField] private string latestRequestJson;

        public string LatestRequestJson { get { return latestRequestJson; } }

        private void Awake() { if (gameManager == null) gameManager = GameManager.Instance; }
        public void SelectHero(string heroId) { selectedHeroId = heroId ?? string.Empty; }
        public void RequestRankUpForSelectedHero()
        {
            if (gameManager == null || string.IsNullOrWhiteSpace(selectedHeroId)) return;
            latestRequestJson = gameManager.RankUp(selectedHeroId);
        }
        public EvolutionDecisionResult ValidateJson(string decisionJson) { return gameManager == null ? new EvolutionDecisionResult() : gameManager.ValidateEvolutionJson(decisionJson); }
        public EvolutionDecisionResult ApplyJson(string decisionJson) { return gameManager == null ? new EvolutionDecisionResult() : gameManager.ApplyEvolutionJson(decisionJson); }
    }
}
