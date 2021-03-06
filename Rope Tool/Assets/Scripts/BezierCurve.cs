using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public static class BezierCurve
{

    public static Vector3 GetBezierPointCubic(float t, List<Transform> points)
    {
        List<Transform> tempPoints = points;
        Vector3 Bt;

        Bt = ((float)Math.Pow((1f - t), 3) * points[0].position) + (3f * (float)Math.Pow((1f - t), 2) * t * points[1].position) + (3f * (1f - t) * (float)Math.Pow(t, 2) * points[2].position) + ((float)Math.Pow(t, 3) * points[3].position);
        return Bt;

    }
    public static Vector3 GetBezierPoint(float t, Transform p0, Transform p1, Transform p2, Transform p3)
    {
        Vector3 Bt;

        Bt = ((float)Math.Pow((1f - t), 3) * p0.position) + (3f * (float)Math.Pow((1f - t), 2) * t * p1.position) + (3f * (1f - t) * (float)Math.Pow(t, 2) * p2.position) + ((float)Math.Pow(t, 3) * p3.position);
        return Bt;
    }
}
