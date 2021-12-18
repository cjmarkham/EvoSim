using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Sheep : MonoBehaviour {
    [HideInInspector]
    public Movement Movement;
    [HideInInspector]
    public Vector3 Destination;
    [HideInInspector]
    public NavMeshAgent Agent;
    [HideInInspector]
    public Queue ActionQueue;

    [Header("Attributes")]
    public Attribute Hunger;
    public Attribute Thirst;
    public Attribute Tiredness;
    private float HungerAdditionPerFrame = 0.01f;
    private float ThirstAdditionPerFrame = 0.02f;
    private float TirednessAdditionPerFrame = 0.005f;
    private int MaxHungerTolerence = 1; // The max amount of hunger before dying
    private int MaxThirstTolerence = 1; // The max amount of thirst before dying
    private int MaxTirednessTolerence = 1; // The max amount of thirst before dying

    public int ViewRadius = 10;
    public int MaxWanderDistance = 10;

    [Header("Debug")]
    public bool FoundFood = false;
    public bool FoundWater = false;
    public List<Transform> FoodInRange;
    public List<Transform> DrinkingSpotsInRange;

    private ProgressBar hungerProgress;
    private ProgressBar thirstProgress;

    private void Start() {
        Agent = GetComponent<NavMeshAgent>();
        FoodInRange = new List<Transform>();
        Movement = GetComponent<Movement>();
        ActionQueue = GetComponent<Queue>();

        Hunger = new Attribute(0f, HungerAdditionPerFrame, MaxHungerTolerence);
        Thirst = new Attribute(0f, ThirstAdditionPerFrame, MaxThirstTolerence);
        Tiredness = new Attribute(0f, TirednessAdditionPerFrame, MaxTirednessTolerence);

        Transform stats = transform.Find("Stats");

        hungerProgress = stats.Find("Hunger Bar").gameObject.GetComponentInChildren<ProgressBar>();
        thirstProgress = stats.Find("Thirst Bar").gameObject.GetComponentInChildren<ProgressBar>();
    }

    private void FixedUpdate() {
        // If we only have 1 action in the queue, add either a wander or graze action.
        // We only add those here as when we get hungry or want to mate, those
        // methods will add to the queue.
        if (ActionQueue.Length() < 2) {
            // Choose either to graze or wander randomly
            int rand = Random.Range(0, 2);
            if (rand > 0) {
                Action wanderingAction = ScriptableObject.CreateInstance<WanderingAction>();
                ActionQueue.Add(wanderingAction);
            } else {
                Action idleAction = ScriptableObject.CreateInstance<IdleAction>();
                ActionQueue.Add(idleAction);
            }
        }

        // Always get the food in range so if we get hungry, we can head to the closest one straight away
        GetFoodInRange();
        // Always get the water in range so if we get thirsty, we can head to the closest one straight away
        GetDrinkingSpotsInRange();

        // Process the actions in the queue and remove them 
        ProcessActionQueue();

        TrackThresholds();
    }

    // Track if we're thirsty, hungry etc and perform actions if needed
    private void TrackThresholds() {
        if (Thirst.ThresholdReached) {
            // Only set the drinking action if we don't have an action or we do and
            // the action isn't drinking
            if (CanDrink()) {
                // Check if this item is already in the queue. This is more to prevent
                // it adding multiple since this is ran in a FixedUpdate
                if (!ActionQueue.HasItem(Actions.Drinking)) {
                    // Add this action to the queue
                    Action drinkAction = ScriptableObject.CreateInstance<DrinkingAction>();
                    ActionQueue.Add(drinkAction);
                }
            }
        }

        if (Hunger.ThresholdReached) {
            // Only set the eating action if we don't have an action or we do and
            // the action isn't eating
            if (CanEat()) {
                // Check if this item is already in the queue. This is more to prevent
                // it adding multiple since this is ran in a FixedUpdate
                if (!ActionQueue.HasItem(Actions.Eating)) {
                    // Add this action to the queue
                    Action eatAction = ScriptableObject.CreateInstance<EatingAction>();
                    ActionQueue.Add(eatAction);
                }
            }
        }

        if (Tiredness.ThresholdReached) {
            if (!ActionQueue.HasItem(Actions.Resting)) {
                Action restAction = ScriptableObject.CreateInstance<RestAction>();
                ActionQueue.Add(restAction);
            }
        }
    }

    private bool CanEat() {
        if (ActionQueue.CurrentAction == null) {
            return true;
        }

        return ActionQueue.CurrentAction.Type != Actions.Eating;
    }

    private bool CanDrink() {
        if (ActionQueue.CurrentAction == null) {
            return true;
        }

        return ActionQueue.CurrentAction.Type != Actions.Drinking;
    }

    private void ProcessActionQueue() {
        if (ActionQueue.Empty()) {
            return;
        }

        if (ActionQueue.CurrentAction != null) {
            // If the first action in the sorted list has a higher priority
            // then we cancel this action and pick the next one.
            // This can happen if the sheep is currently wandering and then gets hungry.
            // The eating action is added to the list so we need to cancel this one.
            Action nextAction = ActionQueue.First();
            if (nextAction.Priority > ActionQueue.CurrentAction.Priority) {
                OnActionEnd();
            } else {
                // Otherwise, let's continue with this action
                return;
            }
        }

        ActionQueue.StartNextAction();
    }

    private void Update() {
        if (ActionQueue.CurrentAction != null) {
            ActionQueue.CurrentAction.OnUpdate();
        }

        if (!FoundFood) {
            Hunger.Increment();
        }
        if (!FoundWater) {
            Thirst.Increment();
        }
        if (ActionQueue.CurrentAction != null && ActionQueue.CurrentAction.Type == Actions.Resting) {
            Tiredness.Increment();
        }

        // UI
        hungerProgress.current = Hunger.Value;
        thirstProgress.current = Thirst.Value;
    }

    public void OnActionEnd() {
        ActionQueue.ResetCurrent();
    }

    void GetFoodInRange() {
        // Get rid of old food since this may not be in our range any more
        FoodInRange.Clear();

        LayerMask mask = LayerMask.GetMask("Food");
        Collider[] collisions = Physics.OverlapSphere(transform.position, ViewRadius, mask);

        foreach (Collider c in collisions) {
            FoodInRange.Add(c.transform);
        }
    }

    void GetDrinkingSpotsInRange() {
        // Get rid of old spots since this may not be in our range any more
        DrinkingSpotsInRange.Clear();

        LayerMask mask = LayerMask.GetMask("DrinkSpot");
        Collider[] collisions = Physics.OverlapSphere(transform.position, ViewRadius, mask);

        foreach (Collider c in collisions) {
            DrinkingSpotsInRange.Add(c.transform);
        }
    }

    public Transform GetClosestFood() {
        Transform closestFood = null;
        float closestDistanceSqr = Mathf.Infinity;

        foreach (Transform food in FoodInRange) {
            Eatable component = food.GetComponent<Eatable>();

            // If this food is being targeted by another sheep, ignore it
            if (component.Targeted) {
                continue;
            }

            Vector3 directionToFood = food.position - transform.position;
            float dSqrToTarget = directionToFood.sqrMagnitude;

            if (dSqrToTarget < closestDistanceSqr) {
                closestDistanceSqr = dSqrToTarget;
                closestFood = food;
                component.Targeted = true;
            }
        }

        return closestFood;
    }

    public Transform GetClosestDrinkingSpot() {
        Transform closestDrinkingSpot = null;
        float closestDistanceSqr = Mathf.Infinity;

        foreach (Transform spot in DrinkingSpotsInRange) {
            Drinkable component = spot.GetComponent<Drinkable>();

            // If this water is being targeted by another sheep, ignore it
            if (component.Targeted) {
                continue;
            }

            Vector3 directionToSpot = spot.position - transform.position;
            float dSqrToTarget = directionToSpot.sqrMagnitude;

            if (dSqrToTarget < closestDistanceSqr) {
                closestDistanceSqr = dSqrToTarget;
                closestDrinkingSpot = spot;
                component.Targeted = true;
            }
        }

        return closestDrinkingSpot;
    }

    private void OnDrawGizmosSelected() {
        if (Application.isPlaying) {
            foreach (Transform food in FoodInRange) {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, food.position);
            }

            foreach (Transform spot in DrinkingSpotsInRange) {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(transform.position, spot.position);
            }
        }
    }
}
