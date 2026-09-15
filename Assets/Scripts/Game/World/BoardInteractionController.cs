using MakeMeHero.Core;
using UnityEngine;

namespace MakeMeHero.Game
{
    /// <summary>Attach once to the board. It translates Hero/tile clicks into Standby deployment commands.</summary>
    public sealed class BoardInteractionController : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;
        [SerializeField] private string selectedHeroId;

        public string SelectedHeroId { get { return selectedHeroId; } }

        private void OnEnable()
        {
            TileView.Clicked += OnTileClicked;
            HeroView.Clicked += OnHeroClicked;
        }

        private void OnDisable()
        {
            TileView.Clicked -= OnTileClicked;
            HeroView.Clicked -= OnHeroClicked;
        }

        public void SelectHero(string heroId) { selectedHeroId = heroId ?? string.Empty; }
        public void ClearSelection() { selectedHeroId = string.Empty; }

        public void UndeploySelectedHero()
        {
            if (gameManager == null || string.IsNullOrWhiteSpace(selectedHeroId)) return;
            gameManager.Undeploy(selectedHeroId);
        }

        private void OnHeroClicked(HeroView view)
        {
            if (view != null) SelectHero(view.CharacterId);
        }

        private void OnTileClicked(TileView tile)
        {
            if (gameManager == null || tile == null || string.IsNullOrWhiteSpace(selectedHeroId)) return;
            gameManager.Deploy(selectedHeroId, tile.Column, tile.Row);
        }
    }
}
