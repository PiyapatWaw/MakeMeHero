using System;

namespace MakeMeHero.Core
{
    public sealed class SystemRandomSource : IRandomSource
    {
        private readonly Random _random;
        public SystemRandomSource() : this(Environment.TickCount) { }
        public SystemRandomSource(int seed) { _random = new Random(seed); }
        public int Next(int exclusiveMaximum) { return _random.Next(exclusiveMaximum); }
    }
}
