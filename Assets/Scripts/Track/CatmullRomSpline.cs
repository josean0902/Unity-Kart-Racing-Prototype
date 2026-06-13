using System.Collections.Generic;
using UnityEngine;

public class CatmullRomSpline
{
    public static List<Vector3> GenerateSpline(Transform[] points, int resolution)
    {

        List<Vector3> curvePoints = new List<Vector3>();

        if (points.Length < 4)
            return curvePoints;

        if (resolution < 1)
            resolution = 1;

        for (int i = 0; i < points.Length; i++)
        {
            Vector3 p0 = points[LoopIndex(i - 1, points.Length)].position;
            Vector3 p1 = points[i].position;
            Vector3 p2 = points[LoopIndex(i + 1, points.Length)].position;
            Vector3 p3 = points[LoopIndex(i + 2, points.Length)].position;

            for (int j = 0; j < resolution; j++)
            {
                float t = j / (float)resolution;
                Vector3 pos = GetCatmullRom(p0, p1, p2, p3, t);
                curvePoints.Add(pos);
            }
        }

        return curvePoints;
    }

    private static int LoopIndex(int i, int size)
    {
        return (i + size) % size;
    }

    private static Vector3 GetCatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        return 0.5f * (
            2f * p1 +
            (-p0 + p2) * t +
            (2f * p0 - 5f * p1 + 4f * p2 - p3) * t * t +
            (-p0 + 3f * p1 - 3f * p2 + p3) * t * t * t
        );
    }
}
