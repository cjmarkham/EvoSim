using UnityEngine;

public class Poop : MonoBehaviour {
    private float spawnTime;

    public GameObject Grass;

    private void Start() {
        spawnTime = Time.timeSinceLevelLoad;
    }

    private void FixedUpdate() {
        float timeSinceInitialization = Time.timeSinceLevelLoad - spawnTime;

        if (timeSinceInitialization > 15f) {
            Vector3 grassPosition = new Vector3(transform.position.x, 0.87f, transform.position.z);
            Instantiate(Grass, grassPosition, Quaternion.Euler(0, 0, 0));
            Destroy(gameObject);
        }
    }
}
