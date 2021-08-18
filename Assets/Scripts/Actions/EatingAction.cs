using UnityEngine;

public class EatingAction : Action {
    public override int Priority => 50;

    public override Actions Type => Actions.Eating;

    private Sheep sheep;

    private Vector3 destination;

    private Transform closestFood;

    public EatingAction() {
        Debug.Log("[NEW ACTION] Eat Food");
    }

    public override void OnStart(Sheep sheep) {
        Debug.Log("[ACTION START] Eating");
        this.sheep = sheep;
        this.sheep.agent.ResetPath();

        closestFood = sheep.GetClosestFood();

        if (closestFood == null) {
            sheep.movement.GetRandomDestination(sheep.maxWanderDistance, out destination);
        } else {
            sheep.FoundFood = true;
            destination = closestFood.position;
        }

        sheep.agent.SetDestination(destination);
    }

    public override void OnUpdate() {
        if (!sheep.agent.pathPending && sheep.agent.remainingDistance <= 0.1f) {
            // Happens when we delete the closest food but there's a couple more frames
            // where this is called.
            if (!closestFood) {
                sheep.OnActionEnd();
                return;
            }

            Eatable component = closestFood.GetComponent<Eatable>();
            sheep.Hunger -= Time.deltaTime * component.Sustenance;

            sheep.agent.ResetPath();

            if (sheep.Hunger <= 0f) {
                sheep.Hunger = 0f;
                component.Remove();
                sheep.FoundFood = false;
                closestFood = null;
                sheep.OnActionEnd();
            }
        }
    }

    public override void OnEnd() {

    }
}
