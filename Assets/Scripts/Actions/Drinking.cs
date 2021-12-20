using UnityEngine;

public class Drinking : Action {
    public override int Priority => 75;

    public override Actions Type => Actions.Drinking;

    private Sheep Sheep;

    private Vector3 Destination;

    private GameObject ClosestWater;

    public Drinking(GameObject closestWater) {
        ClosestWater = closestWater;
    }

    public override void OnStart(Sheep sheep) {
        Sheep = sheep;
        Sheep.Agent.ResetPath();
      
        sheep.FoundWater = true;
        Destination = ClosestWater.transform.position;

        sheep.Agent.SetDestination(Destination);
    }

    public override void OnUpdate() {
        if (!Sheep.Agent.pathPending && Sheep.Agent.remainingDistance <= 0.1f) {
            Sheep.Thirst.Decrement();
            Sheep.Agent.ResetPath();

            // If we've drank enough to zero our thirst
            if (Sheep.Thirst.AttributeSatisfied()) {
                Sheep.FoundWater = false;
                ClosestWater = null; // probably don't need this
                Sheep.OnActionEnd();
            }
        }
    }

    public override void OnFixedUpdate() {

    }

    public override void OnEnd() {

    }
}
