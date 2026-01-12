using UnityEngine;

namespace Core.Runtime
{
    public class FrameClockRunner : MonoBehaviour
    {
        private static FrameClockRunner instance;

        void Awake()
        {
            if (instance != null)
            {
                Debug.LogError(
                    "Duplicate FrameClockRunner detected. Destroying this instance."
                );
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void Update()
        {
            FrameClock.Instance.Update(Time.deltaTime);
        }

        void OnDestroy()
        {
            if (instance == this)
                instance = null;
        }
    }

}
