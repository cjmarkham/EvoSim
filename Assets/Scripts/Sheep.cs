using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Sheep : MonoBehaviour {
    [HideInInspector]
    public Movement Movement;
    [HideInInspector]
    public Vector3 Destination;
    [HideInInspector]
    public NavMeshAgent Agent;
    public Queue ActionQueue;
    
    [Header("Hunger")]
    public Attribute Hunger;
    public float HungerValue = 0f;
    public bool ShouldEat = false;
    public bool FoundFood = false;
    private float HungerIncrementPerFrame = 0.01f;
    private float MaxHungerTolerence = 1f; // The max amount of hunger before dying
    public bool OverrideHungerNeed = false;
    public float HungerDecrementPerFrame = 0.02f;

    [Header("Thirst")]
    public Attribute Thirst;
    public float ThirstValue = 0f;
    public bool ShouldDrink = false;
    public bool FoundWater = false;
    private float ThirstIncrementPerFrame = 0.02f;
    private float MaxThirstTolerence = 1f; // The max amount of thirst before dying
    public bool OverrideThirstNeed = false;
    public float ThirstDecrementPerFrame = 0.04f;

    [Header("Mating")]
    public Attribute ReproductiveUrge;
    public float UrgeValue = 0f;
    public bool WantsToMate = false;
    public bool FoundMate = false;
    private float UrgeIncrementPerFrame = 0.01f;
    public bool OverrideMatingNeed = false;
    public float UrgeDecrementPerFrame = 0.08f;
    public Sheep Mother;
    public Sheep Father;
    public List<Sheep> Children;
    // Used to check if someone wants to mate
    public bool IsMateTargeted = false;

    [Header("Settings")]
    public int ViewRadius = 10;
    public int MaxWanderDistance = 10;
    public bool OverrideWalking = false;
    public float HP = 40f;
    public float MaxHP = 100f;
    private float HPMultiplyFactor = 2f;

    private ProgressBar hungerProgress;
    private ProgressBar thirstProgress;
    private ProgressBar hpProgress;

    public GameObject Poop;
    public Gender gender;

    

    private void Start() {
        Agent = GetComponent<NavMeshAgent>();
        Movement = GetComponent<Movement>();

        ActionQueue = new Queue(this);

        Hunger = new Attribute(0f, HungerIncrementPerFrame, HungerDecrementPerFrame, MaxHungerTolerence);
        Thirst = new Attribute(0f, ThirstIncrementPerFrame, ThirstDecrementPerFrame, MaxThirstTolerence);

        // TODO: Remove this once debugging done
        if (gender == Gender.Male) {
            ReproductiveUrge = new Attribute(0f, UrgeIncrementPerFrame, UrgeDecrementPerFrame, 2f);
        } else {
            ReproductiveUrge = new Attribute(0f, 0f, 0f, 2f);
        }

        Transform stats = transform.Find("Stats");
        hungerProgress = stats.Find("Hunger Bar").gameObject.GetComponentInChildren<ProgressBar>();
        thirstProgress = stats.Find("Thirst Bar").gameObject.GetComponentInChildren<ProgressBar>();
        hpProgress = stats.Find("HP Bar").gameObject.GetComponentInChildren<ProgressBar>();

        // TODO: Randomise gender

        // Debug gender
        Material material = GetComponent<Renderer>().material;
        if (gender == Gender.Male) {
            material.color = Color.blue;
        } else {
            material.color = Color.magenta;
        }
    }

    private void FixedUpdate() {
        // Always make sure we have a next action unless someone is heading over to mate with us
        if (ActionQueue.Length() < 5 && !OverrideWalking && !IsMateTargeted) {
            // Choose either to graze or wander randomly
            int rand = Random.Range(0, 2);
            if (rand > 0) {
                Action wanderingAction = new Wandering();
                ActionQueue.Add(wanderingAction);
            }
            else {
                Action idleAction = new Idle();
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
        if (Thirst.ShouldSatisfyAttribute() && !Drinking() && !FindingWater() && !OverrideThirstNeed) {
            // Check if this item is already in the queue. This is more to prevent
            // it adding multiple since this is ran in a FixedUpdate
            if (!ActionQueue.HasItem(Actions.FindingWater)) {
                // Add this action to the queue
                Action findWaterAction = new FindingWater();
                ActionQueue.Add(findWaterAction);
            }
        }

        if (Hunger.ShouldSatisfyAttribute() && !Eating() && !FindingFood() && !OverrideHungerNeed) {
            // Check if this item is already in the queue. This is more to prevent
            // it adding multiple since this is ran in a FixedUpdate
            if (!ActionQueue.HasItem(Actions.FindingFood)) {
                // Add this action to the queue
                Action findFoodAction = new FindingFood();
                ActionQueue.Add(findFoodAction);
            }
        }

        if (ReproductiveUrge.ShouldSatisfyAttribute() && !Mating() && !FindingMate() && !OverrideMatingNeed) {
            if (!ActionQueue.HasItem(Actions.FindingMate)) {
                Action findMateAction = new FindingMate();
                ActionQueue.Add(findMateAction);
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

    private bool Mating() {
        if (ActionQueue.CurrentAction == null) {
            return false;
        }

        return ActionQueue.CurrentAction.Type == Actions.Mating;
    }

    private bool FindingMate() {
        if (ActionQueue.CurrentAction == null) {
            return false;
        }

        return ActionQueue.CurrentAction.Type == Actions.FindingMate;
    }

    private void ProcessActionQueue() {
        // Should never happen but let's account for it anyway
        if (ActionQueue.Empty()) {
            return;
        }

        if (ActionQueue.Paused()) {
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
        if (ActionQueue.CurrentAction != null && !ActionQueue.Paused()) {
            ActionQueue.CurrentAction.OnUpdate();
        }

        // TODO: These statements are long

        // We only get hungry if we're not currently eating
        if (ActionQueue.CurrentAction == null || (ActionQueue.CurrentAction != null && ActionQueue.CurrentAction.Type != Actions.Eating) && !OverrideHungerNeed) {
            Hunger.Increment();
        }

        // We only get thirsty if we're not currently drinking
        if (ActionQueue.CurrentAction == null || (ActionQueue.CurrentAction != null && ActionQueue.CurrentAction.Type != Actions.Drinking) && !OverrideThirstNeed) {
            Thirst.Increment();
        }

        // We only increase our reproductive urge if we're not currently mating and we're male
        if (gender == Gender.Male) {
            if (ActionQueue.CurrentAction == null || (ActionQueue.CurrentAction != null && ActionQueue.CurrentAction.Type != Actions.Mating) && !OverrideMatingNeed) {
                ReproductiveUrge.Increment();
            }
        }

        // UI
        hungerProgress.current = Hunger.Value;
        thirstProgress.current = Thirst.Value;
        hpProgress.current = HP / 100f;

        // Debug
        HungerValue = Hunger.Value;
        ShouldEat = Hunger.ShouldSatisfyAttribute();

        ThirstValue = Thirst.Value;
        ShouldDrink = Thirst.ShouldSatisfyAttribute();

        UrgeValue = ReproductiveUrge.Value;
        WantsToMate = ReproductiveUrge.ShouldSatisfyAttribute();

        UpdateHP();
    }

    public void OnActionEnd() {
        Debug.Log("Ending Action " + ActionQueue.CurrentAction.Type.ToString());

        // If we've just finished eating, spawn a poop
        if (ActionQueue.CurrentAction.Type == Actions.Eating) {
            Vector3 poopPosition = new Vector3(transform.position.x, 0.9f, transform.position.z);
            // TODO: Fix rotation in blender
            Instantiate(Poop, poopPosition, Quaternion.Euler(-90f, 0, 0));
        }

        // Only start a new action if there is one
        if (ActionQueue.Length() > 0) {
            ActionQueue.StartNextAction();
        } else {
            // If there's no more actions to do, we still want to remove the current action once done
            ActionQueue.ResetCurrent();
        }
    }

    private void UpdateHP() {
        if (Hunger.DangerThresholdReached || Thirst.DangerThresholdReached) {
            HP -= Time.deltaTime * HPMultiplyFactor;
        } else {
            // We're healthy so recover some HP
            if (HP < MaxHP) {
                HP += Time.deltaTime * HPMultiplyFactor;
            }
        }

        // Change sheep color depending on HP level
        // Need ugly if statements until c# 9 is supported
        Material material = gameObject.GetComponent<Renderer>().material;
        if (HP <= 25f) {
            material.color = Color.red;
        } else if (HP <= 50f) {
            material.color = Color.yellow;
        } else {
            material.color = Color.white;
        }
    }

    private bool ShouldDie() {
        return HP <= 0f;
    }

    private void Die() {
        Destroy(gameObject);

        string time = GameObject.Find("Timer").GetComponent<Text>().text;
        Debug.Log("Died at " + time + "seconds");
        if (Hunger.DangerThresholdReached) {
            Debug.Log("Cause of death: hunger");
        }
        if (Thirst.DangerThresholdReached) {
            Debug.Log("Cause of death: thirst");
        }
    }
}
