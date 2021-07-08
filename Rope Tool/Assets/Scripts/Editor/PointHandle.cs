using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ObjectLabel))]
public class PointHandle : Editor
{
    void OnSceneGUI()
    {
        ObjectLabel pointLabel = (ObjectLabel)target;
        if (pointLabel == null)
            return;

        GUIStyle style = new GUIStyle();
        style.richText = true;
        style.fontSize = 35;
        Handles.Label(pointLabel.transform.position + Vector3.up * 0.35f + Vector3.left * 0.10f, "<Color=red>P" + pointLabel.PointNumber.ToString() + "</Color>", style);

    }
}
