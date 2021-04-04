public class RestingAction : Action
{
    private readonly Sheep sheep;

    public RestingAction(Sheep sheep)
    {
        this.sheep = sheep;
        this.sheep.agent.ResetPath();
    }

    public override void OnUpdate()
    {
        
    }

    public override void OnEnd()
    {
        sheep.tiredness = 0f;
        sheep.moveType = Movements.Random;
        sheep.ResetAction();
    }
}
