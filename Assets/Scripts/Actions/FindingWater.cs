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

        // If there's no water close, we have no choice to wander until we find some
        if (ClosestWater == null) {
            sheep.Movement.GetRandomDestination(sheep.MaxWanderDistance, out Destination);
        }
        else {
            sheep.FoundWater = true;
            Destination = ClosestWater.transform.position;
        }

        sheep.Agent.SetDestination(Destination);
    }

    public override void OnUpdate() {
        if (!Sheep.Agent.pathPending && Sheep.Agent.remainingDistance <= 0.1f) {
            Sheep.OnActionEnd();
        }
    }

    public override void OnFixedUpdate() {
        GetClosestWater();
    }

    public override void OnEnd() {
        // If we didn't find water, we need to wander again until we do
        if (!Sheep.FoundWater) {
            Debug.Log("Didn't find water, retrying");
            Action findWaterAction = CreateInstance<FindingWater>();
            Sheep.ActionQueue.Add(findWaterAction);
        } else {
            Debug.Log("Triggered Drinking");
            // otherwise, drink the water

            // TODO: Not using ScriptableObject.createInstance as I don't yet know
            // how to pass an argument to that
            Action drinkAction = new Drinking(ClosestWater);
            Sheep.ActionQueue.Add(drinkAction);
        }

        Sheep.OnActionEnd();
    }

    private void GetClosestWater() {
        float closestDistanceSqr = Mathf.Infinity;

        LayerMask mask = LayerMask.GetMask("Water");
        Collider[] collisions = Physics.OverlapSphere(Sheep.transform.position, Sheep.ViewRadius, mask);

        foreach (Collider c in collisions) {
            GameObject water = c.gameObject;

            Vector3 directionToFood = water.transform.position - Sheep.transform.position;
            float dSqrToTarget = directionToFood.sqrMagnitude;

            if (dSqrToTarget < closestDistanceSqr) {
                closestDistanceSqr = dSqrToTarget;
                ClosestWater = water;
            }
        }
    }
}