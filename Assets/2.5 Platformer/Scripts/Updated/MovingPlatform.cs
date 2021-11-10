using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [Header("Moving Platform")]
    [SerializeField] private List<Transform> travelPoints;
    [SerializeField] private float travelSpeed;
    [SerializeField] private float travelDelay;

    private int currentTravelPointIndex = 0;
    private float travelDelayTimer = 0f;


    private void Start()
    {
        // If there are any travel points set in travelPoints list, make the platform start at the first travel point position
        if (travelPoints.Count > 0) transform.position = travelPoints[0].position;
    }

    // FixedUpdate is called every fixed frame-rate frame.
    // Updates at the same time as the physics calculations.
    void FixedUpdate()
    {
        MoveTowardsPoint();
    }

    private void MoveTowardsPoint()
    {
        // If there are more than 1 travel points...
        if (travelPoints.Count > 1)
        {
            // If the Distance between the platform and the travel point is less than 0.1f...
            // This distance threshold could be declared as a float variable
            if (Vector3.Distance(transform.position, travelPoints[currentTravelPointIndex].position) < 0.1f)
            {
                if (travelDelayTimer < travelDelay)
                {
                    travelDelayTimer += Time.deltaTime;
                    return;
                }

                // Switch the current travel point
                SwitchPoint();
            }
            else
            {
                // Move towards the currently set point to travel with a given travel speed
                transform.position = Vector3.MoveTowards(transform.position, travelPoints[currentTravelPointIndex].position, travelSpeed * Time.deltaTime);
            }
        }
    }

    private void SwitchPoint()
    {
        // By using the modulus operator (%), it retrieves the remainder of the equation.
        // A number lower than the Travel Points count will always return the current index number of the travel point + 1.
        // If it's equal, the remainder will be 0, thus, looping through the list.
        currentTravelPointIndex = (currentTravelPointIndex + 1) % travelPoints.Count;

        travelDelayTimer = 0f;
    }

    private void OnTriggerEnter(Collider other)
    {
        // If the Collider that has just entered the Trigger has an AvatarController script...
        if (other.TryGetComponent(out PlatformerController player))
        {
            // Parent the player to the platform
            player.transform.parent = transform;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // If the Collider that has just exited the Trigger has an AvatarController script...
        if (other.TryGetComponent(out PlatformerController player))
        {
            // Unparent the player from the platform
            player.transform.parent = null;
        }
    }

    private void OnDrawGizmos()
    {
        // If the Travel Points count is higher than 0...
        if (travelPoints.Count > 0)
        {
            // Loop through each point and...
            for (int i = 0; i < travelPoints.Count; i++)
            {
                // Set Sphere color to green if the Travel Point is the current Travel Point set. Else, set it to red.
                Gizmos.color = (i == currentTravelPointIndex) ? Color.green : Color.red;

                // Draw the Travel Point sphere
                Gizmos.DrawSphere(travelPoints[i].position, .25f);

                // Set Line color to white
                Gizmos.color = Color.white;

                // Draw Line to the next Travel Point
                Gizmos.DrawLine(travelPoints[i].position, travelPoints[(i + 1) % travelPoints.Count].position);
            }
        }
    }
}
