using UnityEngine;

// An attribute is something like hunger or thirst
public class Attribute : ScriptableObject {
    private float value;

    public float Value {
        get {
            return value;
        }
    }

    private readonly int DangerThreshold;

    // The amount to increment or decrement in each update call
    private readonly float ModifyAmount;

    private bool Incrementing = true;
    private bool Decrementing = false;

    // Defines whether or not the upper limit has been reached. For example
    // if ThresholdReached = true then we're hungry and should find food
    public bool ThresholdReached = false;

    public Attribute(float startValue, float modifyAmount, int dangerThreshold) {
        value = startValue;
        ModifyAmount = modifyAmount;
        DangerThreshold = dangerThreshold;
    }

    public void Increment() {
        if (Decrementing) {
            return;
        }

        if(Value >= DangerThreshold) {
            ThresholdReached = true;
            return;
        }

        Incrementing = true;
        value += Time.deltaTime * ModifyAmount;
    }

    public void Decrement() {
        if (!ThresholdReached) {
            if (Incrementing) {
                return;
            }
        }

        Decrementing = true;
        // Decrement faster than increment so we don't drink or eat for ages
        value -= Time.deltaTime * (ModifyAmount * 10);

        if (Value <= 0f) {
            ThresholdReached = false;
            Incrementing = true;
        }
    }
}
