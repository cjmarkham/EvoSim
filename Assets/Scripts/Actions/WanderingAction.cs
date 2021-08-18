using UnityEngine;

public class WanderingAction : Action {
    public override int Priority => 1;

    public override Actions Type => Actions.Wandering;

    private Sheep sheep;

    private Vector3 destination;

    public WanderingAction() {
        Debug.Log("[NEW QUEUE ITEM] Wander");
    }

    public override void OnStart(Sheep sheep) {
        Debug.Log("[ACTION START] Wandering");
        this.sheep = sheep;
        //sheep.movement.GetDestination(sheep.transform, sheep.maxWanderDistance, sheep.moveType == Movements.Random, out destination);
        sheep.movement.GetRandomDestination(sheep.maxWanderDistance, out destination);
        sheep.agent.SetDestination(destination);
    }

    public override void OnUpdate() {
        if (!sheep.agent.pathPending && this.sheep.agent.remainingDistance <= 0.1f) {
            sheep.OnActionEnd();
        }
    }

    public override void OnEnd() {
    }
}