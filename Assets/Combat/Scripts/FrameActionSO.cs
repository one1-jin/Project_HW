using UnityEngine;

namespace Combat.Scripts
{
    [CreateAssetMenu(
        fileName = "FrameAction",
        menuName = "Combat/Frame Action"
    )]
    public class FrameActionSO : ScriptableObject
    {
        [Header("Basic Info")]
        public string actionName;

        [Header("Frame Data")]
        public int startupFrames;
        public int activeFrames;
        public int recoveryFrames;

        public int TotalFrames =>
            startupFrames + activeFrames + recoveryFrames;
    }
}
