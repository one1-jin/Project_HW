using UnityEngine;

namespace Core.Runtime
{
    public class FrameClockRunner : MonoBehaviour
    {
        void Awake()
        {
            var runners = FindObjectsByType<FrameClockRunner>(
                FindObjectsSortMode.None
            );

            Debug.Assert(
                runners.Length == 1,
                "[FrameClockRunner] Multiple runners detected in GameScene"
            );

            FrameClock.Instance.Reset();
        }

        void Update()
        {
            FrameClock.Instance.Update(Time.deltaTime);
        }
    }
}
