using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UIController : MonoBehaviour {
    private Sheep selectedSheep;
    private RectTransform CurrentActionRect;
    private RectTransform NextActionsRect;

    private void Start() {
        CurrentActionRect = GameObject.Find("Current Action").GetComponent<RectTransform>();
        NextActionsRect = GameObject.Find("Next Actions").GetComponent<RectTransform>();
    }

    private void FixedUpdate () {
        if (Input.GetMouseButtonDown(0)) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit)) {
                if (hit.transform.gameObject.tag == "Sheep") {
                    cakeslice.Outline outlineScript = hit.transform.gameObject.GetComponent<cakeslice.Outline>();
                    selectedSheep = hit.transform.gameObject.GetComponent<Sheep>();
                    outlineScript.eraseRenderer = false;
                } else {
                    EmptyQueueUI();
                    selectedSheep = null;

                    // Remove the outline from every sheep (expensive?)
                    GameObject[] sheeps = GameObject.FindGameObjectsWithTag("Sheep");
                    foreach (GameObject sheep in sheeps) {
                        cakeslice.Outline outlineScript = sheep.GetComponent<cakeslice.Outline>();
                        outlineScript.eraseRenderer = true;
                    }
                }
            }
        }

        UpdateSheepUI();
    }

    void EmptyQueueUI() {
        foreach (Text existing in CurrentActionRect.GetComponentsInChildren<Text>()) {
            Destroy(existing.gameObject);
        }

        foreach (Text existing in NextActionsRect.GetComponentsInChildren<Text>()) {
            Destroy(existing.gameObject);
        }
    }

    void UpdateSheepUI() {
        if (selectedSheep == null) {
            return;
        }

        EmptyQueueUI();

        // Add the first action to the current action queue
        Action first = selectedSheep.ActionQueue.CurrentAction;
        Text text = CreateTextForAction(first);
        text.transform.SetParent(CurrentActionRect);

        // Add the rest to the next action queue
        List<Action> queueActions = selectedSheep.ActionQueue.All();

        for (int i = 1; i < queueActions.Count; i++) {
            Action action = queueActions[i];
            Text t = CreateTextForAction(action);
            t.transform.SetParent(NextActionsRect);
        }
    }

    Text CreateTextForAction(Action action) {
        GameObject textGameObject = new GameObject();
        Text t = textGameObject.AddComponent<Text>();
        t.text = action.Type.ToString();
        t.alignment = TextAnchor.MiddleRight;
        t.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        t.fontSize = 14;

        return t;
    }
}
