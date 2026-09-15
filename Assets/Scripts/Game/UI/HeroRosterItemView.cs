using System;
using MakeMeHero.Core;
using UnityEngine;
using UnityEngine.UI;

namespace MakeMeHero.Game
{
    /// <summary>Single reusable roster row. The prefab may use only a Button and optional Text fields.</summary>
    public sealed class HeroRosterItemView : MonoBehaviour
    {
        [SerializeField] private Button selectButton;
        [SerializeField] private Text nameText;
        [SerializeField] private Text detailsText;
        private string _heroId;

        public event Action<string> Selected;

        private void Awake() { if (selectButton != null) selectButton.onClick.AddListener(Select); }
        public void Bind(Hero hero)
        {
            _heroId = hero.Id;
            if (nameText != null) nameText.text = hero.DisplayName;
            if (detailsText != null) detailsText.text = string.Format("{0}  ★{1}  EXP {2}/5  DP {3}", hero.Class, hero.RankStars, hero.UnspentRankExperience, hero.DevelopmentPoints);
            if (selectButton != null) selectButton.interactable = !hero.IsDead;
            gameObject.SetActive(true);
        }
        private void Select() { if (Selected != null && !string.IsNullOrWhiteSpace(_heroId)) Selected(_heroId); }
    }
}
