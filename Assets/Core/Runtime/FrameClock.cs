using System;
using Core.Interfaces;
using UnityEngine;

namespace Core.Runtime
{
    /// <summary>
    /// Deterministic fixed-step frame clock.
    /// One "frame" here means a simulation frame, NOT a Unity render frame.
    /// </summary>
    public sealed class FrameClock : IFrameClock
    {
        // ===== Singleton =====
        public static FrameClock Instance { get; } = new FrameClock();

        // ===== Config =====
        public const int FPS = 60;
        public const float FRAME_TIME = 1f / FPS;

        /// <summary>
        /// Maximum simulation ticks allowed per Unity frame.
        /// Prevents spiral-of-death when deltaTime is huge.
        /// </summary>
        private const int MAX_TICKS_PER_UPDATE = 5;

        // ===== State =====
        public int CurrentFrame { get; private set; }
        public event Action<int> OnFrameTick;

        private float accumulator;
        private bool isUpdating;

        // ===== Constructor =====
        private FrameClock() { }

        // ===== Called by Runtime Runner ONLY =====
        public void Update(float deltaTime)
        {
            // Prevent accidental re-entrancy (Update called twice)
            if (isUpdating)
            {
                Debug.LogError("[FrameClock] Duplicate Update call detected.");
                return;
            }

            isUpdating = true;
            accumulator += deltaTime;

            int ticks = 0;

            while (accumulator >= FRAME_TIME)
            {
                Tick();
                accumulator -= FRAME_TIME;

                ticks++;
                if (ticks >= MAX_TICKS_PER_UPDATE)
                {
                    Debug.LogWarning(
                        $"[FrameClock] Tick cap reached ({MAX_TICKS_PER_UPDATE}). " +
                        "Simulation is running behind."
                    );
                    break;
                }
            }

            isUpdating = false;
        }

        // ===== Single deterministic tick =====
        private void Tick()
        {
            CurrentFrame++;
            OnFrameTick?.Invoke(CurrentFrame);
        }

        // ===== Reset (Editor / Runtime safe) =====
        public void Reset()
        {
            CurrentFrame = 0;
            accumulator = 0f;
            isUpdating = false;
        }
    }
}
