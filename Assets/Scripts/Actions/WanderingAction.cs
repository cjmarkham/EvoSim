using UnityEngine;

public class WanderingAction : Action {
    public override int Priority => 1;

    public override Actions Type => Actions.Wandering;

    private Sheep Sheep;

    private Vector3 Destination;

    public WanderingAction() {
    }

    public override void OnStart(Sheep sheep) {
        Sheep = sheep;
        Sheep.Movement.GetRandomDestination(sheep.MaxWanderDistance, out Destination);
        Sheep.Agent.SetDestination(Destination);
    }

    public override void OnUpdate() {
        if (!Sheep.Agent.pathPending && Sheep.Agent.remainingDistance <= 0.1f) {
            Sheep.OnActionEnd();
        }
    }

    public override void OnEnd() {
    }
}