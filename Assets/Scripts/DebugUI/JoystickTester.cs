using SamuraiGame.UI;
using UnityEngine;

namespace SamuraiGame
{
    /// <summary>
    /// Test script for FullscreenCombatJoystick
    /// Shows on-screen debug info for swipe detection
    /// </summary>
    public class JoystickTester : MonoBehaviour
    {
        // Fields - Serialized
        [Header("References")]
        [SerializeField] private FullscreenCombatJoystick fullscreenJoystick;

        [Header("Debug Display")]
        [SerializeField] private bool showDebugLogs = true;
        [SerializeField] private bool showOnScreenDebug = true;

        // Fields - Private
        private string currentAction = "Touch anywhere on screen to start...";
        private Vector2 currentInput = Vector2.zero;

        // Unity Lifecycle Methods
        private void Start()
        {
            if (fullscreenJoystick == null)
                fullscreenJoystick = FindAnyObjectByType<FullscreenCombatJoystick>();

            if (fullscreenJoystick == null)
            {
                Debug.LogError("FullscreenCombatJoystick not found! Please add it to the scene.");
                return;
            }

            fullscreenJoystick.OnSwipeStart += HandleSwipeStart;
            fullscreenJoystick.OnSwipeHold += HandleSwipeHold;
            fullscreenJoystick.OnSwipeEnd += HandleSwipeEnd;
            fullscreenJoystick.OnMovement += HandleMovement;

            Debug.Log("JoystickTester initialized. Touch anywhere on screen!");
        }

        private void OnDestroy()
        {
            if (fullscreenJoystick == null)
                return;

            fullscreenJoystick.OnSwipeStart -= HandleSwipeStart;
            fullscreenJoystick.OnSwipeHold -= HandleSwipeHold;
            fullscreenJoystick.OnSwipeEnd -= HandleSwipeEnd;
            fullscreenJoystick.OnMovement -= HandleMovement;
        }

        private void OnGUI()
        {
            if (!showOnScreenDebug)
                return;

            GUIStyle style = new GUIStyle(GUI.skin.label)
            {
                fontSize = 28,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.UpperCenter
            };
            style.normal.textColor = Color.white;

            GUI.color = new Color(0, 0, 0, 0.5f);
            GUI.Box(new Rect(0, 0, Screen.width, 200), "");
            GUI.color = Color.white;

            GUI.Label(new Rect(0, 30, Screen.width, 50), currentAction, style);

            style.fontSize = 20;
            style.alignment = TextAnchor.UpperLeft;
            GUI.Label(new Rect(30, 100, 400, 100),
                $"Input: ({currentInput.x:F2}, {currentInput.y:F2})\n" +
                $"Direction: {fullscreenJoystick.Direction}\n" +
                $"Holding: {fullscreenJoystick.Holding}",
                style);

            GUI.color = new Color(0, 0, 0, 0.7f);
            GUI.Box(new Rect(0, Screen.height - 180, Screen.width, 180), "");
            GUI.color = Color.yellow;

            style.fontSize = 18;
            style.alignment = TextAnchor.MiddleCenter;
            GUI.Label(new Rect(0, Screen.height - 170, Screen.width, 160),
                "🎮 FULLSCREEN CONTROLS 🎮\n\n" +
                "Touch ANYWHERE and swipe:\n" +
                "⬆️ UP = Attack  |  ⬇️ DOWN = Defense/Parry\n" +
                "⬅️ LEFT = Strafe Left  |  ➡️ RIGHT = Strafe Right",
                style);
        }

        // Event Handlers
        private void HandleSwipeStart(FullscreenCombatJoystick.SwipeDirection direction)
        {
            switch (direction)
            {
                case FullscreenCombatJoystick.SwipeDirection.Up:
                    currentAction = "⚔️ ATTACK STARTED!";
                    if (showDebugLogs)
                        Debug.Log("Attack initiated!");
                    break;

                case FullscreenCombatJoystick.SwipeDirection.Down:
                    currentAction = "🛡️ DEFENSE STANCE!";
                    if (showDebugLogs)
                        Debug.Log("Defense stance activated!");
                    break;

                case FullscreenCombatJoystick.SwipeDirection.Left:
                    currentAction = "⬅️ STRAFE LEFT";
                    if (showDebugLogs)
                        Debug.Log("Strafing left!");
                    break;

                case FullscreenCombatJoystick.SwipeDirection.Right:
                    currentAction = "➡️ STRAFE RIGHT";
                    if (showDebugLogs)
                        Debug.Log("Strafing right!");
                    break;
            }
        }

        private void HandleSwipeHold(FullscreenCombatJoystick.SwipeDirection direction)
        {
            // Continuous hold - used for combo attacks
        }

        private void HandleSwipeEnd()
        {
            currentAction = "Touch anywhere to continue...";
            currentInput = Vector2.zero;

            if (showDebugLogs)
                Debug.Log("Input released");
        }

        private void HandleMovement(Vector2 movement)
        {
            currentInput = movement;
        }
    }
}
