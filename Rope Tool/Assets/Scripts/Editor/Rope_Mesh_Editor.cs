using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
public class Rope_Mesh_Editor : EditorWindow
{
    int ropeID = 1;
    string ropeObjectName = "Rope";

    //GameObject ropeObject; //reference to the object
    public List<GameObject> points;
    int numPoints = 4;
    int baseQuality = 7; //Number of sides. Minimum of 3
    const int MIN_BASE_QUALITY = 3;
    const int MAX_BASE_QUALITY = 1000;
    //Vector3 p0, p1, p2, p3; //p0 is start, p3 is end 
    float radius = 1.0f;
    const float MIN_RADIUS = 0.001f;
    const float MAX_RADIUS = 1000f;
    int lengthQuality = 5;
    const int MIN_LEN_QUALITY = 1;
    const int MAX_LEN_QUALITY = 10000;
    Material mat;

    //GUI enables
    bool SpawnPointsBool = true;
    bool DeletePointsBool = false;
    bool CreateMeshBool = false;
    //Mesh material
    [MenuItem("Tools/Rope Generator")]
    public static void ShowWindow()
    {
        GetWindow(typeof(Rope_Mesh_Editor));
    }

    //Called every time an event on the GUI occurs.
    private void OnGUI()
    {
        GUILayout.Label("Create Rope Object", EditorStyles.boldLabel);
        //Self explanatory: Creates GUI elements with respective names
        ropeID = EditorGUILayout.IntField("Rope ID", ropeID);
        ropeObjectName = EditorGUILayout.TextField("Rope Object Name", ropeObjectName);
        baseQuality = EditorGUILayout.IntSlider("Number of Sides", baseQuality, MIN_BASE_QUALITY, MAX_BASE_QUALITY);
        radius = EditorGUILayout.Slider("Radius", radius, MIN_RADIUS, MAX_RADIUS);
        lengthQuality = EditorGUILayout.IntSlider("Curve Quality", lengthQuality, MIN_LEN_QUALITY, MAX_LEN_QUALITY);
        GUILayout.Space(5f);
        mat = (Material)EditorGUILayout.ObjectField(mat, typeof(Material), true);
        GUILayout.Space(25f);

        //If false, it grays out the button.
        GUI.enabled = SpawnPointsBool;
        if (GUILayout.Button("Spawn Curve Points"))
        {
            SpawnPoints();
            SpawnPointsBool = false;
            DeletePointsBool = true;
            CreateMeshBool = true;
        }
        GUI.enabled = DeletePointsBool;
        if (GUILayout.Button("Delete Points"))
        {
            DeletePoints();
            SpawnPointsBool = true;
            DeletePointsBool = false;
            CreateMeshBool = false;

        }
        GUI.enabled = CreateMeshBool;
        GUILayout.Space(15f);
        if (GUILayout.Button("Create Mesh"))
        {
            CreateMesh();
            SpawnPointsBool = true;
            DeletePointsBool = false;
            CreateMeshBool = false;
        }

    }

    //Creates bezier points on which the curve is to be created with.
    private void SpawnPoints()
    {
        if (points != null)
            return;
        points = new List<GameObject>();

        GameObject pointPrefab = (GameObject)Resources.Load("Point", typeof(GameObject));
        if (pointPrefab == null)
            throw new System.Exception("Point prefab not found. Make sure there is a Resources/Point.prefab with an ObjectLabel script attached to the object");
        for (int i = 0; i < numPoints; ++i)
        {
            Transform sceneCameraTransform = SceneView.lastActiveSceneView.camera.transform;
            GameObject tempPoint = Instantiate(pointPrefab, sceneCameraTransform.position + sceneCameraTransform.forward * 2.5f + sceneCameraTransform.right * (float)i, Quaternion.identity);
            ObjectLabel numberHandle = tempPoint.GetComponent(typeof(ObjectLabel)) as ObjectLabel;
            numberHandle.PointNumber = i;

            //Move spheres in front of scene view camera and spawn to the right.
            points.Add(tempPoint);
        }
        return;
    }
    private void DeletePoints()
    {
        for (int i = points.Count - 1; i >= 0; --i)
        {
            Object.DestroyImmediate(points[i]);
            points.RemoveAt(i);
        }
        points = null;
        return;
    }
    private void CreateMesh()
    {
        //Transform gameobject list to transform list
        List<Transform> transPoints = new List<Transform>();
        foreach (GameObject obj in points)
        {
            transPoints.Add(obj.transform);
        }
        GameObject ropeObject = new GameObject(ropeObjectName + ropeID.ToString(), typeof(MeshFilter), typeof(MeshRenderer));
        ++ropeID;
        RopeGenerator rg = new RopeGenerator(baseQuality, radius, lengthQuality, transPoints, mat, ropeObject);
        rg.CreateRope();
        DeletePoints();
    }
}
