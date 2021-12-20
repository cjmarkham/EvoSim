using UnityEngine;

public class UIController : MonoBehaviour {
    private Sheep selectedSheep;

    public GameController gameController;

    private void Start() {
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

                    selectedSheep.transform.Find("Stats").gameObject.SetActive(true);
                } else {

                    if (hit.transform.gameObject.tag == "Terrain") {
                        gameController.SpawnSheep(new Vector3(hit.point.x, 0, hit.point.z));
                    }

                    selectedSheep = null;

                    // Remove the outline from every sheep (expensive?)
                    GameObject[] sheeps = GameObject.FindGameObjectsWithTag("Sheep");
                    foreach (GameObject sheep in sheeps) {
                        cakeslice.Outline outlineScript = sheep.GetComponent<cakeslice.Outline>();
                        outlineScript.eraseRenderer = true;

                        sheep.transform.Find("Stats").gameObject.SetActive(false);
                    }
                }
            }
        }
    }
}
