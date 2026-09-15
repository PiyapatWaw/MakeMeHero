namespace MakeMeHero.Core
{
    /// <summary>Phase 1 balance policy. Keep this separate so rank rewards can be tuned without changing Hero.</summary>
    public sealed class FixedDevelopmentPointPolicy : IDevelopmentPointPolicy
    {
        public int PointsAwardedForRank(int newRank) { return 5; }
    }
}
