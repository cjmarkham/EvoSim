using UnityEngine;

public class FindingMate : Action {
    public override int Priority => 40;

    public override Actions Type => Actions.FindingMate;

    private Sheep Sheep;

    private Vector3 Destination;

    private GameObject ClosestMate;

    public FindingMate() {
    }

    public override void OnStart(Sheep sheep) {
        Sheep = sheep;

        ClosestMate = GetClosestMate();

        // If there's no sheep close, we have no choice to wander until we find some
        if (ClosestMate == null) {
            Sheep.Movement.GetRandomDestination(sheep.MaxWanderDistance, out Destination);
        } else {
            Sheep.FoundMate = true;
            Destination = ClosestMate.transform.position;
            Action materAction = new Mating(ClosestMate);
            Sheep.ActionQueue.Add(materAction);

            Sheep otherSheep = ClosestMate.GetComponent<Sheep>();
            otherSheep.IsMateTargeted = true;
            // Make sure the other sheep stops so we can catch up to it
            otherSheep.Agent.ResetPath();
            otherSheep.ActionQueue.Pause(true);
        }

        sheep.Agent.SetDestination(Destination);
    }

    public override void OnUpdate() {
        if (!Sheep.Agent.pathPending && Sheep.Agent.remainingDistance <= 0.8f) {
            OnEnd();
        }
    }

    public override void OnFixedUpdate() {
        
    }

    public override void OnEnd() {
        if (!Sheep.FoundMate) {
            Debug.Log("Didn't find mate, retrying");
            Action findMateAction = new FindingMate();
            Sheep.ActionQueue.Add(findMateAction);

            Sheep.OnActionEnd();
            return;
        }

        // Aww
        ClosestMate.transform.LookAt(Sheep.gameObject.transform);

        Sheep.OnActionEnd();
    }

    private GameObject GetClosestMate() {
        LayerMask mask = LayerMask.GetMask("Sheep");
        Collider[] collisions = Physics.OverlapSphere(Sheep.transform.position, Sheep.ViewRadius, mask);

        if (collisions.Length == 0) {
            return null;
        }

        float closestDistance = Mathf.Infinity;
        GameObject closestMate = null;

        foreach (Collider c in collisions) {
            GameObject possibleMate = c.gameObject;
            Sheep possibleMateComponent = c.GetComponent<Sheep>();

            if (possibleMateComponent.gender == Gender.Male) {
                continue;
            }

            // Sorry, you aren't allowed to mate with yourself...
            if (possibleMate == Sheep.gameObject) {
                continue;
            }

            // Can't allow this, sorry
            if (Sheep.Father != null && possibleMate == Sheep.Father) {
                continue;
            }

            // Can't allow this, sorry
            if (Sheep.Mother != null && possibleMate == Sheep.Mother) {
                continue;
            }

            if (Sheep.Children.Count > 0) {
                if (Sheep.Children.Contains(possibleMateComponent)) {
                    continue;
                }
            }

            float distance = Vector3.Distance(c.transform.position, Sheep.transform.position);

            if (distance < closestDistance) {
                closestDistance = distance;
                closestMate = possibleMate;
            }
        }

        return closestMate;
    }
}