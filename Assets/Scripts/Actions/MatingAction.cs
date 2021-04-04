using UnityEngine;

public class MatingAction : Action
{
    private readonly Sheep mater;
    private readonly Sheep matee;

    public MatingAction(Sheep mater, Sheep matee)
    {
        this.mater = mater;
        this.matee = matee;

        mater.agent.ResetPath();
        matee.agent.ResetPath();

        matee.actionType = Actions.Mating;
        matee.agent.isStopped = true;
        matee.beingTargeted = true;
    }

    public override void OnUpdate()
    {

    }

    public override void OnEnd()
    {
        mater.reproductiveUrge = 0f;
        mater.moveType = Movements.Random;
        mater.timeSinceLastMate = Time.time;
        mater.foundMate = false;

        matee.moveType = Movements.Random;
        matee.beingTargeted = false;
        matee.agent.isStopped = false;
        matee.timeSinceLastMate = Time.time;

        mater.mate = null; // oof

        mater.ResetAction();
        matee.ResetAction();
    }
}
