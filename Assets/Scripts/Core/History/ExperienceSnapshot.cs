namespace MakeMeHero.Core
{
    public sealed class ExperienceSnapshot
    {
        public ExperienceSnapshot(int day, string heroId, decimal dealt, decimal taken, decimal healed, int kills, int skillCasts)
        { Day = day; HeroId = heroId; DamageDealt = dealt; DamageTaken = taken; HealingDone = healed; Kills = kills; SkillCasts = skillCasts; }

        public int Day { get; private set; }
        public string HeroId { get; private set; }
        public decimal DamageDealt { get; private set; }
        public decimal DamageTaken { get; private set; }
        public decimal HealingDone { get; private set; }
        public int Kills { get; private set; }
        public int SkillCasts { get; private set; }
    }
}
