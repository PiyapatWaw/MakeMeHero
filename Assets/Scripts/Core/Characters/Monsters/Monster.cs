namespace MakeMeHero.Core
{
    public abstract class Monster : Character
    {
        protected Monster(string id, string name, WolfStats stats) : base(id, name, stats.CharacterStats)
        { CityDamage = stats.CityDamage; MoveReadyAt = stats.FirstMoveAt; }
        public decimal CityDamage { get; private set; }
        public decimal MoveReadyAt { get; set; }
    }
}
