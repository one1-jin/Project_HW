using UnityEngine;
using UnityEngine.EventSystems;
using System;

namespace SamuraiGame.UI
{
    /// <summary>
    /// One-handed swipe joystick for mobile samurai combat
    /// Controls: Up=Attack, Down=Defense/Parry, Left/Right=Circular Strafe
    /// </summary>
    public class MobileCombatJoystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        [Header("UI References")]
        [SerializeField] private RectTransform joystickBG;
        [SerializeField] private RectTransform joystickHandle;
        [SerializeField] private float handleRange = 100f;

        [Header("Swipe Settings")]
        [SerializeField] private float swipeThreshold = 50f;
        [SerializeField] private float angleThreshold = 45f;
        [SerializeField] private float smoothTime = 0.1f;

        // Events
        public event Action<SwipeDirection> OnSwipeStart;
        public event Action<SwipeDirection> OnSwipeHold;
        public event Action OnSwipeEnd;
        public event Action<Vector2> OnMovement;

        // State
        private Vector2 inputVec;
        private Vector2 smoothVec;
        private Vector2 velocity;
        private Vector2 startPos;
        private bool isDragging;
        private bool isHolding;
        private bool swipeDetected;
        private SwipeDirection currentDir = SwipeDirection.None;
        private Vector2 joystickCenter;

        public enum SwipeDirection { None, Up, Down, Left, Right }

        public Vector2 Input => smoothVec;
        public SwipeDirection Direction => currentDir;
        public bool Holding => isHolding;

        private void Awake()
        {
            if (joystickBG != null)
                joystickCenter = joystickBG.position;
        }

        private void Update()
        {
            smoothVec = Vector2.SmoothDamp(smoothVec, inputVec, ref velocity, smoothTime);

            if (isHolding && currentDir != SwipeDirection.None)
            {
                OnSwipeHold?.Invoke(currentDir);
                
                if (currentDir == SwipeDirection.Left || currentDir == SwipeDirection.Right)
                    OnMovement?.Invoke(smoothVec);
            }
        }

        public void OnPointerDown(PointerEventData e)
        {
            startPos = e.position;
            isDragging = true;
            swipeDetected = false;
            
            if (joystickBG != null)
            {
                joystickCenter = startPos;
                joystickBG.position = startPos;
            }
            
            UpdateVisuals(Vector2.zero);
        }

        public void OnDrag(PointerEventData e)
        {
            if (!isDragging) return;

            Vector2 dir = e.position - startPos;
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

        public void OnPointerUp(PointerEventData e)
        {
            isDragging = false;
            isHolding = false;
            swipeDetected = false;
            currentDir = SwipeDirection.None;
            inputVec = Vector2.zero;
            smoothVec = Vector2.zero;
            velocity = Vector2.zero;
            
            OnSwipeEnd?.Invoke();
            UpdateVisuals(Vector2.zero);
        }

        private SwipeDirection GetDirection(Vector2 dir)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            if (angle < 0) angle += 360f;

            if (angle >= 90f - angleThreshold && angle <= 90f + angleThreshold)
                return SwipeDirection.Up;
            else if (angle >= 270f - angleThreshold && angle <= 270f + angleThreshold)
                return SwipeDirection.Down;
            else if (angle >= 360f - angleThreshold || angle <= angleThreshold)
                return SwipeDirection.Right;
            else if (angle >= 180f - angleThreshold && angle <= 180f + angleThreshold)
                return SwipeDirection.Left;

            return SwipeDirection.None;
        }

        private void UpdateVisuals(Vector2 dir)
        {
            if (joystickHandle == null) return;
            
            Vector2 clamped = Vector2.ClampMagnitude(dir, handleRange);
            joystickHandle.position = joystickCenter + clamped;
            joystickHandle.gameObject.SetActive(isDragging);
        }

        public void Reset()
        {
            isDragging = false;
            isHolding = false;
            swipeDetected = false;
            currentDir = SwipeDirection.None;
            inputVec = Vector2.zero;
            smoothVec = Vector2.zero;
            UpdateVisuals(Vector2.zero);
        }
    }
}
