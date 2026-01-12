namespace Core.Interfaces
{
    public interface IFrameClock
    {
        int CurrentFrame { get; }
        event System.Action<int> OnFrameTick;
    }
}