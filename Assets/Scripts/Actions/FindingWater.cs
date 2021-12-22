using UnityEngine;

public class FindingWater : Action {
    public override int Priority => 75;

    public override Actions Type => Actions.FindingWater;

    private Sheep Sheep;

    private Vector3 Destination;

    private GameObject ClosestWater;

    public FindingWater() {
    }

    public override void OnStart(Sheep sheep) {
        Sheep = sheep;

        ClosestWater = GetClosestWater();

        // If there's no water close, we have no choice to wander until we find some
        if (ClosestWater == null) {
            sheep.Movement.GetRandomDestination(sheep.MaxWanderDistance, out Destination);
        }
        else {
            //Debug.Log("Found water");
            sheep.FoundWater = true;
            Destination = ClosestWater.transform.position;

            // We've found water so add drinking action which will be triggered
            // once this action ends (we've reached the water)
            Action drinkAction = new Drinking();
            Sheep.ActionQueue.Add(drinkAction);
        }

        sheep.Agent.SetDestination(Destination);
    }

    public override void OnUpdate() {
        if (!Sheep.Agent.pathPending && Sheep.Agent.remainingDistance <= 0.1f) {
            Sheep.OnActionEnd();
        }
    }

    public override void OnFixedUpdate() {
        if (ClosestWater == null) {
            ClosestWater = GetClosestWater();
        }
    }

    public override void OnEnd() {
        // If we didn't find water, we need to wander again until we do
        if (!Sheep.FoundWater) {
            Debug.Log("Didn't find water, retrying");
            Action findWaterAction = CreateInstance<FindingWater>();
            Sheep.ActionQueue.Add(findWaterAction);
        }

        Sheep.OnActionEnd();
    }

    private GameObject GetClosestWater() {
        LayerMask mask = LayerMask.GetMask("DrinkSpot");
        Collider[] collisions = Physics.OverlapSphere(Sheep.transform.position, Sheep.ViewRadius, mask);

        // We didn't find any water
        if (collisions.Length == 0) {
            return null;
        }

        float closestDistance = Mathf.Infinity;
        GameObject closestWater = null;

        foreach (Collider c in collisions) {
            GameObject water = c.gameObject;

            float distance = Vector3.Distance(c.transform.position, Sheep.transform.position);

            if (distance < closestDistance) {
                closestDistance = distance;
                closestWater = water;
            }
        }

        return closestWater;
    }
}