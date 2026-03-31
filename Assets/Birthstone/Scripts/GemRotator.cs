using UnityEngine;

namespace Birthstone
{
    /// <summary>
    /// Rotates the gem around its Y axis and adds a gentle floating animation.
    /// </summary>
    public class GemRotator : MonoBehaviour
    {
        [HideInInspector] public float rotationSpeed = 30f;
        [SerializeField] float bobAmplitude = 0.05f;
        [SerializeField] float bobFrequency = 1f;

        Vector3 startPosition;
        float timeOffset;

        void Start()
        {
            startPosition = transform.position;
            timeOffset = Random.Range(0f, Mathf.PI * 2f);
        }

        void Update()
        {
            // Rotate
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

            // Bob up and down
            float bob = Mathf.Sin((Time.time + timeOffset) * bobFrequency) * bobAmplitude;
            transform.position = startPosition + Vector3.up * bob;
        }
    }
}
