using MakeMeHero.Core;
using UnityEngine;

namespace MakeMeHero.Game
{
    /// <summary>Attach to Soldier, Archer, Mage, or Healer prefabs.</summary>
    public sealed class HeroView : CharacterView
    {
        [SerializeField] private HeroClass heroClass;

        public HeroClass HeroClass { get { return heroClass; } }

        public void Bind(Hero hero)
        {
            heroClass = hero.Class;
            base.Bind(hero);
        }
    }
}
