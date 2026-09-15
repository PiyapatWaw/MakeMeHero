using MakeMeHero.Core;
using UnityEngine;
using System;

namespace MakeMeHero.Game
{
    /// <summary>Attach to Soldier, Archer, Mage, or Healer prefabs.</summary>
    public sealed class HeroView : CharacterView
    {
        public static event Action<HeroView> Clicked;
        [SerializeField] private HeroClass heroClass;

        public HeroClass HeroClass { get { return heroClass; } }

        public void Bind(Hero hero)
        {
            heroClass = hero.Class;
            base.Bind(hero);
        }

        private void OnMouseUpAsButton()
        {
            if (Clicked != null) Clicked(this);
        }
    }
}
