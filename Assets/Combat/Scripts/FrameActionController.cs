using Core.Interfaces;

namespace Combat.Scripts
{
    public class FrameActionController
    {
        private readonly IFrameClock clock;
        private int actionStartFrame;

        public FrameActionController(IFrameClock clock)
        {
            this.clock = clock;
            clock.OnFrameTick += OnFrame;
        }

        void OnFrame(int frame)
        {
            int elapsed = frame - actionStartFrame;
            // frame-based logic here
        }
    }

}
