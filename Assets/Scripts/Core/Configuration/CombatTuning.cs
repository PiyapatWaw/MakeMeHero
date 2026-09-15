using System.Collections.Generic;

namespace MakeMeHero.Core
{
    public sealed class SkillDefinition
    { public SkillDefinition(SkillId id, decimal cooldown, decimal magnitude, decimal duration = 0m) { Id = id; Cooldown = cooldown; Magnitude = magnitude; Duration = duration; } public SkillId Id { get; private set; } public decimal Cooldown { get; private set; } public decimal Magnitude { get; private set; } public decimal Duration { get; private set; } }
    public sealed class HeroStats
    { public HeroStats(decimal hp, decimal attack, decimal attackInterval, SkillDefinition skill) { CharacterStats = new CharacterStats(hp, attack, attackInterval); Skill = skill; } public CharacterStats CharacterStats { get; private set; } public decimal Hp { get { return CharacterStats.MaximumHp; } } public decimal Attack { get { return CharacterStats.AttackDamage; } } public decimal AttackInterval { get { return CharacterStats.AttackInterval; } } public SkillDefinition Skill { get; private set; } }
    public sealed class WolfStats
    { public WolfStats(decimal hp, decimal attack, decimal attackInterval, decimal cityDamage, decimal firstMoveAt) { CharacterStats = new CharacterStats(hp, attack, attackInterval); CityDamage = cityDamage; FirstMoveAt = firstMoveAt; } public CharacterStats CharacterStats { get; private set; } public decimal Hp { get { return CharacterStats.MaximumHp; } } public decimal Attack { get { return CharacterStats.AttackDamage; } } public decimal AttackInterval { get { return CharacterStats.AttackInterval; } } public decimal CityDamage { get; private set; } public decimal FirstMoveAt { get; private set; } }
    public sealed class CombatTuning
    {
        private readonly Dictionary<HeroClass, HeroStats> _heroes; public CombatTuning(Dictionary<HeroClass, HeroStats> heroes, WolfStats wolf) { _heroes = heroes; Wolf = wolf; } public WolfStats Wolf { get; private set; } public HeroStats Hero(HeroClass heroClass) { return _heroes[heroClass]; }
        public static CombatTuning Phase0() { return new CombatTuning(new Dictionary<HeroClass, HeroStats> { { HeroClass.Soldier, new HeroStats(45m, 6m, 1m, new SkillDefinition(SkillId.Guard, 8m, .5m, 3m)) }, { HeroClass.Archer, new HeroStats(28m, 8m, .8m, new SkillDefinition(SkillId.Volley, 5m, 12m)) }, { HeroClass.Mage, new HeroStats(24m, 10m, 1.2m, new SkillDefinition(SkillId.ArcaneBurst, 6m, 18m)) }, { HeroClass.Healer, new HeroStats(32m, 3m, 1.1m, new SkillDefinition(SkillId.Heal, 5m, 14m)) } }, new WolfStats(24m, 5m, 1.3m, 1m, 1m)); }
    }
}
