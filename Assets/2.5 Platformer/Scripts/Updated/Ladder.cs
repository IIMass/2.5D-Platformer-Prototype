using UnityEngine;

public class Ladder : MonoBehaviour
{
    [SerializeField] private Transform upwardTP;

    [SerializeField] private Collider ladderTrigger;
    [SerializeField] private Collider ladderTravelBounds;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlatformerController controller))
        {
            controller.LadderNearAssign(this);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out PlatformerController controller))
        {
            controller.LadderNearAssign(null);
        }
    }

    public Collider GetLadderTrigger()
    {
        return ladderTrigger;
    }
    public Collider GetLadderTravelBounds()
    {
        return ladderTravelBounds;
    }
    public Transform GetLadderUpEnd()
    {
        return upwardTP;
    }
}