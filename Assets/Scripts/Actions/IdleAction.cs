using UnityEngine;

public class IdleAction : Action {
    public override int Priority => 1;

    public override Actions Type => Actions.Idle;

    private Sheep sheep;

    // The amount of time we will be idle for
    private float IdleTimer;

    public IdleAction() {
    }

    public override void OnStart(Sheep sheep) {
        this.sheep = sheep;
        this.sheep.agent.ResetPath();
        IdleTimer = Random.Range(0.5f, 1.5f);
    }

    public override void OnUpdate() {
        IdleTimer -= Time.deltaTime * 0.5f;

        if (IdleTimer <= 0f) {
            this.sheep.OnActionEnd();
        }
    }

    public override void OnEnd() {

    }
}
