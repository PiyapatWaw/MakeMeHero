namespace MakeMeHero.Core
{
    public enum RunPhase { Standby, Battle, EndOfDay, Lost }
    public enum HeroClass { Soldier, Archer, Mage, Healer }
    public enum EventType { Recruited, Deployed, Undeployed, RankUp, DayStarted, MonsterSpawned, MonsterMoved, Attack, SkillCast, Damage, Heal, Kill, UnitDied, CityDamaged, DayCompleted, RunLost }
    public enum SkillId { Guard, Volley, ArcaneBurst, Heal }
}
