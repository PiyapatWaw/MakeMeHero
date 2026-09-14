using System;

namespace MakeMeHero.Core
{
    public static class HeroFactory
    {
        public static Hero Create(HeroClass heroClass, string id, string displayName, HeroStats stats)
        {
            switch (heroClass)
            {
                case HeroClass.Soldier: return new Soldier(id, displayName, stats);
                case HeroClass.Archer: return new Archer(id, displayName, stats);
                case HeroClass.Mage: return new Mage(id, displayName, stats);
                case HeroClass.Healer: return new Healer(id, displayName, stats);
                default: throw new ArgumentOutOfRangeException("heroClass");
            }
        }
    }
}
