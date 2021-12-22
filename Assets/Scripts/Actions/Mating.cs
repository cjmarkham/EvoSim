using UnityEngine;

public class Mating : Action {
    public override int Priority => 40;

    public override Actions Type => Actions.Mating;

    private Sheep Sheep;

    public GameObject ClosestMate;

    public Mating(GameObject closestMate) {
        ClosestMate = closestMate;
    }

    public override void OnStart(Sheep sheep) {
        Sheep = sheep;
        Sheep.Agent.ResetPath();
    }

    public override void OnUpdate() {
        Sheep.ReproductiveUrge.Decrement();

        if (Sheep.ReproductiveUrge.AttributeSatisfied()) {
            Sheep.FoundMate = false;

            Sheep otherSheep = ClosestMate.GetComponent<Sheep>();
            otherSheep.IsMateTargeted = false;
            otherSheep.ActionQueue.Pause(false);

            ClosestMate = null;
            Sheep.OnActionEnd();

            GameController gc = GameObject.Find("GameController").GetComponent<GameController>();

            Vector3 babyPosition = new Vector3(Sheep.transform.position.x, 1f, Sheep.transform.position.z);
            GameObject babySheep = Instantiate(gc.Sheep, babyPosition, Quaternion.Euler(0, 0, 0));
            babySheep.transform.localScale = new Vector3(7f, 12f, 7f);

            Sheep babySheepComponent = babySheep.GetComponent<Sheep>();
            babySheepComponent.Mother = otherSheep;
            babySheepComponent.Father = Sheep;

            otherSheep.Children.Add(babySheepComponent);
            Sheep.Children.Add(babySheepComponent);
        }
    }

    public override void OnFixedUpdate() {

    }

    public override void OnEnd() {

    }
}
