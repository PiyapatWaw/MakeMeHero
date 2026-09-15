using System.Collections.Generic;

namespace MakeMeHero.Core
{
    public sealed class UnitStateSnapshot
    {
        public UnitStateSnapshot(Character character)
        {
            UnitId = character.Id; DisplayName = character.DisplayName; MaximumHp = character.MaximumHp; Hp = character.Hp;
            AttackDamage = character.AttackDamage; AttackInterval = character.AttackInterval; Position = character.Position.HasValue ? character.Position.Value.ToString() : null; IsDead = character.IsDead;
            var hero = character as Hero;
            Kind = hero == null ? UnitKind.Monster : UnitKind.Hero;
            Archetype = hero == null ? character.GetType().Name : hero.Class.ToString();
            RankStars = hero == null ? 0 : hero.RankStars;
            DevelopmentPoints = hero == null ? 0 : hero.DevelopmentPoints;
            Skills = hero == null ? new List<string>() : new List<string> { hero.Skill.Id.ToString() };
            Placement = IsDead ? "Defeated" : Position == null ? "Reserve" : "Field";
        }
        public string UnitId { get; private set; }
        public string DisplayName { get; private set; }
        public UnitKind Kind { get; private set; }
        public string Archetype { get; private set; }
        public int RankStars { get; private set; }
        public int DevelopmentPoints { get; private set; }
        public decimal MaximumHp { get; private set; }
        public decimal Hp { get; private set; }
        public decimal AttackDamage { get; private set; }
        public decimal AttackInterval { get; private set; }
        public string Position { get; private set; }
        public string Placement { get; private set; }
        public bool IsDead { get; private set; }
        public IList<string> Skills { get; private set; }
    }
}
