using System;

namespace MakeMeHero.Core
{
    public abstract class Character
    {
        protected Character(string id, string displayName, decimal maximumHp, decimal attackDamage, decimal attackInterval)
        { Id = id; DisplayName = displayName; MaximumHp = maximumHp; Hp = maximumHp; AttackDamage = attackDamage; AttackInterval = attackInterval; }

        public string Id { get; private set; }
        public string DisplayName { get; private set; }
        public decimal MaximumHp { get; private set; }
        public decimal Hp { get; private set; }
        public decimal AttackDamage { get; private set; }
        public decimal AttackInterval { get; private set; }
        public decimal AttackReadyAt { get; set; }
        public GridPosition? Position { get; set; }
        public bool IsDead { get { return Hp <= 0m; } }
        public void Restore() { Hp = MaximumHp; }
        public void ReceiveDamage(decimal amount) { Hp = Math.Max(0m, Hp - amount); }
        public void ReceiveHeal(decimal amount) { Hp = Math.Min(MaximumHp, Hp + amount); }
        internal void IncreaseMaximumHp(decimal amount) { MaximumHp += amount; Hp += amount; }
        internal void IncreaseAttackDamage(decimal amount) { AttackDamage += amount; }
        internal void ReduceAttackInterval(decimal amount, decimal minimum) { AttackInterval = Math.Max(minimum, AttackInterval - amount); }
    }
}
