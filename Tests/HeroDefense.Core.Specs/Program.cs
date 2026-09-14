using System;
using System.Linq;
using MakeMeHero.Core;

internal static class Program
{
    private static void Main()
    {
        StartsWithFourReserveHeroesAndGold();
        FormationAndRecruitAreStandbyOnly();
        RankNeedsDeployedSurvivalDays();
        EmptyFieldStillResolvesEncounter();
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

    private static void RankNeedsDeployedSurvivalDays()
    {
        var run = NewRun();
        var soldier = run.Heroes.Single(x => x.Class == HeroClass.Soldier);
        Assert(!soldier.CanRankUp, "Fresh hero cannot rank up.");
        ExpectThrows(() => run.RankUp(soldier.Id));
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

    private static void Assert(bool condition, string message) { if (!condition) throw new Exception(message); }
    private static void ExpectThrows(Action action) { try { action(); } catch (InvalidOperationException) { return; } throw new Exception("Expected InvalidOperationException."); }
    private sealed class FixedRandom : IRandomSource { public int Next(int exclusiveMaximum) { return 0; } }
}
