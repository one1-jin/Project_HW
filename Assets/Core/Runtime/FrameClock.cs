using System;
using Core.Interfaces;
using UnityEngine;

namespace Core.Runtime
{
    using System;

    public sealed class FrameClock : IFrameClock
    {
        // ===== Singleton =====
        public static FrameClock Instance { get; } = new FrameClock();

        // ===== Config =====
        public const int FPS = 60;
        public const float FRAME_TIME = 1f / FPS;

        // ===== State =====
        public int CurrentFrame { get; private set; }
        public event Action<int> OnFrameTick;

        private float accumulator;
        private bool isUpdating;
        private int lastUnityFrameTicked = -1;

        private FrameClock() { }

        // ===== Called by Runtime Runner ONLY =====
        public void Update(float deltaTime)
        {
            // Prevent double Update in same Unity frame
            if (isUpdating)
            {
                Debug.LogError(
                    "[FrameClock] Duplicate Update call detected"
                );
                return;
            }

            isUpdating = true;

            accumulator += deltaTime;

            while (accumulator >= FRAME_TIME)
            {
                Tick();
                accumulator -= FRAME_TIME;
            }

            isUpdating = false;
        }

        // ===== Single deterministic tick =====
        private void Tick()
        {
            // Extra safety: prevent double tick per Unity frame
            if (Time.frameCount == lastUnityFrameTicked)
            {
                Debug.LogError(
                    "[FrameClock] Tick called twice in same Unity frame!"
                );
                return;
            }

            lastUnityFrameTicked = Time.frameCount;

            CurrentFrame++;
            OnFrameTick?.Invoke(CurrentFrame);
        }

        // ===== Reset (Editor / Runtime safe) =====
        public void Reset()
        {
            CurrentFrame = 0;
            accumulator = 0f;
            lastUnityFrameTicked = -1;
            isUpdating = false;
        }
    }
}
