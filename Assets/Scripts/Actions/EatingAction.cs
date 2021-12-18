using UnityEngine;

public class EatingAction : Action {
    public override int Priority => 50;

    public override Actions Type => Actions.Eating;

    private Sheep Sheep;

    private Vector3 Destination;

    private Transform ClosestFood;

    public EatingAction() {
    }

    public override void OnStart(Sheep sheep) {
        Sheep = sheep;
        Sheep.Agent.ResetPath();

        ClosestFood = sheep.GetClosestFood();

        if (ClosestFood == null) {
            sheep.Movement.GetRandomDestination(sheep.MaxWanderDistance, out Destination);
        } else {
            sheep.FoundFood = true;
            Destination = ClosestFood.position;
        }

        sheep.Agent.SetDestination(Destination);
    }

    public override void OnUpdate() {
        if (!Sheep.Agent.pathPending && Sheep.Agent.remainingDistance <= 0.1f) {
            // Happens when we delete the closest food but there's a couple more frames
            // where this is called.
            if (!ClosestFood) {
                Sheep.OnActionEnd();
                return;
            }

            Eatable component = ClosestFood.GetComponent<Eatable>();
            Sheep.Hunger.Decrement();

            Sheep.Agent.ResetPath();

            if (!Sheep.Hunger.ThresholdReached) {
                component.Remove();
                Sheep.FoundFood = false;
                ClosestFood = null;
                Sheep.OnActionEnd();
            }
        }
    }

    public override void OnEnd() {

    }
}
