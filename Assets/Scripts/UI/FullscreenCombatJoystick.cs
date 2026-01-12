using System;
using UnityEngine;

namespace SamuraiGame.UI
{
    /// <summary>
    /// Fullscreen swipe joystick - detects input anywhere on screen
    /// Controls: Up=Attack, Down=Defense/Parry, Left/Right=Circular Strafe
    /// </summary>
    public class FullscreenCombatJoystick : MonoBehaviour
    {
        public enum SwipeDirection
        {
            None,
            Up,
            Down,
            Left,
            Right
        }

        // Fields - Serialized
        [Header("Visual References")]
        [SerializeField] private RectTransform joystickBG;
        [SerializeField] private RectTransform joystickHandle;
        [SerializeField] private float handleRange = 100f;
        [SerializeField] private bool showVisuals = true;

        [Header("Swipe Settings")]
        [SerializeField] private float swipeThreshold = 50f;
        [SerializeField] private float angleThreshold = 45f;
        [SerializeField] private float smoothTime = 0.1f;

        // Fields - Private
        private Vector2 inputVec;
        private Vector2 smoothVec;
        private Vector2 velocity;
        private Vector2 startPos;
        private Vector2 currentPos;
        private Vector2 joystickCenter;
        private SwipeDirection currentDir = SwipeDirection.None;
        private int touchId = -1;
        private bool isDragging;
        private bool isHolding;
        private bool swipeDetected;

        // Properties
        public Vector2 Input => smoothVec;
        public SwipeDirection Direction => currentDir;
        public bool Holding => isHolding;

        // Events
        public event Action<SwipeDirection> OnSwipeStart;
        public event Action<SwipeDirection> OnSwipeHold;
        public event Action OnSwipeEnd;
        public event Action<Vector2> OnMovement;

        // Unity Lifecycle Methods
        private void Awake()
        {
            if (joystickBG != null && !showVisuals)
                joystickBG.gameObject.SetActive(false);
            
            if (joystickHandle != null)
                joystickHandle.gameObject.SetActive(false);
        }

        private void Update()
        {
            HandleInput();
            smoothVec = Vector2.SmoothDamp(smoothVec, inputVec, ref velocity, smoothTime);

            if (isHolding && currentDir != SwipeDirection.None)
            {
                OnSwipeHold?.Invoke(currentDir);

                if (currentDir == SwipeDirection.Left || currentDir == SwipeDirection.Right)
                    OnMovement?.Invoke(smoothVec);
            }
        }

        // Public Methods
        public void ResetJoystick()
        {
            isDragging = false;
            isHolding = false;
            swipeDetected = false;
            currentDir = SwipeDirection.None;
            inputVec = Vector2.zero;
            smoothVec = Vector2.zero;

            if (joystickBG != null)
                joystickBG.gameObject.SetActive(false);
            
            if (joystickHandle != null)
                joystickHandle.gameObject.SetActive(false);
        }

        public void SetSensitivity(float threshold)
        {
            swipeThreshold = Mathf.Max(10f, threshold);
        }

        public void SetAngleThreshold(float angle)
        {
            angleThreshold = Mathf.Clamp(angle, 15f, 90f);
        }

        // Private Methods
        private void HandleInput()
        {
            if (UnityEngine.Input.touchCount > 0)
            {
                Touch touch = UnityEngine.Input.GetTouch(0);

                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        OnInputDown(touch.position);
                        touchId = touch.fingerId;
                        break;

                    case TouchPhase.Moved:
                    case TouchPhase.Stationary:
                        if (touchId == touch.fingerId)
                            OnInputDrag(touch.position);
                        break;

                    case TouchPhase.Ended:
                    case TouchPhase.Canceled:
                        if (touchId == touch.fingerId)
                        {
                            OnInputUp();
                            touchId = -1;
                        }
                        break;
                }
            }
            else
            {
                if (UnityEngine.Input.GetMouseButtonDown(0))
                    OnInputDown(UnityEngine.Input.mousePosition);
                else if (UnityEngine.Input.GetMouseButton(0))
                    OnInputDrag(UnityEngine.Input.mousePosition);
                else if (UnityEngine.Input.GetMouseButtonUp(0))
                    OnInputUp();
            }
        }

        private void OnInputDown(Vector2 position)
        {
            startPos = position;
            currentPos = position;
            isDragging = true;
            swipeDetected = false;
            joystickCenter = startPos;

            if (showVisuals && joystickBG != null)
            {
                joystickBG.gameObject.SetActive(true);
                joystickBG.position = startPos;
            }

            UpdateVisuals(Vector2.zero);
        }

        private void OnInputDrag(Vector2 position)
        {
            if (!isDragging)
                return;

            currentPos = position;
            Vector2 dir = currentPos - startPos;
            float dist = dir.magnitude;

            if (!swipeDetected && dist >= swipeThreshold)
            {
                SwipeDirection detected = GetDirection(dir);
                if (detected != SwipeDirection.None)
                {
                    currentDir = detected;
                    isHolding = true;
                    swipeDetected = true;
                    OnSwipeStart?.Invoke(currentDir);
                }
            }

            inputVec = Vector2.ClampMagnitude(dir / handleRange, 1f);
            UpdateVisuals(dir);
        }

        private void OnInputUp()
        {
            isDragging = false;
            isHolding = false;
            swipeDetected = false;
            currentDir = SwipeDirection.None;
            inputVec = Vector2.zero;
            smoothVec = Vector2.zero;
            velocity = Vector2.zero;

            OnSwipeEnd?.Invoke();

            if (showVisuals && joystickBG != null)
                joystickBG.gameObject.SetActive(false);
            
            if (joystickHandle != null)
                joystickHandle.gameObject.SetActive(false);
        }

        private SwipeDirection GetDirection(Vector2 dir)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            if (angle < 0)
                angle += 360f;

            if (angle >= 90f - angleThreshold && angle <= 90f + angleThreshold)
                return SwipeDirection.Up;
            
            if (angle >= 270f - angleThreshold && angle <= 270f + angleThreshold)
                return SwipeDirection.Down;
            
            if (angle >= 360f - angleThreshold || angle <= angleThreshold)
                return SwipeDirection.Right;
            
            if (angle >= 180f - angleThreshold && angle <= 180f + angleThreshold)
                return SwipeDirection.Left;

            return SwipeDirection.None;
        }

        private void UpdateVisuals(Vector2 dir)
        {
            if (!showVisuals || joystickHandle == null)
                return;

            Vector2 clamped = Vector2.ClampMagnitude(dir, handleRange);
            joystickHandle.position = joystickCenter + clamped;
            joystickHandle.gameObject.SetActive(isDragging);
        }
    }
}
