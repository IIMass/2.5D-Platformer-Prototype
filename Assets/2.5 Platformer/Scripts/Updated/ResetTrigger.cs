using UnityEngine;

public class ResetTrigger : MonoBehaviour
{
    [SerializeField] private Transform resetTransform;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out CharacterController controller))
        {
            controller.enabled = false;
            controller.transform.position = resetTransform.position;
            controller.enabled = true;
        }
    }
}