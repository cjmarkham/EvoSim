using UnityEngine;

public class Eatable : MonoBehaviour {
    public bool Targeted = false;

    public float Sustenance = 0.2f;

    public void Remove() {
        // When one piece of grass is eaten, spawn another somewhere random
        GameObject.Find("GameController").GetComponent<GameController>().SpawnGrass();
        Destroy(gameObject);
    }
}
