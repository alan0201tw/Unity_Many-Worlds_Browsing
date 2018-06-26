using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SineDistributionDrawer : MonoBehaviour {

    public float Alpha = 1.0f;
    public Vector3 Direction = Vector3.up;

    public float RadiusOffset = 0.01f;
    public float RadiusCount = 2;

    public float CircleCount = 4;


    public List<Vector3> Perturbations = new List<Vector3>();

    public void Start()
    {
        
    }

    public void OnGUI()
    {
        if (GUILayout.Button("Calculate perturbation"))
        {
            calculatePerturbation();
        }
    }

    private void calculatePerturbation()
    {
        Perturbations.Clear();

        Vector3 normal = Direction.normalized;
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

                var point = transform.position + Direction + dir * radius;

                Vector3 vec = point - transform.position;

                float angle = Vector3.Angle(vec, Direction);

                float sin_90_Angle = Mathf.Sin((90  -angle) * Mathf.Deg2Rad);

                Vector3 ret = vec.normalized * Mathf.Pow(sin_90_Angle, layer + 1);

                Perturbations.Add(ret);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, Direction);
        if (Perturbations != null)
        {
            Gizmos.color = Color.yellow;
            foreach (var vec in Perturbations)
            {
                Gizmos.DrawRay(transform.position, vec);
            }
        }

    }
}
