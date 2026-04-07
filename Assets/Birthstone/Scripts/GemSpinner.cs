using UnityEngine;
using UnityEngine.InputSystem;

namespace Birthstone
{
    /// <summary>
    /// Flick-to-spin gem controller (X and Y axes).
    /// Tracks swipe velocity and applies angular momentum with friction.
    /// </summary>
    public class GemSpinner : MonoBehaviour
    {
        [Header("Spin Physics")]
        [SerializeField] float swipeMultiplier = 0.12f;
        [SerializeField] float friction = 0.97f;
        [SerializeField] float minAngularVelocity = 5f;

        [Header("Stats")]
        public float TotalSpins { get; private set; }
        public float CurrentSpeed => angularVelocity.magnitude;

        Vector2 angularVelocity;  // x = yaw (horizontal swipe), y = pitch (vertical swipe)
        float totalRotation;
        bool isDragging;
        Vector2 prevDragPos;

        // Smoothing for swipe velocity
        Vector2[] velocityHistory = new Vector2[5];
        int velocityIndex;

        public event System.Action<float> OnSpinUpdate;
        public event System.Action OnFlick;

        void Update()
        {
            HandleInput();
            ApplyRotation();
        }

        void HandleInput()
        {
            // Touch input
            var touchscreen = Touchscreen.current;
            if (touchscreen != null)
            {
                var touch0 = touchscreen.touches[0];
                if (touch0.press.isPressed)
                {
                    Vector2 pos = touch0.position.ReadValue();

                    if (touch0.press.wasPressedThisFrame)
                        StartDrag(pos);

                    if (isDragging)
                        UpdateDrag(pos);
                }

                if (touch0.press.wasReleasedThisFrame && isDragging)
                    EndDrag();

                return;
            }

            // Mouse input fallback
            var mouse = Mouse.current;
            if (mouse == null) return;

            if (mouse.leftButton.isPressed)
            {
                Vector2 pos = mouse.position.ReadValue();

                if (mouse.leftButton.wasPressedThisFrame)
                    StartDrag(pos);

                if (isDragging)
                    UpdateDrag(pos);
            }

            if (mouse.leftButton.wasReleasedThisFrame && isDragging)
                EndDrag();
        }

        void StartDrag(Vector2 pos)
        {
            isDragging = true;
            prevDragPos = pos;
            velocityIndex = 0;
            for (int i = 0; i < velocityHistory.Length; i++)
                velocityHistory[i] = Vector2.zero;
        }

        void UpdateDrag(Vector2 pos)
        {
            Vector2 delta = pos - prevDragPos;
            float dt = Mathf.Max(Time.deltaTime, 0.001f);
            Vector2 vel = delta / dt;

            velocityHistory[velocityIndex % velocityHistory.Length] = vel;
            velocityIndex++;

            // Direct drag: horizontal swipe → yaw, vertical swipe → pitch
            angularVelocity.x = delta.x * swipeMultiplier / dt;
            angularVelocity.y = delta.y * swipeMultiplier / dt;

            prevDragPos = pos;
        }

        void EndDrag()
        {
            isDragging = false;

            // Average velocity for smooth flick
            Vector2 avgVel = Vector2.zero;
            int count = Mathf.Min(velocityIndex, velocityHistory.Length);
            for (int i = 0; i < count; i++)
                avgVel += velocityHistory[i];
            if (count > 0) avgVel /= count;

            angularVelocity = avgVel * swipeMultiplier;

            if (angularVelocity.magnitude > 100f)
                OnFlick?.Invoke();
        }

        void ApplyRotation()
        {
            if (!isDragging)
            {
                angularVelocity *= friction;
                if (angularVelocity.magnitude < minAngularVelocity)
                    angularVelocity = Vector2.zero;
            }

            float yawDelta = angularVelocity.x * Time.deltaTime;
            float pitchDelta = -angularVelocity.y * Time.deltaTime; // invert: swipe up → rotate backward

            // Apply rotation in world space so axes don't get tangled
            transform.Rotate(Vector3.up, yawDelta, Space.World);
            transform.Rotate(Vector3.right, pitchDelta, Space.World);

            // Track total spins
            float rotMag = Mathf.Sqrt(yawDelta * yawDelta + pitchDelta * pitchDelta);
            totalRotation += Mathf.Abs(rotMag);
            TotalSpins = totalRotation / 360f;
            OnSpinUpdate?.Invoke(TotalSpins);
        }

        /// <summary>
        /// Give the gem an initial spin.
        /// </summary>
        public void AddSpin(float velocityX, float velocityY = 0f)
        {
            angularVelocity.x += velocityX;
            angularVelocity.y += velocityY;
        }
    }
}
