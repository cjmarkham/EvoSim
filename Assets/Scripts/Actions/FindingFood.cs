﻿using UnityEngine;

public class FindingFood : Action {
    public override int Priority => 75;

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
            //Debug.Log("Found food");
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

    private GameObject GetClosestFood() {
        LayerMask mask = LayerMask.GetMask("Food");
        Collider[] collisions = Physics.OverlapSphere(Sheep.transform.position, Sheep.ViewRadius, mask);

        // We didn't find any food
        if (collisions.Length == 0) {
            return null;
        }

        float closestDistance = Mathf.Infinity;
        GameObject closestFood = null;

        foreach (Collider c in collisions) {
            GameObject food = c.gameObject;

            Eatable component = food.GetComponent<Eatable>();

            // If this food is being targeted by another sheep, ignore it
            if (component.Targeted) {
                continue;
            }

            float distance = Vector3.Distance(c.transform.position, Sheep.transform.position);

            if (distance < closestDistance) {
                closestDistance = distance;
                closestFood = food;
            }
        }

        return closestFood;
    }
}