using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Sheep : MonoBehaviour
{
    private Movement movement;
    public Actions actionType;
    public Action action;

    Vector3 destination;

    // This needs to be public for other sheep scripts to use
    [HideInInspector]
    public NavMeshAgent agent;

    [Header("Attributes")]
    public float hunger = 0f;
    public float tiredness = 0f;
    public int viewRadius = 3;
    public int maxWanderDistance = 3;
    public float reproductiveUrge = 0f;

    [Header("Debug")]
    public bool foundFood = false;
    public bool foundMate = false;
    // Used if another sheep wants to mate with us
    public bool beingTargeted = false;
    public bool eligableToMate = false;

    private float lastActionTime = 0f;

    public List<Transform> foodInRange;
    public Transform foodTarget;

    private List<Transform> matableSheepInRange;
    public Sheep mate;
    [HideInInspector]
    public float timeSinceLastMate = 0f;

    // Random is used after resting, forward for everything else
    public Movements moveType = Movements.Forward;

    public Genders gender;
    public bool isChild = false;

    public GameObject loveParticles;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        foodInRange = new List<Transform>();
        matableSheepInRange = new List<Transform>();
        movement = GetComponent<Movement>();

        // Pick a random gender
        //int random = Random.Range(0, 1);
        //if (random == 1)
        //{
        //    gender = Genders.Male;
        //}
        //else
        //{
        //    gender = Genders.Female;
        //}
    }

    private void FixedUpdate()
    {
        GetFoodInRange();
        GetMatableSheepInRange();
    }

    private void Update()
    {
        DoDebug();
        UpdateAttributes();

        // We aren't doing anything so pick an action
        if (actionType == Actions.None)
        {
            GetNextAction();
        }

        // Even if we have a current action, we want to interupt it
        // if we get hungry. This solves the issue where the sheep will only
        // search for food after it has finished it's current movement
        if (IsHungry() || WantsToMate())
        {
            GetNextAction();
        }

        Act();

        if (!agent.pathPending && agent.remainingDistance <= 0.1f)
        {
            switch(actionType)
            {
                case Actions.FindingFood:
                    if (foundFood)
                    {
                        actionType = Actions.Eating;
                        action = new EatingAction(this);
                    }
                    else
                    {
                        // We didn't find any food so set action to None so we can
                        // get a new action
                        ResetAction();
                    }
                    break;
                case Actions.FindingMate:
                    if (foundMate)
                    {
                        actionType = Actions.Mating;
                        action = new MatingAction(this, mate);
                        particleInstance = Instantiate(loveParticles, agent.destination, Quaternion.Euler(-90f, 0, 0));
                    }
                    else
                    {
                        // We didn't find a mate so set action to None so we can
                        // get a new action
                        ResetAction();
                    }
                    break;
                case Actions.Wandering:
                    ResetAction();
                    break;
            }

            moveType = Movements.Forward;
        }
    }

    // GetNextAction will decide what action to take next, based on priorities
    // Priority list:
    // Food
    // Mating
    // Wandering
    private void GetNextAction()
    {
        lastActionTime = Time.time;

        // Being hungry takes precedence over everything ATM
        if (IsHungry())
        {
            if (!foundFood)
            {
                FindFood();
            }
            actionType = Actions.FindingFood;
        }
        else if (WantsToMate())
        {
            if (!foundMate)
            {
                FindMate();
            }
            actionType = Actions.FindingMate;
        }
        else
        {
            movement.GetDestination(transform, maxWanderDistance, moveType == Movements.Random, out destination);
            actionType = Actions.Wandering;
        }

        //if (IsTired())
        //{
        //    return Actions.Resting;
        //}

        agent.SetDestination(destination);
    }

    private void Act()
    {
        switch (actionType)
        {
            case Actions.Resting:
                Rest();
                break;
            case Actions.Eating:
                Eat();
                break;
            case Actions.Mating:
                Mate();
                break;
        }
    }

    private void UpdateAttributes()
    {
        // Only the males want to do the nasty
        if (actionType != Actions.Mating && gender == Genders.Male && !isChild)
        {
            reproductiveUrge += Time.deltaTime * 0.008f;
        }
        if (actionType != Actions.Resting)
        {
            tiredness += Time.deltaTime * 0.005f;
        }
        if (actionType != Actions.Eating)
        {
            hunger += Time.deltaTime * 0.1f;
        }
    }

    private void FindMate()
    {
        Sheep closestMate = GetClosestMate();
        if (closestMate)
        {
            foundMate = true;
            // We don't want to path find to the center of the mate so apply
            // some offsets
            destination = closestMate.transform.position + new Vector3(0.6f, 0, 0.6f);
            mate = closestMate;

            // We need to stop the mate otherwise it will just continue to wander off
            mate.agent.isStopped = true;
        }
        else
        {
            movement.GetDestination(transform, maxWanderDistance, moveType == Movements.Random, out destination);
        }
    }

    // FindFood will return either the position of the closest food, or a random point to wander to
    private void FindFood()
    {
        if (foundFood)
        {
            return;
        }

        Transform closestFood = GetClosestFood();
        if (closestFood)
        {
            foundFood = true;
            destination = closestFood.position;
            foodTarget = closestFood;
        }
        else
        {
            movement.GetDestination(transform, maxWanderDistance, moveType == Movements.Random, out destination);
        }
    }

    private GameObject particleInstance;

    private void Mate()
    {
        // We don't want the female sheep to run it's mate
        // function, otherwise we would get issues when spawning
        // Instead, we can control the mates script from here
        if (gender == Genders.Female)
        {
            return;
        }

        // Look at each other (Aww)
        // TODO: Currently this just snaps to the rotation, make it smoother
        mate.transform.LookAt(transform);
        transform.LookAt(mate.transform);

        action.OnUpdate();

        float timeSinceStartedMating = Time.time - lastActionTime;

        // TODO: How long do we want to mate for?
        if (timeSinceStartedMating > 5f)
        {
            action.OnEnd();

            particleInstance.GetComponent<ParticleSystem>().Stop();
            Destroy(particleInstance, 1f);
            SpawnBabySheep();
        }
    }

    // This is called when looking for a mate
    public bool EligableMate()
    {
        if (isChild || gender == Genders.Male)
        {
            return false;
        }

        // TODO: Add mate interval to variable
        if (Time.time > timeSinceLastMate + 10f || timeSinceLastMate == 0f)
        {
            return true;
        }

        return false;
    }

    private void SpawnBabySheep()
    {
        Vector3 position = new Vector3(transform.position.x, 0.14f, transform.position.z);
        GameController gc = GameObject.Find("GameController").GetComponent<GameController>();

        GameObject baby = Instantiate(gc.sheep, position, Quaternion.Euler(0, 0, 0));
        baby.transform.localScale = new Vector3(10, 15, 10);
        Sheep sheepComponent = baby.GetComponent<Sheep>();
        sheepComponent.isChild = true;
    }

    private void Rest()
    {
        action.OnUpdate();

        float timeSinceStartedResting = Time.time - lastActionTime;

        if (timeSinceStartedResting > 5f)
        {
            action.OnEnd();
        }
    }

    private void Eat()
    {
        action.OnUpdate();

        float timeSinceStartedEating = Time.time - lastActionTime;

        if (timeSinceStartedEating > 5f)
        {
            action.OnEnd();
        }
    }

    public void ResetAction()
    {
        actionType = Actions.None;
        action = null;
    }

    public bool WantsToMate()
    {
        return reproductiveUrge >= 1f && actionType != Actions.FindingMate && actionType != Actions.Mating;
    }

    private bool IsHungry()
    {
        return hunger >= 0.8f && actionType != Actions.FindingFood && actionType != Actions.Eating;
    }

    private bool IsTired()
    {
        return tiredness >= 0.8f;
    }

    void GetFoodInRange()
    {
        // Get rid of old food since this may not be in our range any more
        foodInRange.Clear();

        LayerMask mask = LayerMask.GetMask("Food");
        Collider[] collisions = Physics.OverlapSphere(transform.position, viewRadius, mask);

        foreach (Collider c in collisions)
        {
            foodInRange.Add(c.transform);
        }
    }

    void GetMatableSheepInRange()
    {
        // Get rid of old mates since this may not be in our range any more
        matableSheepInRange.Clear();

        LayerMask mask = LayerMask.GetMask("Sheep");
        Collider[] collisions = Physics.OverlapSphere(transform.position, viewRadius, mask);

        foreach (Collider c in collisions)
        {
            // Don't mate with ourselves !!!
            if (c.gameObject == this.gameObject)
            {
                continue;
            }

            Sheep sheepComponent = c.gameObject.GetComponent<Sheep>();
            if (!sheepComponent.EligableMate())
            {
                continue;
            }

            matableSheepInRange.Add(c.transform);
        }
    }

    private Transform GetClosestFood()
    {
        Transform closestFood = null;
        float closestDistanceSqr = Mathf.Infinity;

        foreach (Transform food in foodInRange)
        {
            // If this food is being targeted by another sheep, ignore it
            if (food.GetComponent<Eatable>().beingTargeted)
            {
                continue;
            }

            Vector3 directionToFood = food.position - transform.position;
            float dSqrToTarget = directionToFood.sqrMagnitude;

            if (dSqrToTarget < closestDistanceSqr)
            {
                closestDistanceSqr = dSqrToTarget;
                closestFood = food;
                food.GetComponent<Eatable>().beingTargeted = true;
            }
        }

        return closestFood;
    }

    // TODO: Factor in desirability and gender
    private Sheep GetClosestMate()
    {
        Sheep closestMate = null;
        float closestDistanceSqr = Mathf.Infinity;

        foreach (Transform sheepTransform in matableSheepInRange)
        {
            Sheep sheep = sheepTransform.GetComponent<Sheep>();

            // If the other sheep is male or someone else has dibs, ignore it
            if (sheep.beingTargeted || sheep.gender == Genders.Male)
            {
                continue;
            }

            Vector3 directionToMate = sheep.transform.position - transform.position;
            float dSqrToTarget = directionToMate.sqrMagnitude;

            if (dSqrToTarget < closestDistanceSqr)
            {
                closestDistanceSqr = dSqrToTarget;
                closestMate = sheep;
                sheep.beingTargeted = true;
            }
        }

        return closestMate;
    }

    private void OnDrawGizmosSelected()
    {
        if (Application.isPlaying)
        {
            if (agent.hasPath)
            {
                Gizmos.DrawLine(transform.position, destination);
                Gizmos.DrawSphere(destination, 0.3f);
            }

            foreach(Transform food in foodInRange)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, food.position);
            }

            foreach (Transform sheep in matableSheepInRange)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(transform.position, sheep.position);
            }
        }
    }

    private void DoDebug()
    {
        eligableToMate = EligableMate();
    }
}
