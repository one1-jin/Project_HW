#if UNITY_EDITOR
using UnityEditor;

namespace Core.Runtime
{
    internal static class FrameClockEditorReset
    {
        static FrameClockEditorReset()
        {
            FrameClock.Instance.Reset();
        }
    }
}
#endif