using UnityEngine;

public class LedgeChecker : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("LedgeGrabChecker"))
        {
            other.GetComponentInParent<PlatformerController>().LedgeGrab();
        }
    }
}