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
        float maxX = 6.5f;
        float minX = -6.5f;
        float maxZ = 6.5f;
        float minZ = -6.5f;

        float randomX = Random.Range(minX, maxX);
        float randomZ = Random.Range(minZ, maxZ);
        Instantiate(Sheep, new Vector3(randomX, 0, randomZ), Quaternion.Euler(0, 0, 0));
    }

    public void SpawnGrass() {
        int maxX = 7;
        int minX = -7;
        int maxZ = 7;
        int minZ = -7;

        int randomX = Random.Range(minX, maxX);
        int randomZ = Random.Range(minZ, maxZ);

        Instantiate(Grass, new Vector3(randomX, 0, randomZ), Quaternion.Euler(-90, 0, 0));
    }
}
