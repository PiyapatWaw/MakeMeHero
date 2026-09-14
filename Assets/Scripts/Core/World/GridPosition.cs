using System;

namespace MakeMeHero.Core
{
    public struct GridPosition : IEquatable<GridPosition>
    {
        public GridPosition(int column, int row) { Column = column; Row = row; }
        public int Column { get; private set; }
        public int Row { get; private set; }
        public bool Equals(GridPosition other) { return Column == other.Column && Row == other.Row; }
        public override bool Equals(object obj) { return obj is GridPosition && Equals((GridPosition)obj); }
        public override int GetHashCode() { return (Column * 397) ^ Row; }
        public override string ToString() { return Column + "," + Row; }
        public static readonly GridPosition CityGate = new GridPosition(0, 1);
        public static readonly GridPosition SpawnGate = new GridPosition(2, 1);
    }
}
