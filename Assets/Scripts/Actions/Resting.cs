using UnityEngine;

public class Resting : Action {
    public override int Priority => 30;

    public override Actions Type => Actions.Resting;

    private Sheep Sheep;

    public Resting() {
    }

    public override void OnStart(Sheep sheep) {
        Sheep = sheep;
        Sheep.Agent.ResetPath();
    }

    public override void OnUpdate() {
        
    }

    public override void OnFixedUpdate() {

    }

    public override void OnEnd() {

    }
}
