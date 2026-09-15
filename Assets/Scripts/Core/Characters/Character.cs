using System;

namespace MakeMeHero.Core
{
    public abstract class Character
    {
        private CharacterStats _stats;
        protected Character(string id, string displayName, CharacterStats stats)
        { Id = id; DisplayName = displayName; _stats = stats; Hp = stats.MaximumHp; }

        public string Id { get; private set; }
        public string DisplayName { get; private set; }
        public CharacterStats Stats { get { return _stats; } }
        public decimal MaximumHp { get { return _stats.MaximumHp; } }
        public decimal Hp { get; private set; }
        public decimal AttackDamage { get { return _stats.AttackDamage; } }
        public decimal AttackInterval { get { return _stats.AttackInterval; } }
        public decimal AttackReadyAt { get; set; }
        public GridPosition? Position { get; set; }
        public bool IsDead { get { return Hp <= 0m; } }
        public void Restore() { Hp = MaximumHp; }
        public void ReceiveDamage(decimal amount) { Hp = Math.Max(0m, Hp - amount); }
        public void ReceiveHeal(decimal amount) { Hp = Math.Min(MaximumHp, Hp + amount); }
        internal void ApplyStatDelta(CharacterStatDelta delta)
        {
            var previousMaximumHp = _stats.MaximumHp;
            _stats = _stats.Apply(delta);
            Hp = Math.Min(_stats.MaximumHp, Hp + (_stats.MaximumHp - previousMaximumHp));
        }
    }
}
