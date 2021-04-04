using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Sheep))]
public class SensoryRadiusEditor : Editor
{
    void OnSceneGUI()
    {
        Sheep sheep = (Sheep)target;
        Handles.DrawWireArc(sheep.transform.position, Vector3.up, Vector3.forward, 360, sheep.viewRadius);
    }
}
