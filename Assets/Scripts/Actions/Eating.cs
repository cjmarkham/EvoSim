﻿using UnityEngine;

public class Eating : Action {
    public override int Priority => 50;

    public override Actions Type => Actions.Eating;

    private Sheep Sheep;

    public GameObject ClosestFood;

    public Eating(GameObject closestFood) {
        ClosestFood = closestFood;
    }

    public override void OnStart(Sheep sheep) {
        Sheep = sheep;
        Sheep.Agent.ResetPath();
    }

    public override void OnUpdate() {
        Eatable component = ClosestFood.GetComponent<Eatable>();
        Sheep.Hunger.Decrement();
        Sheep.Agent.ResetPath();

        if (Sheep.Hunger.AttributeSatisfied()) {
            component.Remove();
            Sheep.FoundFood = false;
            ClosestFood = null;
            Sheep.OnActionEnd();
        }
    }

    public override void OnFixedUpdate() {

    }

    public override void OnEnd() {

    }
}
