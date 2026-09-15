using System.Collections.Generic;

namespace MakeMeHero.Core
{
    public sealed class UnitCombatResult
    {
        public UnitCombatResult(UnitStateSnapshot before)
        {
            UnitId = before.UnitId; Kind = before.Kind; Archetype = before.Archetype; RankStars = before.RankStars; HpBeforeBattle = before.Hp;
            SkillUsage = new Dictionary<string, int>(); SupportContributions = new List<SupportContribution>();
        }
        public string UnitId { get; private set; }
        public UnitKind Kind { get; private set; }
        public string Archetype { get; private set; }
        public int RankStars { get; private set; }
        public decimal HpBeforeBattle { get; private set; }
        public decimal HpAfterBattle { get; private set; }
        public bool Survived { get; private set; }
        public string PositionAfterBattle { get; private set; }
        public decimal DamageDealt { get; private set; }
        public decimal DamageTaken { get; private set; }
        public decimal DamageByNormal { get; private set; }
        public decimal DamageBySkill { get; private set; }
        public decimal DamageMitigated { get; private set; }
        public decimal HealingDone { get; private set; }
        public decimal HealingReceived { get; private set; }
        public int Kills { get; private set; }
        public int Assists { get; private set; }
        public int NormalAttackCount { get; private set; }
        public string KillerId { get; private set; }
        public string DeathCause { get; private set; }
        public IDictionary<string, int> SkillUsage { get; private set; }
        public IList<SupportContribution> SupportContributions { get; private set; }
        public void RecordAttack() { NormalAttackCount++; }
        public void RecordSkill(SkillId? skillId) { if (skillId.HasValue) { var key = skillId.Value.ToString(); SkillUsage[key] = SkillUsage.ContainsKey(key) ? SkillUsage[key] + 1 : 1; } }
        public void RecordDamageDealt(decimal amount, CombatActionKind? action) { DamageDealt += amount; if (action == CombatActionKind.Skill) DamageBySkill += amount; else DamageByNormal += amount; }
        public void RecordDamageTaken(decimal amount) { DamageTaken += amount; }
        public void RecordHealDone(decimal amount) { HealingDone += amount; }
        public void RecordHealReceived(decimal amount) { HealingReceived += amount; }
        public void RecordMitigated(decimal amount) { DamageMitigated += amount; }
        public void RecordKill() { Kills++; }
        public void RecordAssist() { Assists++; }
        public void RecordSupport(SupportContribution contribution) { SupportContributions.Add(contribution); }
        public void RecordDeath(string killerId, string deathCause) { KillerId = killerId; DeathCause = deathCause; }
        public void Complete(UnitStateSnapshot after) { HpAfterBattle = after.Hp; Survived = !after.IsDead; PositionAfterBattle = after.Position; }
    }

    public sealed class SupportContribution
    {
        public SupportContribution(string sourceId, string targetId, SkillId skillId, decimal amount)
        { SourceId = sourceId; TargetId = targetId; SkillId = skillId; Amount = amount; }
        public string SourceId { get; private set; }
        public string TargetId { get; private set; }
        public SkillId SkillId { get; private set; }
        public decimal Amount { get; private set; }
    }
}
