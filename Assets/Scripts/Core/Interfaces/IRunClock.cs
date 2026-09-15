using System;

namespace MakeMeHero.Core
{
    public interface IRunClock
    {
        DateTimeOffset UtcNow { get; }
    }
}
