using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public static class BezierCurve
{
    //public Transform p0, p1, p2, p3;
    // Start is called before the first frame update
    /*void Start()
    {
        Vector3[] bezierPoints = new Vector3[100];
        GameObject[] spheres = new GameObject[100];
        float t = 0.0f;
        //Create points along curve
        for (int i = 0; i < 100; ++i)
        {
            bezierPoints[i] = GetBezierPoint(t);
            t = t + 0.01f;
        }
        //Create spheres to visualize points
        for (int k = 0; k < 100; ++k)
        {
            spheres[k] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            spheres[k].transform.position = bezierPoints[k];
        }
    }*/

    // Update is called once per frame
    public static Vector3 GetBezierPoint(float t, Transform p0, Transform p1, Transform p2, Transform p3)
    {
        Vector3 Bt;

        Bt = ((float)Math.Pow((1f - t), 3) * p0.position) + (3f * (float)Math.Pow((1f - t), 2) * t * p1.position) + (3f * (1f - t) * (float)Math.Pow(t, 2) * p2.position) + ((float)Math.Pow(t, 3) * p3.position);
        return Bt;
    }
}
