using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Sheep : MonoBehaviour
{
    private List<Transform> foodInRange;
    private Movement movement;
    public Actions action;

    Vector3 destination;
    NavMeshAgent agent;

    [Header("Attributes")]
    public float hunger = 0f;
    public float tiredness = 0f;
    public int viewRadius = 3;
    public int maxWanderDistance = 3;

    private float lastActionTime = 0f;

    private bool foundFood = false;
    private Transform foodTarget;

    // Random is used after resting, forward for everything else
    public Movements moveType = Movements.Forward;

    private void Start()
    {
        action = Actions.None;
        agent = GetComponent<NavMeshAgent>();
        foodInRange = new List<Transform>();
        movement = GetComponent<Movement>();
    }

    private void FixedUpdate()
    {
        GetFoodInRange();
    }

    private void Update()
    {
        UpdateAttributes();

        // We aren't doing anything so pick an action
        if (action == Actions.None)
        {
            action = GetNextAction();
        }

        // If we are hungry and either wandering or resting, we need to interupt
        // the action and find food
        if (IsHungry() && action != Actions.FindingFood && action != Actions.Eating)
        {
            action = GetNextAction(); // This prioritises food over resting/wandering
        }

        Act();

        if (!agent.pathPending && agent.remainingDistance <= 0.1f)
        {
            if (action == Actions.Wandering || action == Actions.FindingFood)
            {
                if (foundFood)
                {
                    action = Actions.Eating;
                }
                else
                {
                    action = Actions.None;
                }
            }
            moveType = Movements.Forward;
        }
    }

    private void UpdateAttributes()
    {
        if (action != Actions.Resting)
        {
            tiredness += Time.deltaTime * 0.05f;
        }
        if (action != Actions.Eating)
        {
            hunger += Time.deltaTime * 0.2f;
        }
    }

    private Actions GetNextAction()
    {
        lastActionTime = Time.time;

        // Being hungry takes precedence over everything ATM
        if (IsHungry())
        {
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
            return Actions.FindingFood;
        }

        if (IsTired())
        {
            return Actions.Resting;
        }

        movement.GetDestination(transform, maxWanderDistance, moveType == Movements.Random, out destination);
        return Actions.Wandering;
    }

    private void Act()
    {
        switch (action)
        {
            case Actions.Resting:
                Rest();
                break;
            case Actions.Wandering:
            case Actions.FindingFood:
                agent.SetDestination(destination);
                break;
            case Actions.Eating:
                Eat();
                break;
        }
    }

    private void Rest()
    {
        agent.ResetPath();

        float timeSinceStartedResting = Time.time - lastActionTime;

        if (timeSinceStartedResting > 5f)
        {
            Debug.Log("Finished resting");
            tiredness = 0f;
            action = Actions.None;
            moveType = Movements.Random;
        }
    }

    private void Eat()
    {
        agent.ResetPath();

        float timeSinceStartedEating = Time.time - lastActionTime;

        if (timeSinceStartedEating > 5f)
        {
            Debug.Log("Finished eating");
            hunger = 0f;
            tiredness = 0f; // We've being stationary while eating
            action = Actions.None;
            foodInRange.Clear();
            foundFood = false;
            Destroy(foodTarget.gameObject);
            foodTarget = null;
            moveType = Movements.Random;
        }
    }

    private bool IsHungry()
    {
        return hunger >= 0.8f;
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

    private void OnDrawGizmosSelected()
    {
        if (Application.isPlaying)
        {
            if (agent.hasPath)
            {
                Gizmos.DrawLine(transform.position, destination);
            }

            foreach(Transform food in foodInRange)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, food.position);
            }
        }
    }
}
