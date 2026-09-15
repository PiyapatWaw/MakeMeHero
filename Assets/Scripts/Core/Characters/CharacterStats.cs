using System;

namespace MakeMeHero.Core
{
    /// <summary>Shared immutable combat-stat value for every Character, including Heroes and Monsters.</summary>
    public struct CharacterStats
    {
        public const decimal MinimumAttackInterval = .20m;
        public CharacterStats(decimal maximumHp, decimal attackDamage, decimal attackInterval)
        {
            if (maximumHp <= 0m) throw new ArgumentOutOfRangeException("maximumHp");
            if (attackDamage < 0m) throw new ArgumentOutOfRangeException("attackDamage");
            if (attackInterval < MinimumAttackInterval) throw new ArgumentOutOfRangeException("attackInterval");
            MaximumHp = maximumHp; AttackDamage = attackDamage; AttackInterval = attackInterval;
        }
        public decimal MaximumHp { get; private set; }
        public decimal AttackDamage { get; private set; }
        public decimal AttackInterval { get; private set; }
        public CharacterStats Apply(CharacterStatDelta delta)
        {
            return new CharacterStats(
                MaximumHp + delta.MaximumHp,
                Math.Max(0m, AttackDamage + delta.AttackDamage),
                Math.Max(MinimumAttackInterval, AttackInterval + delta.AttackInterval));
        }
    }

    /// <summary>Additive stat change produced by evolution, tuning modifiers, or future effects.</summary>
    public struct CharacterStatDelta
    {
        public CharacterStatDelta(decimal maximumHp, decimal attackDamage, decimal attackInterval)
        { MaximumHp = maximumHp; AttackDamage = attackDamage; AttackInterval = attackInterval; }
        public decimal MaximumHp { get; private set; }
        public decimal AttackDamage { get; private set; }
        public decimal AttackInterval { get; private set; }
        public static CharacterStatDelta operator +(CharacterStatDelta left, CharacterStatDelta right)
        { return new CharacterStatDelta(left.MaximumHp + right.MaximumHp, left.AttackDamage + right.AttackDamage, left.AttackInterval + right.AttackInterval); }
    }
}
