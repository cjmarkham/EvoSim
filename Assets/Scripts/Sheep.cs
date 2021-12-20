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
    

    [Header("Hunger")]
    public Attribute Hunger;
    public float HungerValue = 0f;
    public bool ShouldEat = false;
    public bool FoundFood = false;
    private float HungerAdditionPerFrame = 0.01f;
    private float MaxHungerTolerence = 1f; // The max amount of hunger before dying

    [Header("Thirst")]
    public Attribute Thirst;
    public float ThirstValue = 0f;
    public bool ShouldDrink = false;
    public bool FoundWater = false;
    private float ThirstAdditionPerFrame = 0.02f;
    private float MaxThirstTolerence = 1f; // The max amount of thirst before dying

    [Header("Settings")]
    public int ViewRadius = 10;
    public int MaxWanderDistance = 10;

    private ProgressBar hungerProgress;
    private ProgressBar thirstProgress;

    private void Start() {
        Agent = GetComponent<NavMeshAgent>();
        Movement = GetComponent<Movement>();
        ActionQueue = GetComponent<Queue>();

        Hunger = new Attribute(0f, HungerAdditionPerFrame, MaxHungerTolerence);
        Thirst = new Attribute(0f, ThirstAdditionPerFrame, MaxThirstTolerence);

        Transform stats = transform.Find("Stats");
        hungerProgress = stats.Find("Hunger Bar").gameObject.GetComponentInChildren<ProgressBar>();
        thirstProgress = stats.Find("Thirst Bar").gameObject.GetComponentInChildren<ProgressBar>();
    }

    private void FixedUpdate() {
        // Always make sure we have a next action
        if (ActionQueue.Length() < 5) {
            // Choose either to graze or wander randomly
            int rand = Random.Range(0, 2);
            if (rand > 0) {
                Action wanderingAction = ScriptableObject.CreateInstance<Wandering>();
                ActionQueue.Add(wanderingAction);
            }
            else {
                Action idleAction = ScriptableObject.CreateInstance<Idle>();
                ActionQueue.Add(idleAction);
            }
        }

        // Process the actions in the queue and remove them 
        ProcessActionQueue();

        TrackThresholds();

        if (ActionQueue.CurrentAction != null) {
            ActionQueue.CurrentAction.OnFixedUpdate();
        }

        if (ShouldDie()) {
            Die();
        }
    }

    // Track if we're thirsty, hungry etc and perform actions if needed
    private void TrackThresholds() {
        if (Thirst.ShouldSatisfyAttribute() && !Drinking() && !FindingWater()) {
            // Check if this item is already in the queue. This is more to prevent
            // it adding multiple since this is ran in a FixedUpdate
            if (!ActionQueue.HasItem(Actions.FindingWater)) {
                // Add this action to the queue
                Action findWaterAction = ScriptableObject.CreateInstance<FindingWater>();
                ActionQueue.Add(findWaterAction);
            }
        }

        if (Hunger.ShouldSatisfyAttribute() && !Eating() && !FindingFood()) {
            // Check if this item is already in the queue. This is more to prevent
            // it adding multiple since this is ran in a FixedUpdate
            if (!ActionQueue.HasItem(Actions.FindingFood)) {
                // Add this action to the queue
                Action findFoodAction = ScriptableObject.CreateInstance<FindingFood>();
                ActionQueue.Add(findFoodAction);
            }
        }
    }

    private bool Eating() {
        if (ActionQueue.CurrentAction == null) {
            return false;
        }

        return ActionQueue.CurrentAction.Type == Actions.Eating;
    }

    private bool FindingFood() {
        if (ActionQueue.CurrentAction == null) {
            return false;
        }

        return ActionQueue.CurrentAction.Type == Actions.FindingFood;
    }
    
    private bool Drinking() {
        if (ActionQueue.CurrentAction == null) {
            return false;
        }

        return ActionQueue.CurrentAction.Type == Actions.Drinking;
    }

    private bool FindingWater() {
        if (ActionQueue.CurrentAction == null) {
            return false;
        }

        return ActionQueue.CurrentAction.Type == Actions.FindingWater;
    }

    private void ProcessActionQueue() {
        // Should never happen but let's account for it anyway
        if (ActionQueue.Empty()) {
            return;
        }

        if (ActionQueue.CurrentAction != null) {
            // If the next action in the sorted list has a higher priority than the current action
            // then we cancel this action and pick the next one.
            // This can happen if the sheep is currently wandering and then gets hungry.
            // The eating action is added to the list so we need to cancel this one.
            Action nextAction = ActionQueue.First();
            if (nextAction.Priority > ActionQueue.CurrentAction.Priority) {
                Debug.Log("Ending " + ActionQueue.CurrentAction.Type.ToString() + " as " + nextAction.Type.ToString() + " has higher priority");
                OnActionEnd();
            }
        }

        // We only start the next action if no current action
        if (ActionQueue.CurrentAction == null) {
            ActionQueue.StartNextAction();
        }
    }

    private void Update() {
        if (ActionQueue.CurrentAction != null) {
            ActionQueue.CurrentAction.OnUpdate();
        }

        // We only get hungry if we're not currently eating
        if (ActionQueue.CurrentAction != null && ActionQueue.CurrentAction.Type != Actions.Eating) {
            Hunger.Increment();
        }

        // We only get thirsty if we're not currently drinking
        if (ActionQueue.CurrentAction != null && ActionQueue.CurrentAction.Type != Actions.Drinking) {
            Thirst.Increment();
        }

        // UI
        hungerProgress.current = Hunger.Value;
        thirstProgress.current = Thirst.Value;

        // Debug
        HungerValue = Hunger.Value;
        ShouldEat = Hunger.ShouldSatisfyAttribute();

        ThirstValue = Thirst.Value;
        ShouldDrink = Thirst.ShouldSatisfyAttribute();
    }

    public void OnActionEnd() {
        Debug.Log("OnActionEnd - " + ActionQueue.CurrentAction.Type.ToString());
        ActionQueue.StartNextAction();
    }

    private bool ShouldDie() {
        // TODO: These values are both 1
        if (Hunger.DangerThresholdReached && Thirst.DangerThresholdReached) {
            return true;
        }

        return false;
    }

    private void Die() {
        Destroy(gameObject);
    }
}
