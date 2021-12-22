using UnityEngine;

public class Eatable : MonoBehaviour {
    public bool Targeted = false;

    public float Sustenance = 0.2f;

    public void Remove() {
        Destroy(gameObject);
    }
}
