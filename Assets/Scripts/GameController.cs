using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public GameObject sheep;
    List<GameObject> totalSheep;
    
    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Space))
        {
            GameObject newSheep = Instantiate(sheep, new Vector3(0, 0, 0), Quaternion.Euler(0, 0, 0));
            totalSheep.Add(newSheep);
        }
    }
}
