using System;
using System.Collections.Generic;
using System.Linq;

namespace MakeMeHero.Core
{
    public sealed class Run
    {
        private readonly CombatTuning _tuning;
        private readonly IEncounterScalingPolicy _scaling;
        private readonly IRandomSource _random;
        private readonly List<Hero> _heroes = new List<Hero>();
        private readonly List<Wolf> _wolves = new List<Wolf>();
        private readonly List<CombatEvent> _events = new List<CombatEvent>();
        private readonly List<ExperienceSnapshot> _snapshots = new List<ExperienceSnapshot>();
        private readonly Dictionary<GridPosition, decimal> _guardExpiresAt = new Dictionary<GridPosition, decimal>();
        private int _heroSequence; private int _wolfSequence; private int _spawned; private int _totalToSpawn;
        private decimal _nextSpawnAt;

        public Run(string id, CombatTuning tuning, IEncounterScalingPolicy scaling, IRandomSource random)
        { Id = id; _tuning = tuning; _scaling = scaling; _random = random; CityHp = 25m; Gold = 100; Day = 1; Phase = RunPhase.Standby; AddStartingRoster(); }
        public string Id { get; private set; }
        public int Day { get; private set; }
        public int Gold { get; private set; }
        public decimal CityHp { get; private set; }
        public RunPhase Phase { get; private set; }
        public decimal BattleTime { get; private set; }
        public IEnumerable<Hero> Heroes { get { return _heroes; } }
        public IEnumerable<Wolf> Wolves { get { return _wolves; } }
        public IEnumerable<CombatEvent> Events { get { return _events; } }
        public IEnumerable<ExperienceSnapshot> ExperienceSnapshots { get { return _snapshots; } }

        public Hero Recruit(HeroClass heroClass)
        { EnsureStandby(); if (Gold < 10) throw new InvalidOperationException("Not enough Gold."); if (_heroes.Count(x => !x.IsDead && !x.Position.HasValue) >= 30) throw new InvalidOperationException("Reserve is full."); Gold -= 10; var hero = CreateHero(heroClass); _events.Add(new CombatEvent(EventType.Recruited, Day, BattleTime, hero.Id, detail: hero.Class.ToString())); return hero; }
        public void Deploy(string heroId, GridPosition tile)
        { EnsureStandby(); EnsureTile(tile); var hero = HeroById(heroId); if (hero.IsDead) throw new InvalidOperationException("Dead heroes cannot deploy."); if (_heroes.Count(x => x.Position.HasValue && x.Position.Value.Equals(tile) && !x.IsDead) >= 3) throw new InvalidOperationException("Tile is full of heroes."); hero.Position = tile; _events.Add(new CombatEvent(EventType.Deployed, Day, BattleTime, hero.Id, detail: tile.ToString())); }
        public void Undeploy(string heroId)
        { EnsureStandby(); var hero = HeroById(heroId); hero.Position = null; _events.Add(new CombatEvent(EventType.Undeployed, Day, BattleTime, hero.Id)); }
        public void RankUp(string heroId)
        { EnsureStandby(); var hero = HeroById(heroId); hero.RankUp(); _events.Add(new CombatEvent(EventType.RankUp, Day, BattleTime, hero.Id, value: hero.RankStars)); }
        public void StartDay()
        { EnsureStandby(); Phase = RunPhase.Battle; BattleTime = 0m; _wolves.Clear(); _guardExpiresAt.Clear(); _spawned = 0; _totalToSpawn = _scaling.MonsterCountForDay(Day); _nextSpawnAt = 0m; foreach (var hero in _heroes.Where(x => !x.IsDead && x.Position.HasValue)) { hero.AttackReadyAt = 0m; hero.SkillReadyAt = 0m; } _events.Add(new CombatEvent(EventType.DayStarted, Day, 0m)); }
        public void Advance(decimal seconds)
        {
            if (Phase != RunPhase.Battle) throw new InvalidOperationException("Battle is not active.");
            var end = BattleTime + seconds;
            while (Phase == RunPhase.Battle && BattleTime <= end)
            {
                var next = NextTimestamp(end);
                if (!next.HasValue) { BattleTime = end; break; }
                BattleTime = next.Value;
                SpawnDueWolves(); ResolveCombat(); MoveDueWolves(); FinishIfComplete();
                if (BattleTime == end) break;
            }
        }
        private decimal? NextTimestamp(decimal end)
        {
            var candidates = new List<decimal>();
            if (_spawned < _totalToSpawn && _nextSpawnAt <= end) candidates.Add(_nextSpawnAt);
            candidates.AddRange(_wolves.Where(x => !x.IsDead && x.MoveReadyAt <= end).Select(x => x.MoveReadyAt));
            candidates.AddRange(_heroes.Select(NextHeroActionAt).Where(x => x.HasValue).Select(x => x.Value));
            candidates.AddRange(_wolves.Select(NextWolfActionAt).Where(x => x.HasValue).Select(x => x.Value));
            var due = candidates.Where(x => x >= BattleTime && x <= end).ToList();
            return due.Count == 0 ? (decimal?)null : due.Min();
        }
        private decimal? NextHeroActionAt(Hero hero)
        {
            if (hero.IsDead || !hero.Position.HasValue) return null;
            var canCast = CanCast(hero);
            var canAttack = _wolves.Any(x => !x.IsDead && SameTile(x, hero));
            if (canCast && hero.SkillReadyAt <= BattleTime) return BattleTime;
            if (canAttack && hero.AttackReadyAt <= BattleTime) return BattleTime;
            var candidates = new List<decimal>();
            if (canCast) candidates.Add(hero.SkillReadyAt);
            if (canAttack) candidates.Add(hero.AttackReadyAt);
            return candidates.Count == 0 ? (decimal?)null : candidates.Min();
        }
        private decimal? NextWolfActionAt(Wolf wolf)
        {
            if (wolf.IsDead || !wolf.Position.HasValue || !_heroes.Any(x => !x.IsDead && SameTile(x, wolf))) return null;
            return wolf.AttackReadyAt <= BattleTime ? BattleTime : wolf.AttackReadyAt;
        }
        private void SpawnDueWolves()
        { while (_spawned < _totalToSpawn && _nextSpawnAt <= BattleTime) { _spawned++; var wolf = new Wolf("Wolf #" + (++_wolfSequence), _tuning.Wolf); wolf.AttackReadyAt = BattleTime; wolf.MoveReadyAt = BattleTime + 1m; _wolves.Add(wolf); _events.Add(new CombatEvent(EventType.MonsterSpawned, Day, BattleTime, wolf.Id)); _nextSpawnAt += _scaling.SpawnInterval; } }
        private void ResolveCombat()
        {
            var intents = new List<ActionIntent>();
            foreach (var hero in _heroes.Where(x => !x.IsDead && x.Position.HasValue).ToList()) PlanHero(hero, intents);
            foreach (var wolf in _wolves.Where(x => !x.IsDead && x.Position.HasValue).ToList()) PlanWolf(wolf, intents);
            foreach (var intent in intents) intent.Apply(this);
            RecordDeaths();
        }
        private void PlanHero(Hero hero, List<ActionIntent> intents)
        {
            if (hero.SkillReadyAt <= BattleTime && CanCast(hero)) { intents.Add(new SkillIntent(hero)); hero.SkillReadyAt = BattleTime + hero.Skill.Cooldown; return; }
            if (hero.AttackReadyAt <= BattleTime) { var target = LowestHp(_wolves.Where(x => !x.IsDead && SameTile(x, hero))); if (target != null) { intents.Add(new DamageIntent(hero, target, hero.AttackDamage, false)); hero.AttackReadyAt = BattleTime + hero.AttackInterval; } }
        }
        private void PlanWolf(Wolf wolf, List<ActionIntent> intents)
        { if (wolf.AttackReadyAt <= BattleTime) { var target = LowestHp(_heroes.Where(x => !x.IsDead && SameTile(x, wolf))); if (target != null) { intents.Add(new DamageIntent(wolf, target, wolf.AttackDamage, false)); wolf.AttackReadyAt = BattleTime + wolf.AttackInterval; } } }
        private bool CanCast(Hero hero)
        { if (hero.Skill.Id == SkillId.Guard) return true; if (hero.Skill.Id == SkillId.Heal) return _heroes.Any(x => !x.IsDead && SameTile(x, hero) && x.Hp < x.MaximumHp); return _wolves.Any(x => !x.IsDead && SameTile(x, hero)); }
        private void MoveDueWolves()
        { foreach (var wolf in _wolves.Where(x => !x.IsDead && x.MoveReadyAt <= BattleTime).ToList()) { if (wolf.Position.HasValue && _heroes.Any(x => !x.IsDead && SameTile(x, wolf))) continue; MoveWolf(wolf); wolf.MoveReadyAt = BattleTime + 1m; } }
        private void MoveWolf(Wolf wolf)
        {
            if (!wolf.Position.HasValue) { if (CanEnter(GridPosition.SpawnGate)) SetPosition(wolf, GridPosition.SpawnGate); return; }
            if (wolf.Position.Value.Equals(GridPosition.CityGate)) { CityHp = Math.Max(0m, CityHp - wolf.CityDamage); wolf.ReceiveDamage(wolf.Hp); _events.Add(new CombatEvent(EventType.CityDamaged, Day, BattleTime, wolf.Id, value: wolf.CityDamage)); if (CityHp == 0m) { Phase = RunPhase.Lost; _events.Add(new CombatEvent(EventType.RunLost, Day, BattleTime)); } return; }
            var choices = Neighbors(wolf.Position.Value).Where(CanEnter).ToList();
            if (choices.Count == 0) return;
            var distance = choices.Min(DistanceToCity);
            var shortest = choices.Where(x => DistanceToCity(x) == distance).ToList();
            SetPosition(wolf, shortest[_random.Next(shortest.Count)]);
        }
        private void SetPosition(Wolf wolf, GridPosition position) { var from = wolf.Position.HasValue ? wolf.Position.Value.ToString() : "Spawn"; wolf.Position = position; _events.Add(new CombatEvent(EventType.MonsterMoved, Day, BattleTime, wolf.Id, detail: from + "->" + position)); }
        private bool CanEnter(GridPosition tile) { var heroCount = _heroes.Count(x => !x.IsDead && x.Position.HasValue && x.Position.Value.Equals(tile)); return heroCount == 0 || _wolves.Count(x => !x.IsDead && x.Position.HasValue && x.Position.Value.Equals(tile)) < heroCount; }
        private static IEnumerable<GridPosition> Neighbors(GridPosition p) { if (p.Column > 0) yield return new GridPosition(p.Column - 1, p.Row); if (p.Column < 2) yield return new GridPosition(p.Column + 1, p.Row); if (p.Row > 0) yield return new GridPosition(p.Column, p.Row - 1); if (p.Row < 2) yield return new GridPosition(p.Column, p.Row + 1); }
        private static int DistanceToCity(GridPosition p) { return p.Column + 1 + Math.Abs(p.Row - 1); }
        private void RecordDeaths() { foreach (var unit in _heroes.Cast<Character>().Concat(_wolves).Where(x => x.IsDead).ToList()) if (!_events.Any(x => x.Type == EventType.UnitDied && x.TargetId == unit.Id)) _events.Add(new CombatEvent(EventType.UnitDied, Day, BattleTime, targetId: unit.Id)); }
        private void FinishIfComplete()
        { if (_spawned != _totalToSpawn || _wolves.Any(x => !x.IsDead)) return; Phase = RunPhase.EndOfDay; foreach (var hero in _heroes.Where(x => !x.IsDead && x.Position.HasValue)) { WriteSnapshot(hero); hero.AwardSurvivalDay(); hero.Restore(); } Gold += 10 + 5 * (Day - 1); _events.Add(new CombatEvent(EventType.DayCompleted, Day, BattleTime, value: Gold)); Day++; Phase = RunPhase.Standby; }
        private void WriteSnapshot(Hero hero)
        { var e = _events.Where(x => x.Day == Day).ToList(); decimal dealt = e.Where(x => x.Type == EventType.Damage && x.SourceId == hero.Id).Sum(x => x.Value); decimal taken = e.Where(x => x.Type == EventType.Damage && x.TargetId == hero.Id).Sum(x => x.Value); decimal healed = e.Where(x => x.Type == EventType.Heal && x.SourceId == hero.Id).Sum(x => x.Value); int kills = e.Count(x => x.Type == EventType.Kill && x.SourceId == hero.Id); int casts = e.Count(x => x.Type == EventType.SkillCast && x.SourceId == hero.Id); _snapshots.Add(new ExperienceSnapshot(Day, hero.Id, dealt, taken, healed, kills, casts)); }
        private Hero CreateHero(HeroClass type) { var hero = HeroFactory.Create(type, type + "-" + (++_heroSequence), type + " #" + _heroSequence, _tuning.Hero(type)); _heroes.Add(hero); return hero; }
        private void AddStartingRoster() { foreach (HeroClass type in Enum.GetValues(typeof(HeroClass))) CreateHero(type); }
        private Hero HeroById(string id) { return _heroes.Single(x => x.Id == id); }
        private static bool SameTile(Character a, Character b) { return a.Position.HasValue && b.Position.HasValue && a.Position.Value.Equals(b.Position.Value); }
        private static T LowestHp<T>(IEnumerable<T> units) where T : Character { return units.OrderBy(x => x.Hp).ThenBy(x => x.Id).FirstOrDefault(); }
        private static Hero LowestHpPercent(IEnumerable<Hero> units) { return units.OrderBy(x => x.Hp / x.MaximumHp).ThenBy(x => x.Id).FirstOrDefault(); }
        private void EnsureStandby() { if (Phase != RunPhase.Standby) throw new InvalidOperationException("This command requires Standby Phase."); }
        private static void EnsureTile(GridPosition tile) { if (tile.Column < 0 || tile.Column > 2 || tile.Row < 0 || tile.Row > 2) throw new ArgumentOutOfRangeException("tile"); }

        private abstract class ActionIntent { public abstract void Apply(Run run); }
        private sealed class DamageIntent : ActionIntent
        { private readonly Character _source; private readonly Character _target; private readonly decimal _amount; private readonly bool _skill;
          public DamageIntent(Character source, Character target, decimal amount, bool skill) { _source = source; _target = target; _amount = amount; _skill = skill; }
          public override void Apply(Run run) { run._events.Add(new CombatEvent(_skill ? EventType.SkillCast : EventType.Attack, run.Day, run.BattleTime, _source.Id, _target.Id)); var amount = _amount; if (_target is Hero && _target.Position.HasValue && run._guardExpiresAt.ContainsKey(_target.Position.Value) && run._guardExpiresAt[_target.Position.Value] > run.BattleTime) amount *= .5m; var alive = !_target.IsDead; _target.ReceiveDamage(amount); run._events.Add(new CombatEvent(EventType.Damage, run.Day, run.BattleTime, _source.Id, _target.Id, amount)); if (alive && _target.IsDead) run._events.Add(new CombatEvent(EventType.Kill, run.Day, run.BattleTime, _source.Id, _target.Id)); } }
        private sealed class SkillIntent : ActionIntent
        { private readonly Hero _hero; public SkillIntent(Hero hero) { _hero = hero; }
          public override void Apply(Run run) { run._events.Add(new CombatEvent(EventType.SkillCast, run.Day, run.BattleTime, _hero.Id, detail: _hero.Skill.Id.ToString())); if (_hero.Skill.Id == SkillId.Guard) { run._guardExpiresAt[_hero.Position.Value] = run.BattleTime + _hero.Skill.Duration; return; } if (_hero.Skill.Id == SkillId.Heal) { var target = LowestHpPercent(run._heroes.Where(x => !x.IsDead && SameTile(x, _hero) && x.Hp < x.MaximumHp)); if (target != null) { target.ReceiveHeal(_hero.Skill.Magnitude); run._events.Add(new CombatEvent(EventType.Heal, run.Day, run.BattleTime, _hero.Id, target.Id, _hero.Skill.Magnitude)); } return; } foreach (var target in run._wolves.Where(x => !x.IsDead && SameTile(x, _hero)).ToList()) new DamageIntent(_hero, target, _hero.Skill.Magnitude, true).Apply(run); } }
    }

}
