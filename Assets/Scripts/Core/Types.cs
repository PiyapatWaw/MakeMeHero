namespace MakeMeHero.Core
{
    public enum RunPhase { Standby, Battle, EndOfDay, Lost, Abandoned }
    public enum HeroClass { Soldier, Archer, Mage, Healer }
    public enum EventType { Recruited, Deployed, Undeployed, RankUp, EvolutionApplied, DayStarted, MonsterSpawned, MonsterMoved, Attack, SkillCast, Damage, DamageMitigated, Heal, Kill, UnitDied, CityDamaged, BattleCompleted, DayCompleted, RunLost, RunAbandoned }
    public enum SkillId { Guard, Volley, ArcaneBurst, Heal }
    public enum CombatActionKind { NormalAttack, Skill, Environmental }
    public enum RunEndReason { Lost, Abandoned }
    public enum UnitKind { Hero, Monster }
    public enum EvolvableStat { MaximumHp, AttackDamage, AttackInterval }
    public enum EvolutionDecisionSource { Human, RuleBased, ManualExternalLLM, ExternalAgent, ReinforcementLearning, Evolutionary, Unknown }
    public enum CharacterHistoryEndReason { Died, RunLost, RunAbandoned }
}
