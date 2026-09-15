using System.Collections.Generic;
using System.Linq;
using MakeMeHero.Core;
using UnityEngine;

namespace MakeMeHero.Game
{
    /// <summary>Builds a selectable reserve/field roster. It delegates all game commands to controllers.</summary>
    public sealed class HeroRosterView : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;
        [SerializeField] private BoardInteractionController boardInteraction;
        [SerializeField] private RankUpRequestController rankUpController;
        [SerializeField] private Transform contentRoot;
        [SerializeField] private HeroRosterItemView itemPrefab;
        private readonly List<HeroRosterItemView> _items = new List<HeroRosterItemView>();

        private void OnEnable()
        {
            if (gameManager == null) gameManager = GameManager.Instance;
            if (gameManager != null) gameManager.RunChanged += Render;
        }
        private void Start() { if (gameManager != null) Render(gameManager.CurrentRun); }
        private void OnDisable() { if (gameManager != null) gameManager.RunChanged -= Render; }

        private void Render(Run run)
        {
            if (run == null || itemPrefab == null) return;
            var heroes = run.Heroes.OrderBy(x => x.IsDead).ThenBy(x => x.Class).ThenBy(x => x.Id).ToList();
            while (_items.Count < heroes.Count)
            {
                var item = Instantiate(itemPrefab, contentRoot == null ? transform : contentRoot);
                item.Selected += SelectHero;
                _items.Add(item);
            }
            for (var index = 0; index < _items.Count; index++)
            {
                if (index < heroes.Count) _items[index].Bind(heroes[index]);
                else _items[index].gameObject.SetActive(false);
            }
        }

        private void SelectHero(string heroId)
        {
            if (boardInteraction != null) boardInteraction.SelectHero(heroId);
            if (rankUpController != null) rankUpController.SelectHero(heroId);
        }
    }
}
