using UnityEngine;

public sealed class SlowRotate : MonoBehaviour
{
    [SerializeField] private Vector3 rotationAxis = Vector3.up;
    [SerializeField] private float degreesPerSecond = 15f;
    [SerializeField] private Space space = Space.Self;

    private void Update()
    {
        if (rotationAxis.sqrMagnitude <= 0f) return;
        transform.Rotate(rotationAxis.normalized, degreesPerSecond * Time.deltaTime, space);
    }
}