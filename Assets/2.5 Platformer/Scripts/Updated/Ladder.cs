using UnityEngine;

public class Ladder : MonoBehaviour
{
    [SerializeField] private Transform upEnd;
    [SerializeField] private Transform downEnd;

    private Collider ladderTrigger;

    private void Start()
    {
        ladderTrigger = GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlatformerController controller))
        {
            controller.LadderNearAssign(this, true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out PlatformerController controller))
        {
            controller.LadderNearAssign(this, false);
        }
    }

    public Collider GetLadderTrigger()
    {
        return ladderTrigger;
    }
    public Transform GetLadderUpEnd()
    {
        return upEnd;
    }
    public Transform GetLadderDownEnd()
    {
        return downEnd;
    }
}