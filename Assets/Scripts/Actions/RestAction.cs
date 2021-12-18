public class RestAction : Action {
    public override int Priority => 30;

    public override Actions Type => Actions.Resting;

    private Sheep Sheep;

    public RestAction() {
    }

    public override void OnStart(Sheep sheep) {
        Sheep = sheep;
        Sheep.Agent.ResetPath();
    }

    public override void OnUpdate() {
        if (!Sheep.Agent.pathPending && Sheep.Agent.remainingDistance <= 0.1f) {
            Sheep.Tiredness.Decrement();

            if (!Sheep.Tiredness.ThresholdReached) {
                Sheep.OnActionEnd();
            }
        }
    }

    public override void OnEnd() {

    }
}
