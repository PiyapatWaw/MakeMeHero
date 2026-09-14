namespace MakeMeHero.Core
{
    public interface IRandomSource
    {
        int Next(int exclusiveMaximum);
    }
}
