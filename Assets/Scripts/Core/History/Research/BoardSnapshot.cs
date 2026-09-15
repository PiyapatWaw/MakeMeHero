using System.Collections.Generic;
using System.Linq;

namespace MakeMeHero.Core
{
    public sealed class BoardSnapshot
    {
        private BoardSnapshot(Run run)
        {
            CityHp = run.CityHp; Gold = run.Gold; Phase = run.Phase; Day = run.Day;
            Units = run.Heroes.Cast<Character>().Concat(run.Wolves).Select(x => new UnitStateSnapshot(x)).ToList();
            Reserve = Units.Where(x => x.Placement == "Reserve").ToList();
            Rooms = new List<RoomSnapshot>();
            for (var row = 0; row < 3; row++) for (var column = 0; column < 3; column++)
            {
                var position = new GridPosition(column, row).ToString();
                Rooms.Add(new RoomSnapshot(position, Units.Where(x => x.Position == position).ToList()));
            }
        }
        public int Day { get; private set; }
        public decimal CityHp { get; private set; }
        public int Gold { get; private set; }
        public RunPhase Phase { get; private set; }
        public IList<UnitStateSnapshot> Units { get; private set; }
        public IList<UnitStateSnapshot> Reserve { get; private set; }
        public IList<RoomSnapshot> Rooms { get; private set; }
        public static BoardSnapshot Capture(Run run) { return new BoardSnapshot(run); }
    }

    public sealed class RoomSnapshot
    {
        public RoomSnapshot(string position, IList<UnitStateSnapshot> members) { Position = position; Members = members; UnitIds = members.Select(x => x.UnitId).ToList(); }
        public string Position { get; private set; }
        public IList<string> UnitIds { get; private set; }
        public IList<UnitStateSnapshot> Members { get; private set; }
    }
}
