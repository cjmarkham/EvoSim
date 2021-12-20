using UnityEngine;

// An attribute is something like hunger or thirst
public class Attribute : ScriptableObject {
    private float value;

    public float Value {
        get {
            return value;
        }
    }

    // The max limit before the animal starts losing HP (not implemented)
    private readonly float DangerThreshold;

    // Whether or not the danger threshold has been reached
    public bool DangerThresholdReached = false;

    // The amount to increment or decrement in each update call
    private readonly float ModifyAmount;

    private bool Decrementing = false;

    public Attribute(float startValue, float modifyAmount, float dangerThreshold) {
        value = startValue;
        ModifyAmount = modifyAmount;
        DangerThreshold = dangerThreshold;
    }

    public void Increment() {
        if(Value >= DangerThreshold) {
            DangerThresholdReached = true;
            return;
        }

        value += Time.deltaTime * ModifyAmount;
    }

    public void Decrement() {
        Decrementing = true;

        // Decrement faster than increment so we don't drink or eat for ages
        value -= Time.deltaTime * (ModifyAmount * 10);

        if (Value <= 0f) {
            DangerThresholdReached = false;
            Decrementing = false;
        }
    }

    public bool ShouldSatisfyAttribute() {
        // If we're decrementing then we're already satisfying our attribute
        if (Decrementing) {
            return false;
        }

        if (value > (DangerThreshold / 2)) {
            return true;
        }

        return false;
    }

    public bool AttributeSatisfied() {
        return value <= 0f;
    }
}
