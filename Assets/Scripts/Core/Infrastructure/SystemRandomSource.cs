using System;

namespace MakeMeHero.Core
{
    public sealed class SystemRandomSource : IRandomSource
    {
        private readonly Random _random = new Random();
        public int Next(int exclusiveMaximum) { return _random.Next(exclusiveMaximum); }
    }
}
