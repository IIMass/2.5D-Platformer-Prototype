using UnityEngine;

namespace Platformer.Legacy
{
    public class LedgeChecker : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("LedgeGrabChecker"))
            {
                other.GetComponentInParent<PController>().LedgeGrab();
            }
        }
    }
}