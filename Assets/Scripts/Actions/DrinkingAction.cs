using UnityEngine;

public class DrinkingAction : Action {
    public override int Priority => 50;

    public override Actions Type => Actions.Drinking;

    private Sheep Sheep;

    private Vector3 Destination;

    private Transform ClosestDrinkingSpot;

    public DrinkingAction() {
    }

    public override void OnStart(Sheep sheep) {
        Sheep = sheep;
        Sheep.Agent.ResetPath();

        ClosestDrinkingSpot = sheep.GetClosestDrinkingSpot();

        if (ClosestDrinkingSpot == null) {
            sheep.Movement.GetRandomDestination(sheep.MaxWanderDistance, out Destination);
        } else {
            sheep.FoundWater = true;
            Destination = ClosestDrinkingSpot.position;
        }

        sheep.Agent.SetDestination(Destination);
    }

    public override void OnUpdate() {
        if (!Sheep.Agent.pathPending && Sheep.Agent.remainingDistance <= 0.1f) {
            Sheep.Thirst.Decrement();
            Sheep.Agent.ResetPath();

            // If we've drank enough to zero our thirst
            if (!Sheep.Thirst.ThresholdReached) {
                Sheep.FoundWater = false;
                ClosestDrinkingSpot = null;
                Sheep.OnActionEnd();
            }
        }
    }

    public override void OnEnd() {

    }
}
