using UnityEngine;

public class UIController : MonoBehaviour {
    public GameController gameController;

    private void Start() {
    }

    private void FixedUpdate () {
        if (Input.GetMouseButtonDown(0)) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit)) {
                if (hit.transform.gameObject.tag == "Terrain") {
                    gameController.SpawnSheep(new Vector3(hit.point.x, 0, hit.point.z));
                }
            }
        }
    }
}
