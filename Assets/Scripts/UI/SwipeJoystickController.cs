using UnityEngine;
using UnityEngine.EventSystems;
using System;

namespace SamuraiGame.UI
{
    /// <summary>
    /// One-handed swipe-based joystick controller for mobile combat game.
    /// Supports directional movement and combat actions (attack/defense).
    /// </summary>
    public class SwipeJoystickController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        [Header("Joystick Settings")]
        [SerializeField] private RectTransform joystickBackground;
        [SerializeField] private RectTransform joystickHandle;
        [SerializeField] private float handleRange = 100f;
        [SerializeField] private bool showVisualFeedback = true;

        [Header("Swipe Detection")]
        [SerializeField] private float swipeThreshold = 50f;
        [SerializeField] private float angleThreshold = 45f; // Angle tolerance for direction detection

        [Header("Input Smoothing")]
        [SerializeField] private float inputSmoothTime = 0.1f;

        // Events
        public event Action<SwipeDirection> OnSwipeDetected;
        public event Action<SwipeDirection> OnSwipeHold;
        public event Action OnSwipeReleased;
        public event Action<Vector2> OnMovementInput; // For circular movement (left/right)

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
            Up,      // Attack
            Down,    // Defense
            Left,    // Strafe Left
            Right    // Strafe Right
        }

        // Properties
        public Vector2 InputVector => smoothedInput;
        public SwipeDirection CurrentDirection => currentSwipeDirection;
        public bool IsHolding => isHolding;

        private void Awake()
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

        private void Update()
        {
            // Smooth input for movement
            smoothedInput = Vector2.SmoothDamp(smoothedInput, inputVector, ref inputVelocity, inputSmoothTime);

            // Continuous hold detection
            if (isHolding && currentSwipeDirection != SwipeDirection.None)
            {
                OnSwipeHold?.Invoke(currentSwipeDirection);

                // For horizontal movement, send continuous movement input
                if (currentSwipeDirection == SwipeDirection.Left || currentSwipeDirection == SwipeDirection.Right)
                {
                    OnMovementInput?.Invoke(smoothedInput);
                }
            }
        }

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

            // Calculate input vector for movement
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

        private SwipeDirection GetSwipeDirection(Vector2 direction)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            // Normalize angle to 0-360 range
            if (angle < 0) angle += 360f;

            // Up: 45° to 135°
            if (angle >= 90f - angleThreshold && angle <= 90f + angleThreshold)
            {
                return SwipeDirection.Up;
            }
            // Down: 225° to 315°
            else if (angle >= 270f - angleThreshold && angle <= 270f + angleThreshold)
            {
                return SwipeDirection.Down;
            }
            // Right: -45° to 45° (315° to 45°)
            else if (angle >= 360f - angleThreshold || angle <= angleThreshold)
            {
                return SwipeDirection.Right;
            }
            // Left: 135° to 225°
            else if (angle >= 180f - angleThreshold && angle <= 180f + angleThreshold)
            {
                return SwipeDirection.Left;
            }

            return SwipeDirection.None;
        }

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

        // Public methods for external control
        public void ResetJoystick()
        {
            isDragging = false;
            isHolding = false;
            swipeDetected = false;
            currentSwipeDirection = SwipeDirection.None;
            inputVector = Vector2.zero;
            smoothedInput = Vector2.zero;
            UpdateJoystickVisuals(Vector2.zero);
        }

        public void SetSensitivity(float threshold)
        {
            swipeThreshold = Mathf.Max(10f, threshold);
        }

        private void OnDrawGizmosSelected()
        {
            if (joystickBackground == null) return;

            // Draw detection range
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(joystickBackground.position, swipeThreshold);

            // Draw handle range
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(joystickBackground.position, handleRange);
        }
    }
}
