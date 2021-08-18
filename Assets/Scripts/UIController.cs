using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UIController : MonoBehaviour {
    private Sheep selectedSheep;
    private RectTransform QueueListRect;

    private void Start() {
        QueueListRect = GameObject.Find("Queue List").GetComponent<RectTransform>();
    }

    private void Update () {
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
        foreach (Text existing in QueueListRect.GetComponentsInChildren<Text>()) {
            Destroy(existing.gameObject);
        }
    }

    void UpdateSheepUI() {
        if (selectedSheep == null) {
            return;
        }

        EmptyQueueUI();

        List<Action> queueActions = selectedSheep.ActionQueue.All();

        foreach (Action action in queueActions) {
            GameObject textGameObject = new GameObject();
            Text t = textGameObject.AddComponent<Text>();
            t.text = action.Type.ToString();
            t.alignment = TextAnchor.MiddleRight;
            t.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            t.fontSize = 14;
            t.transform.SetParent(QueueListRect);
        }
    }
}
