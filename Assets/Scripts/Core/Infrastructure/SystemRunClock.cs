using System;

namespace MakeMeHero.Core
{
    public sealed class SystemRunClock : IRunClock
    {
        public DateTimeOffset UtcNow { get { return DateTimeOffset.UtcNow; } }
    }
}
