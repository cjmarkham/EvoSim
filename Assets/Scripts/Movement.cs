using UnityEngine;
using UnityEngine.AI;

public enum Movements {
    Forward,
    Random
}

public class Movement : MonoBehaviour {
    public bool GetRandomDestination(int maxDistance, out Vector3 position) {
        // Find a random point at our max walk radius
        // We don't want to pick a point off the navmesh so account for that
        Vector3 randomDirection = Random.insideUnitSphere * maxDistance;
        randomDirection += transform.position;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, maxDistance, NavMesh.AllAreas)) {
            position = hit.position;
            return true;
        }
        else {
            position = transform.position;
            return false;
        }
    }
}
