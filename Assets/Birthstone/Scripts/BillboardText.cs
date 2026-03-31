using UnityEngine;

namespace Birthstone
{
    /// <summary>
    /// Makes the text always face the camera.
    /// </summary>
    public class BillboardText : MonoBehaviour
    {
        void LateUpdate()
        {
            if (Camera.main != null)
            {
                transform.rotation = Quaternion.LookRotation(
                    transform.position - Camera.main.transform.position);
            }
        }
    }
}
