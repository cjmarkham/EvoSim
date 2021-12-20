using UnityEngine;

public class Drinking : Action {
    public override int Priority => 75;

    public override Actions Type => Actions.Drinking;

    private Sheep Sheep;

    public Drinking() {
    }

    public override void OnStart(Sheep sheep) {
        Sheep = sheep;
        Sheep.Agent.ResetPath();
    }

    public override void OnUpdate() {
        if (!Sheep.Agent.pathPending && Sheep.Agent.remainingDistance <= 0.1f) {
            Sheep.Thirst.Decrement();
            Sheep.Agent.ResetPath();

            // If we've drank enough to zero our thirst
            if (Sheep.Thirst.AttributeSatisfied()) {
                Sheep.FoundWater = false;
                Sheep.OnActionEnd();
            }
        }
    }

    public override void OnFixedUpdate() {

    }

    public override void OnEnd() {

    }
}
