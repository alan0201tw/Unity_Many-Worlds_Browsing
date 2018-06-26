using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SinDistributionUtility
{
    public static float Alpha = 100f;
    public static float RadiusOffset = 1f;
    public static float RadiusCount = 3;

    public static float CircleCount = 3;

    public static List<Vector3> CalculatePerturbation(Vector3 origin , Vector3 direction)
    {
        List<Vector3> Perturbations = new List<Vector3>();
        Perturbations.Clear();

        Vector3 normal = direction.normalized;
        Vector3 tangent = Vector3.Cross(normal, Vector3.forward);
        if (tangent.magnitude == 0)
        {
            tangent = Vector3.Cross(normal, Vector3.up);
        }
        tangent.Normalize();
        Vector3 arcTangent = Vector3.Cross(normal, tangent);

        for (int layer = 0; layer < RadiusCount; layer++)
        {
            float radius = (layer + 1) * RadiusOffset;
            float degreeOffset = 360.0f / CircleCount;

            for (int i = 0; i < CircleCount; i++)
            {
                float degree = degreeOffset * i;

                float cos = Mathf.Cos(degree * Mathf.Deg2Rad);
                float sin = Mathf.Sin(degree * Mathf.Deg2Rad);

                Vector3 dir = cos * tangent + sin * arcTangent;

                var point = origin + direction + dir * radius;

                Vector3 vec = point - origin;

                float angle = Vector3.Angle(vec, direction);

                float sin_90_Angle = Mathf.Sin((90 - angle) * Mathf.Deg2Rad);

                Vector3 ret = vec.normalized * Mathf.Pow(sin_90_Angle, layer + 1);
                //
                ret *= Alpha;
                //
                Perturbations.Add(ret);
            }
        }

        return Perturbations;
    }
}