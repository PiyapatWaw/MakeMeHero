using System;
using System.Collections.Generic;
using System.Linq;
using MakeMeHero.Core;
using MakeMeHero.Game;

internal static class Program
{
    private static void Main()
    {
        StartsWithFourReserveHeroesAndGold();
        FormationAndRecruitAreStandbyOnly();
        RankRequestNeedsUnspentExperience();
        EmptyFieldStillResolvesEncounter();
        ResearchLoggingCapturesDailySnapshotAndRunClosure();
        EvolutionDecisionIsAtomicAndRecordedForNextDay();
        EvolutionDecisionRejectsInvalidState();
        EvolutionJsonImporterRejectsUntrustedInput();
        SharedCharacterStatsAndEvolutionRequestAreConsistent();
        EvolutionRequestUsesLifetimeHistoryWindow();
        EvolutionRequestStartsAtLatestRankUpBattle();
        Console.WriteLine("HeroDefense.Core smoke specifications passed.");
    }

    private static Run NewRun()
    {
        return new Run("test", CombatTuning.Phase0(), new ExponentialEncounterScalingPolicy(), new FixedRandom());
    }

    private static void StartsWithFourReserveHeroesAndGold()
    {
        var run = NewRun();
        Assert(run.Heroes.Count() == 4, "Expected four starting heroes.");
        Assert(run.Gold == 100, "Expected 100 starting Gold.");
        Assert(run.CityHp == 25m, "Expected 25 starting City HP.");
    }

    private static void FormationAndRecruitAreStandbyOnly()
    {
        var run = NewRun();
        var soldier = run.Heroes.Single(x => x.Class == HeroClass.Soldier);
        run.Deploy(soldier.Id, GridPosition.CityGate);
        run.Recruit(HeroClass.Mage);
        Assert(run.Gold == 90, "Recruit should cost 10 Gold.");
        run.StartDay();
        ExpectThrows(() => run.Deploy(soldier.Id, new GridPosition(1, 1)));
    }

    private static void RankRequestNeedsUnspentExperience()
    {
        var run = NewRun();
        var soldier = run.Heroes.Single(x => x.Class == HeroClass.Soldier);
        Assert(!soldier.CanRankUp, "Fresh hero cannot rank up.");
        ExpectThrows(() => run.EnsureRankUpRequestEligible(soldier.Id));
    }

    private static void EmptyFieldStillResolvesEncounter()
    {
        var run = NewRun();
        run.StartDay();
        run.Advance(10m);
        Assert(run.Phase == RunPhase.Standby, "An empty field should still complete Day 1.");
        Assert(run.Day == 2, "Completed Day 1 should advance the day counter.");
        Assert(run.CityHp == 22m, "Three Day 1 wolves should each damage the City once.");
        Assert(run.Gold == 110, "A completed Day 1 should grant 10 Gold.");
    }

    private static void ResearchLoggingCapturesDailySnapshotAndRunClosure()
    {
        var repository = new InMemoryRunRepository();
        var clock = new FixedClock();
        var service = new RunApplicationService(repository, CombatTuning.Phase0(), new ExponentialEncounterScalingPolicy(), seed => new FixedRandom(), clock);
        var run = service.CreateRun(new RunStartOptions(42, "research-smoke", "test", "1.0"));
        service.Recruit(run.Id, HeroClass.Mage);
        service.StartDay(run.Id);
        service.AdvanceTime(run.Id, 10m);

        var log = service.ResearchLog(run.Id);
        Assert(log.Metadata.Seed == 42, "Research metadata should retain the supplied seed.");
        Assert(log.DailySnapshots.Count == 1, "A completed battle should produce one daily snapshot.");
        var day = log.DailySnapshots[0];
        Assert(day.Encounter.PlannedEnemyCount == 3, "Day 1 encounter plan should be captured.");
        Assert(day.DevelopmentDecisions.Count == 1 && day.DevelopmentDecisions[0].Type == EventType.Recruited, "Standby recruit should belong to the next battle preparation.");
        Assert(day.BoardBeforeBattle.Units.Count == 5, "Board before battle should include all reserve heroes.");
        Assert(day.BoardAfterBattle.CityHp == 22m, "Board after battle should preserve City damage before reward state.");
        Assert(day.StandbyStateAfterReward != null, "Post-reward Standby state should be captured separately.");
        Assert(day.UnitCombatResults.Count(x => x.Kind == UnitKind.Monster) == 3, "Each spawned Wolf should have a combat result.");
        Assert(log.CharacterHistories.Count == 5 && log.CharacterHistories["Soldier-1"].Days.Count == 1, "Each starting hero should retain a target-centric lifetime day history.");

        service.EndRun(run.Id);
        Assert(log.Metadata.EndReason == RunEndReason.Abandoned && log.Metadata.EndedAtUtc.HasValue, "Abandoned run should record terminal metadata.");
        Assert(log.CharacterHistories["Soldier-1"].EndReason == CharacterHistoryEndReason.RunAbandoned, "An ended run should close living hero lifetime histories.");
    }

    private static void EvolutionDecisionIsAtomicAndRecordedForNextDay()
    {
        var repository = new InMemoryRunRepository();
        var service = new RunApplicationService(repository, CombatTuning.Phase0(), new ExponentialEncounterScalingPolicy(), seed => new FixedRandom(), new FixedClock());
        var run = service.CreateRun(new RunStartOptions(9));
        var soldier = run.Heroes.Single(x => x.Class == HeroClass.Soldier);
        for (var experienceDay = 0; experienceDay < 5; experienceDay++) soldier.AwardSurvivalExperience();
        var request = service.RequestRankUp(run.Id, soldier.Id);
        Assert(request.AvailableDevelopmentPoints == 5 && soldier.RankStars == 1 && soldier.UnspentRankExperience == 5, "A RankUp request must not mutate the hero before a decision applies.");

        var valid = new CharacterEvolutionDecision(1, "evolution-smoke-1", run.Id, soldier.Id, run.Day,
            new List<StatAllocation> { new StatAllocation(EvolvableStat.MaximumHp, 3), new StatAllocation(EvolvableStat.AttackDamage, 2) }, null,
            new EvolutionDecisionMetadata(EvolutionDecisionSource.ManualExternalLLM, "smoke"));
        var applied = service.ApplyEvolutionDecision(run.Id, valid);
        Assert(applied.IsValid, "A valid evolution allocation should apply.");
        Assert(soldier.DevelopmentPoints == 0 && soldier.MaximumHp == 60m && soldier.AttackDamage == 8m && soldier.RankStars == 2 && soldier.UnspentRankExperience == 0, "Evolution rank-up transaction should apply deterministic stats and consume five experience.");

        var duplicate = service.ApplyEvolutionDecision(run.Id, valid);
        Assert(!duplicate.IsValid && duplicate.Errors.Any(x => x.Code == "DUPLICATE_DECISION"), "Applied decision ids must be idempotent.");
        Assert(soldier.MaximumHp == 60m && soldier.AttackDamage == 8m, "Duplicate decision must not mutate the hero.");

        var invalidSkill = new CharacterEvolutionDecision(1, "evolution-smoke-2", run.Id, soldier.Id, run.Day,
            new List<StatAllocation> { new StatAllocation(EvolvableStat.MaximumHp, 1) }, "Fortify", EvolutionDecisionMetadata.Unknown);
        var rejected = service.ApplyEvolutionDecision(run.Id, invalidSkill);
        Assert(!rejected.IsValid && rejected.Errors.Any(x => x.Code == "SKILL_SELECTION_OUT_OF_SCOPE"), "Skill selection is rejected in stat-only phase.");
        Assert(soldier.MaximumHp == 60m, "Atomic rejection must not partially apply stat changes.");

        service.StartDay(run.Id);
        service.AdvanceTime(run.Id, 10m);
        var evolutionRecord = service.ResearchLog(run.Id).DailySnapshots[0].DevelopmentDecisions.SingleOrDefault(x => x.Type == EventType.EvolutionApplied);
        Assert(evolutionRecord != null && evolutionRecord.EvolutionAudit.After.MaximumHp == 60m, "Evolution audit should enter the next day's preparation history.");
    }

    private static void EvolutionDecisionRejectsInvalidState()
    {
        var repository = new InMemoryRunRepository();
        var service = new RunApplicationService(repository, CombatTuning.Phase0(), new ExponentialEncounterScalingPolicy(), seed => new FixedRandom(), new FixedClock());
        var run = service.CreateRun(new RunStartOptions(10));
        var hero = run.Heroes.First();
        for (var day = 0; day < 5; day++) hero.AwardSurvivalExperience();
        var tooMany = new CharacterEvolutionDecision(1, "too-many", run.Id, hero.Id, run.Day, new List<StatAllocation> { new StatAllocation(EvolvableStat.MaximumHp, 6) }, null, EvolutionDecisionMetadata.Unknown);
        Assert(!service.ApplyEvolutionDecision(run.Id, tooMany).IsValid && hero.DevelopmentPoints == 0 && hero.RankStars == 1, "Too many points should reject without consuming rank experience or awarding points.");
        service.StartDay(run.Id);
        var combatPhase = new CharacterEvolutionDecision(1, "combat-phase", run.Id, hero.Id, run.Day, new List<StatAllocation> { new StatAllocation(EvolvableStat.MaximumHp, 1) }, null, EvolutionDecisionMetadata.Unknown);
        Assert(!service.ApplyEvolutionDecision(run.Id, combatPhase).IsValid && hero.MaximumHp == 45m, "Evolution must reject during combat without mutation.");
    }

    private static void EvolutionJsonImporterRejectsUntrustedInput()
    {
        var importer = new CharacterEvolutionJsonImporter();
        CharacterEvolutionDecision decision;
        var valid = importer.TryImport("{\"schemaVersion\":1,\"decisionId\":\"json-1\",\"runId\":\"run\",\"unitId\":\"Soldier-1\",\"decisionDay\":6,\"statAllocations\":[{\"stat\":\"MaximumHp\",\"points\":2}],\"metadata\":{\"source\":\"ManualExternalLLM\",\"experimentId\":\"smoke\"}}", out decision);
        Assert(valid.IsValid && decision != null && decision.StatAllocations[0].Stat == EvolvableStat.MaximumHp, "Valid external JSON should deserialize into a method-agnostic decision.");
        var invalid = importer.TryImport("{\"schemaVersion\":1,\"unexpected\":true}", out decision);
        Assert(!invalid.IsValid && invalid.Errors.Any(x => x.Code == "UNKNOWN_FIELD"), "Unknown external JSON fields must be rejected before validation.");
    }

    private static void SharedCharacterStatsAndEvolutionRequestAreConsistent()
    {
        var repository = new InMemoryRunRepository();
        var service = new RunApplicationService(repository, CombatTuning.Phase0(), new ExponentialEncounterScalingPolicy(), seed => new FixedRandom(), new FixedClock());
        var run = service.CreateRun(new RunStartOptions(11));
        var soldier = run.Heroes.Single(x => x.Class == HeroClass.Soldier);
        var wolf = new Wolf("wolf-stats", CombatTuning.Phase0().Wolf);
        Assert(soldier.Stats.MaximumHp == soldier.MaximumHp && wolf.Stats.AttackDamage == wolf.AttackDamage, "Hero and Monster should expose the same CharacterStats value type.");
        for (var day = 0; day < 5; day++) soldier.AwardSurvivalExperience();
        var request = service.CreateEvolutionRequest(run.Id, soldier.Id);
        Assert(request.AvailableDevelopmentPoints == 5 && request.CurrentStats.MaximumHp == 45m, "External evolution request should state the available five points and current shared stat block.");
        var requestJson = new CharacterEvolutionRequestJsonExporter().Serialize(request);
        Assert(requestJson.Contains("\"availableDevelopmentPoints\": 5"), "External request JSON should explicitly disclose the five allocatable points in camelCase.");
        var decision = new CharacterEvolutionDecision(1, "stats-struct", run.Id, soldier.Id, run.Day, new List<StatAllocation> { new StatAllocation(EvolvableStat.MaximumHp, 5) }, null, EvolutionDecisionMetadata.Unknown);
        Assert(service.ApplyEvolutionDecision(run.Id, decision).IsValid && soldier.Stats.MaximumHp == 70m, "Evolution allocation should create a delta and apply it through CharacterStats.");
    }

    private static void EvolutionRequestUsesLifetimeHistoryWindow()
    {
        var repository = new InMemoryRunRepository();
        var service = new RunApplicationService(repository, CombatTuning.Phase0(), new ExponentialEncounterScalingPolicy(), seed => new FixedRandom(), new FixedClock());
        var run = service.CreateRun(new RunStartOptions(12));
        var soldier = run.Heroes.Single(x => x.Class == HeroClass.Soldier);
        service.StartDay(run.Id);
        service.AdvanceTime(run.Id, 10m);
        for (var experienceDay = 0; experienceDay < 5; experienceDay++) soldier.AwardSurvivalExperience();
        var request = service.CreateEvolutionRequest(run.Id, soldier.Id);
        Assert(request.Days.Count == 1 && request.HistoryWindowStartDay == 1, "First RankUp request should include lifetime history from the hero's first day.");
        var day = request.Days[0];
        Assert(day.Standby.Reserve.Any(member => member.UnitId == soldier.Id && member.Alive), "Standby snapshot should expose an alive target in the reserve roster.");
        Assert(day.Battle != null && day.End != null && day.End.Self.UnitId == soldier.Id, "Every history day should contain standby, battle, and end snapshots.");
    }

    private static void EvolutionRequestStartsAtLatestRankUpBattle()
    {
        var repository = new InMemoryRunRepository();
        var service = new RunApplicationService(repository, CombatTuning.Phase0(), new ExponentialEncounterScalingPolicy(), seed => new FixedRandom(), new FixedClock());
        var run = service.CreateRun(new RunStartOptions(13));
        var soldier = run.Heroes.Single(x => x.Class == HeroClass.Soldier);
        service.StartDay(run.Id); service.AdvanceTime(run.Id, 10m);
        for (var experienceDay = 0; experienceDay < 5; experienceDay++) soldier.AwardSurvivalExperience();
        var first = new CharacterEvolutionDecision(1, "latest-window-first", run.Id, soldier.Id, run.Day, new List<StatAllocation> { new StatAllocation(EvolvableStat.MaximumHp, 1) }, null, EvolutionDecisionMetadata.Unknown);
        Assert(service.ApplyEvolutionDecision(run.Id, first).IsValid, "First rank-up should apply during Standby.");
        service.StartDay(run.Id); service.AdvanceTime(run.Id, 10m);
        for (var experienceDay = 0; experienceDay < 5; experienceDay++) soldier.AwardSurvivalExperience();
        var request = service.CreateEvolutionRequest(run.Id, soldier.Id);
        Assert(request.BaselineEvolution != null && request.HistoryWindowStartDay == 2 && request.Days.Count == 1 && request.Days[0].Day == 2, "Later RankUp requests must begin with the battle day where the latest rank was applied.");
    }

    private static void Assert(bool condition, string message) { if (!condition) throw new Exception(message); }
    private static void ExpectThrows(Action action) { try { action(); } catch (InvalidOperationException) { return; } throw new Exception("Expected InvalidOperationException."); }
    private sealed class FixedRandom : IRandomSource { public int Next(int exclusiveMaximum) { return 0; } }
    private sealed class FixedClock : IRunClock { public DateTimeOffset UtcNow { get { return new DateTimeOffset(2026, 9, 15, 0, 0, 0, TimeSpan.Zero); } } }
}
