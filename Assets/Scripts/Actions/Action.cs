using UnityEngine;

public abstract class Action : ScriptableObject {
    public virtual int Priority {
        get {
            return 0;
        }
    }

    public virtual Actions Type {
        get {
            return Actions.Wandering;
        }
    }

    public abstract void OnStart(Sheep sheep);
    public abstract void OnUpdate();
    public abstract void OnEnd();
}
