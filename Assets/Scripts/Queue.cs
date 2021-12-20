using System.Collections.Generic;
using UnityEngine;

public class Queue : MonoBehaviour {
    public List<Action> ActionQueue;

    public Action CurrentAction;

    private Sheep Sheep;

    public void Start() {
        Sheep = GetComponent<Sheep>();
        ActionQueue = new List<Action>();
    }

    public bool Empty() {
        return ActionQueue.Count == 0;
    }

    public bool HasItem(Actions actionType) {
        return ActionQueue.Exists(a => a.Type == actionType);
    }

    public Action First() {
        return ActionQueue[0];
    }

    public Action Pop() {
        Action action = ActionQueue[0];
        ActionQueue.RemoveAt(0);
        return action;
    }

    public void Add(Action action) {
        // Add the item to the queue
        ActionQueue.Add(action);

        // Now sort the queue so the higher priority items are first
        Sort();
    }

    private void Sort() {
        // We don't care about sorting if there's nothing to sort
        if (ActionQueue.Count < 2) {
            return;
        }

        ActionQueue.Sort(delegate (Action a, Action b) {
            if (a.Priority > b.Priority) {
                return -1;
            }
            return 1;
        });
    }

    public void StartNextAction() {
        CurrentAction = Pop();
        CurrentAction.OnStart(Sheep);
        Debug.Log("Starting Action " + CurrentAction.Type.ToString());
    }

    public void ResetCurrent() {
        CurrentAction = null;
    }

    public List<Action> All() {
        return ActionQueue;
    }

    public int Length() {
        return ActionQueue.Count;
    }
}
