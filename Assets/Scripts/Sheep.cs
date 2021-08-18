using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Sheep : MonoBehaviour {
    [HideInInspector]
    public Movement movement;
    public Vector3 destination;

    // This needs to be public for other sheep scripts to use
    [HideInInspector]
    public NavMeshAgent agent;

    [Header("Attributes")]
    public float Hunger = 0f;
    private float HungerAdditionPerFrame = 0.01f;
    public int ViewRadius = 10;
    public int maxWanderDistance = 10;
    private float MaxHungerTolerence = 1f; // The max amount of hunger before dying
    public float Tiredness = 0f;
    private float TirednessAdditionPerFrame = 0.01f;

    [Header("Debug")]
    public bool FoundFood = false;
    public List<Transform> FoodInRange;

    // Random is used after resting, forward for everything else
    //public Movements moveType = Movements.Forward;
    [Header("Queue")]
    public Queue ActionQueue;

    private void Start() {
        agent = GetComponent<NavMeshAgent>();
        FoodInRange = new List<Transform>();
        movement = GetComponent<Movement>();
        ActionQueue = GetComponent<Queue>();

        MaxHungerTolerence = Random.Range(0.8f, 1.2f);
        ViewRadius = Random.Range(8, 12);
    }

    private void FixedUpdate() {
        // If we have no actions in the queue, add a wander action.
        // We only add wander here as when we get hungry or want to mate, those
        // methods will add to the queue.
        if (ActionQueue.Empty()) {
            // Choose either to graze or wander randomly
            int rand = Random.Range(0, 2);
            Debug.Log(rand);
            if (rand > 0) {
                Action wanderingAction = ScriptableObject.CreateInstance<WanderingAction>();
                ActionQueue.Add(wanderingAction);
            } else {
                Action idleAction = ScriptableObject.CreateInstance<IdleAction>();
                ActionQueue.Add(idleAction);
            }
        }

        TrackHunger();
        TrackTiredness();
        GetFoodInRange();

        ProcessActionQueue();
    }

    private bool CanEat() {
        if (ActionQueue.CurrentAction == null) {
            return true;
        }

        if (ActionQueue.CurrentAction.Type != Actions.Eating) {
            return true;
        }

        return false;
    }

    private void TrackHunger() {
        if (Hunger >= 0.8f) {
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
    }

    private void TrackTiredness() {
        if (Tiredness >= 1f) {
            if (!ActionQueue.HasItem(Actions.Resting)) {
                Action restAction = ScriptableObject.CreateInstance<RestAction>();
                ActionQueue.Add(restAction);
            }
        }
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

        UpdateAttributes();

        if (Hunger >= MaxHungerTolerence) {
            Die();
        }
    }

    public void OnActionEnd() {
        ActionQueue.ResetCurrent();
    }


    private void UpdateAttributes() {
        UpdateHunger();
        UpdateTiredness();
    }

    private void UpdateHunger() {
        // We could check for the currentAction == Actions.Eating but that action is a bit
        // misleading because the animal doesn't actually eat until it finds food.
        if (!FoundFood) {
            Hunger += Time.deltaTime * HungerAdditionPerFrame;
        }
    }

    private void UpdateTiredness() {
        // Don't update if we're resting
        if (ActionQueue.CurrentAction != null && ActionQueue.CurrentAction.Type == Actions.Resting) {
            return;
        }

        Tiredness += Time.deltaTime * TirednessAdditionPerFrame;
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

    private void OnDrawGizmosSelected() {
        if (Application.isPlaying) {
            foreach (Transform food in FoodInRange) {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(transform.position, food.position);
            }
        }
    }

    private void Die() {
        Destroy(gameObject);
    }
}
