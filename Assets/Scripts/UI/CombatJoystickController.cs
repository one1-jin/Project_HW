using UnityEngine;
using UnityEngine.EventSystems;
using System;

namespace SamuraiGame.UI
{
    /// <summary>
    /// One-handed swipe-based joystick controller for mobile samurai combat game.
    /// Supports directional movement and combat actions (attack/defense).
    /// </summary>
    public class CombatJoystickController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        [Header("Joystick UI References")]
        [SerializeField] private RectTransform joystickBackground;
        [SerializeField] private RectTransform joystickHandle;
        [SerializeField] private float handleRange = 100f;
        [SerializeField] private bool showVisualFeedback = true;

        [Header("Swipe Detection Settings")]
        [SerializeField] private float swipeThreshold = 50f;
        [SerializeField] private float angleThreshold = 45f; // Angle tolerance for direction detection

        [Header("Input Smoothing")]
        [SerializeField] private float inputSmoothTime = 0.1f;

        // Events for game systems
        public event Action<SwipeDirection> OnSwipeDetected;
        public event Action<SwipeDirection> OnSwipeHold;
        public event Action OnSwipeReleased;
        public event Action<Vector2> OnMovementInput; // For circular strafe movement

        // Input state
        private Vector2 inputVector;
        private Vector2 smoothedInput;
        private Vector2 inputVelocity;
        private Vector2 startPosition;
        private Vector2 currentPosition;
        private bool isHolding = false;
        private SwipeDirection currentSwipeDirection = SwipeDirection.None;
        private bool swipeDetected = false;

        // Joystick state
        private Vector2 joystickCenter;
        private bool isDragging = false;

        public enum SwipeDirection
        {
            None,
            Up,      // Attack - Executes combo attacks
            Down,    // Defense - Parry stance
            Left,    // Circular strafe left
            Right    // Circular strafe right
        }

        // Public Properties
        public Vector2 InputVector => smoothedInput;
        public SwipeDirection CurrentDirection => currentSwipeDirection;
        public bool IsHolding => isHolding;
        public bool IsDragging => isDragging;

        private void Awake()
        {
            InitializeJoystick();
        }

        private void Update()
        {
            UpdateInputSmoothing();
            ProcessHoldInput();
        }

        #region Initialization

        private void InitializeJoystick()
        {
            if (joystickBackground != null)
            {
                joystickCenter = joystickBackground.position;
            }

            if (joystickHandle != null && !showVisualFeedback)
            {
                joystickHandle.gameObject.SetActive(false);
            }
        }

        #endregion

        #region Input Processing

        private void UpdateInputSmoothing()
        {
            // Smooth input for movement
            smoothedInput = Vector2.SmoothDamp(smoothedInput, inputVector, ref inputVelocity, inputSmoothTime);
        }

        private void ProcessHoldInput()
        {
            // Continuous hold detection
            if (isHolding && currentSwipeDirection != SwipeDirection.None)
            {
                OnSwipeHold?.Invoke(currentSwipeDirection);

                // For horizontal movement (circular strafing), send continuous movement input
                if (currentSwipeDirection == SwipeDirection.Left || currentSwipeDirection == SwipeDirection.Right)
                {
                    OnMovementInput?.Invoke(smoothedInput);
                }
            }
        }

        #endregion

        #region Unity Event System Handlers

        public void OnPointerDown(PointerEventData eventData)
        {
            startPosition = eventData.position;
            currentPosition = eventData.position;
            isDragging = true;
            swipeDetected = false;

            if (joystickBackground != null)
            {
                joystickCenter = startPosition;
                joystickBackground.position = startPosition;
            }

            UpdateJoystickVisuals(Vector2.zero);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!isDragging) return;

            currentPosition = eventData.position;
            Vector2 direction = currentPosition - startPosition;
            float distance = direction.magnitude;

            // Detect swipe direction once threshold is met
            if (!swipeDetected && distance >= swipeThreshold)
            {
                SwipeDirection detectedDirection = GetSwipeDirection(direction);
                
                if (detectedDirection != SwipeDirection.None)
                {
                    currentSwipeDirection = detectedDirection;
                    isHolding = true;
                    swipeDetected = true;
                    OnSwipeDetected?.Invoke(currentSwipeDirection);
                }
            }

            // Calculate input vector for movement (normalized)
            inputVector = Vector2.ClampMagnitude(direction / handleRange, 1f);

            // Update visual feedback
            UpdateJoystickVisuals(direction);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            isDragging = false;
            isHolding = false;
            swipeDetected = false;
            
            SwipeDirection releasedDirection = currentSwipeDirection;
            currentSwipeDirection = SwipeDirection.None;

            // Reset input values
            inputVector = Vector2.zero;
            smoothedInput = Vector2.zero;
            inputVelocity = Vector2.zero;

            OnSwipeReleased?.Invoke();

            // Reset visual feedback
            UpdateJoystickVisuals(Vector2.zero);

            if (joystickHandle != null && !showVisualFeedback)
            {
                joystickHandle.gameObject.SetActive(false);
            }
        }

        #endregion

        #region Swipe Direction Detection

        private SwipeDirection GetSwipeDirection(Vector2 direction)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            // Normalize angle to 0-360 range
            if (angle < 0) angle += 360f;

            // Up: 90° ± angleThreshold (Attack)
            if (angle >= 90f - angleThreshold && angle <= 90f + angleThreshold)
            {
                return SwipeDirection.Up;
            }
            // Down: 270° ± angleThreshold (Defense/Parry)
            else if (angle >= 270f - angleThreshold && angle <= 270f + angleThreshold)
            {
                return SwipeDirection.Down;
            }
            // Right: 0° ± angleThreshold (Strafe Right)
            else if (angle >= 360f - angleThreshold || angle <= angleThreshold)
            {
                return SwipeDirection.Right;
            }
            // Left: 180° ± angleThreshold (Strafe Left)
            else if (angle >= 180f - angleThreshold && angle <= 180f + angleThreshold)
            {
                return SwipeDirection.Left;
            }

            return SwipeDirection.None;
        }

        #endregion

        #region Visual Feedback

        private void UpdateJoystickVisuals(Vector2 direction)
        {
            if (!showVisualFeedback || joystickHandle == null) return;

            // Clamp handle position within range
            Vector2 clampedDirection = Vector2.ClampMagnitude(direction, handleRange);
            joystickHandle.position = joystickCenter + clampedDirection;

            // Show handle when dragging
            if (!joystickHandle.gameObject.activeSelf && isDragging)
            {
                joystickHandle.gameObject.SetActive(true);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Resets the joystick to its default state
        /// </summary>
        public void ResetJoystick()
        {
            isDragging = false;
            isHolding = false;
            swipeDetected = false;
            currentSwipeDirection = SwipeDirection.None;
            inputVector = Vector2.zero;
            smoothedInput = Vector2.zero;
            inputVelocity = Vector2.zero;
            UpdateJoystickVisuals(Vector2.zero);
        }

        /// <summary>
        /// Sets the swipe detection sensitivity
        /// </summary>
        /// <param name="threshold">Minimum distance for swipe detection</param>
        public void SetSensitivity(float threshold)
        {
            swipeThreshold = Mathf.Max(10f, threshold);
        }

        /// <summary>
        /// Sets the angle tolerance for direction detection
        /// </summary>
        /// <param name="angle">Angle threshold in degrees</param>
        public void SetAngleThreshold(float angle)
        {
            angleThreshold = Mathf.Clamp(angle, 15f, 90f);
        }

        /// <summary>
        /// Enables or disables visual feedback
        /// </summary>
        public void SetVisualFeedback(bool enabled)
        {
            showVisualFeedback = enabled;
            if (joystickHandle != null)
            {
                joystickHandle.gameObject.SetActive(enabled && isDragging);
            }
        }

        #endregion

        #region Debug Visualization

        private void OnDrawGizmosSelected()
        {
            if (joystickBackground == null) return;

            // Draw swipe detection threshold
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(joystickBackground.position, swipeThreshold);

            // Draw handle range
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(joystickBackground.position, handleRange);

            // Draw directional indicators
            Vector3 center = joystickBackground.position;
            Gizmos.color = Color.red;
            Gizmos.DrawLine(center, center + Vector3.up * handleRange); // Up - Attack
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(center, center + Vector3.down * handleRange); // Down - Defense
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(center, center + Vector3.left * handleRange); // Left - Strafe
            Gizmos.DrawLine(center, center + Vector3.right * handleRange); // Right - Strafe
        }

        #endregion
    }
}
