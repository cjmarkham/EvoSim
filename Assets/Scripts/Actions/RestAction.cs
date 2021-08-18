using UnityEngine;

public class RestAction : Action {
    public override int Priority => 30;

    public override Actions Type => Actions.Resting;

    private Sheep sheep;

    public RestAction() {
        Debug.Log("[NEW ACTION] Rest");
    }

    public override void OnStart(Sheep sheep) {
        Debug.Log("[ACTION START] Resting");
        this.sheep = sheep;
        this.sheep.agent.ResetPath();
    }

    public override void OnUpdate() {
        if (!sheep.agent.pathPending && sheep.agent.remainingDistance <= 0.1f) {
            sheep.Tiredness -= Time.deltaTime * 0.1f;

            if (sheep.Tiredness <= 0f) {
                sheep.Tiredness = 0f;
                sheep.OnActionEnd();
            }
        }
    }

    public override void OnEnd() {

    }
}
