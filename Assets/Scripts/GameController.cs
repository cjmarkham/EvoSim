using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour {
    public GameObject Sheep;
    public GameObject Grass;
    int totalSheep;

    Text sheepCountText;
    Text TimerText;

    private void Start() {
        sheepCountText = GameObject.Find("Sheep Count").GetComponent<Text>();
        TimerText = GameObject.Find("Timer").GetComponent<Text>();
    }

    private void Update() {
        totalSheep = GameObject.FindGameObjectsWithTag("Sheep").Length;
        sheepCountText.text = totalSheep.ToString();

        TimerText.text = Time.realtimeSinceStartup.ToString();
    }

    public void SpawnSheep() {
        float maxX = 30f;
        float minX = 0f;
        float maxZ = 11f;
        float minZ = 0f;

        float randomX = Random.Range(minX, maxX);
        float randomZ = Random.Range(minZ, maxZ);
        Instantiate(Sheep, new Vector3(randomX, 0, randomZ), Quaternion.Euler(0, 0, 0));
    }

    public void SpawnGrass() {
        float maxX = 30f;
        float minX = 0f;
        float maxZ = 11f;
        float minZ = 0f;

        float randomX = Random.Range(minX, maxX);
        float randomZ = Random.Range(minZ, maxZ);

        Instantiate(Grass, new Vector3(randomX, 0.87f, randomZ), Quaternion.Euler(0, 0, 0));
    }

    public void SetSpeed(int scale) {
        Time.timeScale = scale;
    }
}
