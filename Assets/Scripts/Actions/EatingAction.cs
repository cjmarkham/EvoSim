public class EatingAction : Action
{
    private readonly Sheep sheep;

    public EatingAction(Sheep sheep)
    {
        this.sheep = sheep;
        this.sheep.agent.ResetPath();
    }

    public override void OnUpdate()
    {
        
    }

    public override void OnEnd()
    {
        sheep.hunger = 0f;
        sheep.tiredness = 0f; // We've being stationary while eating
        sheep.foodInRange.Clear();
        sheep.foundFood = false;
        Destroy(sheep.foodTarget.gameObject);
        sheep.foodTarget = null;
        sheep.moveType = Movements.Random;

        sheep.ResetAction();
    }
}
