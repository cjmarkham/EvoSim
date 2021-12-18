using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode()]
public class ProgressBar : MonoBehaviour
{
    public float maximum = 1f;
    public float current;

    public Image mask;

    private void Start() {
    }

    private void Update() {
        GetCurrentFill();
    }

    private void GetCurrentFill() {
        float fillAmount = current / maximum;
        mask.fillAmount = fillAmount;
    }
}
