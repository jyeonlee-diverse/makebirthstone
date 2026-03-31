using UnityEngine;
using UnityEngine.InputSystem;

namespace Birthstone
{
    /// <summary>
    /// Orbital camera controller supporting both mouse (PC) and touch (mobile).
    /// PC: Right-click drag to orbit, scroll to zoom, left-click to focus.
    /// Mobile: One finger drag to orbit, pinch to zoom, tap to focus.
    /// </summary>
    public class BirthstoneCamera : MonoBehaviour
    {
        [Header("Orbit")]
        [SerializeField] float orbitSpeed = 0.3f;
        [SerializeField] float touchOrbitSpeed = 0.15f;
        [SerializeField] float minPitch = -20f;
        [SerializeField] float maxPitch = 60f;

        [Header("Zoom")]
        [SerializeField] float zoomSpeed = 0.5f;
        [SerializeField] float pinchZoomSpeed = 0.02f;
        [SerializeField] float minDistance = 3f;
        [SerializeField] float maxDistance = 15f;

        [Header("Focus")]
        [SerializeField] float focusSpeed = 3f;

        float yaw;
        float pitch = 20f;
        float distance = 9f;
        Vector3 targetPosition = Vector3.zero;
        Vector3 currentTargetPosition;

        // Touch tracking
        float prevPinchDistance;
        bool wasDragging;
        Vector2 prevTouchPos;
        float touchStartTime;
        Vector2 touchStartPos;
        const float tapThreshold = 0.3f;       // max seconds for a tap
        const float tapMoveThreshold = 20f;     // max pixels moved for a tap

        void Start()
        {
            currentTargetPosition = targetPosition;
            yaw = transform.eulerAngles.y;
            pitch = transform.eulerAngles.x;
        }

        void Update()
        {
            HandleTouchInput();
            HandleMouseInput();
            UpdateCamera();
        }

        void HandleTouchInput()
        {
            var touchscreen = Touchscreen.current;
            if (touchscreen == null) return;

            int touchCount = 0;
            foreach (var touch in touchscreen.touches)
            {
                if (touch.press.isPressed) touchCount++;
            }

            if (touchCount == 1)
            {
                var touch0 = touchscreen.touches[0];
                Vector2 pos = touch0.position.ReadValue();
                Vector2 delta = touch0.delta.ReadValue();

                if (touch0.press.wasPressedThisFrame)
                {
                    touchStartTime = Time.time;
                    touchStartPos = pos;
                    prevTouchPos = pos;
                    wasDragging = false;
                }

                // Check if moved enough to be a drag
                if (Vector2.Distance(pos, touchStartPos) > tapMoveThreshold)
                    wasDragging = true;

                // Orbit with single finger drag
                if (wasDragging)
                {
                    yaw += delta.x * touchOrbitSpeed;
                    pitch -= delta.y * touchOrbitSpeed;
                    pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
                }

                // Tap to focus (released quickly without much movement)
                if (touch0.press.wasReleasedThisFrame && !wasDragging
                    && (Time.time - touchStartTime) < tapThreshold)
                {
                    TryFocusGem(pos);
                }

                prevTouchPos = pos;
            }
            else if (touchCount >= 2)
            {
                // Pinch to zoom
                var touch0 = touchscreen.touches[0];
                var touch1 = touchscreen.touches[1];
                Vector2 pos0 = touch0.position.ReadValue();
                Vector2 pos1 = touch1.position.ReadValue();
                float curDist = Vector2.Distance(pos0, pos1);

                if (touch1.press.wasPressedThisFrame)
                {
                    prevPinchDistance = curDist;
                }

                if (prevPinchDistance > 0)
                {
                    float pinchDelta = curDist - prevPinchDistance;
                    distance -= pinchDelta * pinchZoomSpeed * distance * 0.01f;
                    distance = Mathf.Clamp(distance, minDistance, maxDistance);
                }

                prevPinchDistance = curDist;
                wasDragging = true; // prevent tap on release
            }
        }

        void HandleMouseInput()
        {
            var mouse = Mouse.current;
            if (mouse == null) return;

            // Right mouse drag to orbit
            if (mouse.rightButton.isPressed)
            {
                Vector2 delta = mouse.delta.ReadValue();
                yaw += delta.x * orbitSpeed;
                pitch -= delta.y * orbitSpeed;
                pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
            }

            // Scroll to zoom
            float scroll = mouse.scroll.ReadValue().y;
            if (Mathf.Abs(scroll) > 0.01f)
            {
                distance -= scroll * zoomSpeed * 0.01f * distance;
                distance = Mathf.Clamp(distance, minDistance, maxDistance);
            }

            // Left click to focus on gem
            if (mouse.leftButton.wasPressedThisFrame)
            {
                TryFocusGem(mouse.position.ReadValue());
            }

            // Escape to reset view
            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
            {
                targetPosition = Vector3.zero;
                distance = 9f;
                pitch = 20f;
            }
        }

        void TryFocusGem(Vector2 screenPos)
        {
            Ray ray = Camera.main.ScreenPointToRay(screenPos);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                var rotator = hit.transform.GetComponent<GemRotator>();
                if (rotator != null)
                {
                    targetPosition = hit.transform.position;
                }
            }
        }

        void UpdateCamera()
        {
            currentTargetPosition = Vector3.Lerp(currentTargetPosition, targetPosition,
                                                  focusSpeed * Time.deltaTime);

            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
            Vector3 offset = rotation * new Vector3(0, 0, -distance);

            transform.position = currentTargetPosition + offset;
            transform.LookAt(currentTargetPosition);
        }
    }
}
