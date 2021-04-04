using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public GameObject sheep;
    int totalSheep;

    Text sheepCountText;

    private void Start()
    {
        sheepCountText = GameObject.Find("Sheep Count").GetComponent<Text>();
    }

    private void Update()
    {
        totalSheep = GameObject.FindGameObjectsWithTag("Sheep").Length;
        sheepCountText.text = totalSheep.ToString();
    }

    public void SpawnSheep()
    {
        Debug.Log("SPAWN");
        Instantiate(sheep, new Vector3(0, 0, 0), Quaternion.Euler(0, 0, 0));
    }
}
