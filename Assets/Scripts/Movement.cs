using UnityEngine;
using UnityEngine.AI;

public enum Movements
{
    Forward,
    Random
}

public class Movement : MonoBehaviour
{
    public bool GetDestination(Transform transform, int maxDistance, bool shouldRandom, out Vector3 position)
    {
        if (shouldRandom)
        {
            return GetRandomDestination(maxDistance, out position);
        }


        // Find a point in front of us within our max distance
        Vector3 offset = transform.forward * maxDistance;
        Vector3 newPosition = offset + new Vector3(
            Random.Range(-maxDistance, maxDistance),
            0,
            Random.Range(-maxDistance, maxDistance)
        );

        NavMeshHit hit;
        if (NavMesh.SamplePosition(newPosition, out hit, maxDistance, NavMesh.AllAreas))
        {
            position = hit.position;
            return true;
        }
        else
        {
            position = transform.position;
            return false;
        }
    }

    public bool GetRandomDestination(int maxDistance, out Vector3 position)
    {
        // Find a random point within our max walk radius
        // We don't want to pick a point off the navmesh so account for that
        Vector3 randomDirection = Random.insideUnitSphere * maxDistance;
        randomDirection += transform.position;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, maxDistance, NavMesh.AllAreas))
        {
            position = hit.position;
            return true;
        }
        else
        {
            position = transform.position;
            return false;
        }
    }
}
