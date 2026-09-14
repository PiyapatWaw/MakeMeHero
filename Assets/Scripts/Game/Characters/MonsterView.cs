using MakeMeHero.Core;

namespace MakeMeHero.Game
{
    /// <summary>Attach to Wolf and future monster prefabs.</summary>
    public sealed class MonsterView : CharacterView
    {
        public void Bind(Monster monster)
        {
            base.Bind(monster);
        }
    }
}
