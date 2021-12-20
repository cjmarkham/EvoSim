using UnityEngine;

public class FindingFood : Action {
    public override int Priority => 50;

    public override Actions Type => Actions.FindingFood;

    private Sheep Sheep;

    private Vector3 Destination;

    private GameObject ClosestFood;

    public FindingFood() {
    }

    public override void OnStart(Sheep sheep) {
        Sheep = sheep;

        ClosestFood = GetClosestFood();

        // If there's no food close, we have no choice to wander until we find some
        if (ClosestFood == null) {
            sheep.Movement.GetRandomDestination(sheep.MaxWanderDistance, out Destination);
        } else {
            Debug.Log("Found food");
            sheep.FoundFood = true;
            Destination = ClosestFood.transform.position;

            // Set this as targeted so no other sheep steals our food
            Eatable component = ClosestFood.GetComponent<Eatable>();
            component.Targeted = true;

            // We've found food so add eating action which will be triggered
            // once this action ends (we've reached the food)
            Action eatAction = new Eating(ClosestFood);
            Sheep.ActionQueue.Add(eatAction);
        }

        sheep.Agent.SetDestination(Destination);
    }

    public override void OnUpdate() {
        if (!Sheep.Agent.pathPending && Sheep.Agent.remainingDistance <= 0.1f) {
            OnEnd();
        }
    }

    public override void OnFixedUpdate() {
        if (ClosestFood == null) {
            ClosestFood = GetClosestFood();
        }

        if (ClosestFood != null) {
            ClosestFood.GetComponent<Renderer>().material.color = Color.red;
        }
    }

    public override void OnEnd() {
        // If we didn't find food, we need to wander again until we do
        if (!Sheep.FoundFood) {
            Debug.Log("Didn't find food, retrying");
            Action findFoodAction = CreateInstance<FindingFood>();
            Sheep.ActionQueue.Add(findFoodAction);
        }

        Sheep.OnActionEnd();
    }

    // TODO: Sometimes, this doesn't actually get the closest food
    private GameObject GetClosestFood() {
        LayerMask mask = LayerMask.GetMask("Food");
        Collider[] collisions = Physics.OverlapSphere(Sheep.transform.position, Sheep.ViewRadius, mask);

        float closestDistance = Vector2.Distance(collisions[0].transform.position, Sheep.transform.position);
        GameObject closestFood = collisions[0].gameObject;

        foreach (Collider c in collisions) {
            GameObject food = c.gameObject;

            food.GetComponent<Renderer>().material.color = Color.blue;

            Eatable component = food.GetComponent<Eatable>();

            // If this food is being targeted by another sheep, ignore it
            if (component.Targeted) {
                continue;
            }

            float distance = Vector2.Distance(c.transform.position, Sheep.transform.position);

            if (distance < closestDistance) {
                closestDistance = distance;
                closestFood = food;
                component.Targeted = true;
            }
        }

        return closestFood;
    }
}