using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MWBTest;
using TransformDataUtility;

public class MWB_DummyObject : MonoBehaviour
{
    public static int pathNumber = 0;
    public static int dotNumber = 0;

    public MWB_System Manager;
    public MWB_Object objectSource;

    public MWB_DummyObject forkedSource;

    public new Rigidbody rigidbody;
    public RigidbodyState rigidbodyState;
    //
    public MWB_DummyObjectList correspondingDummyList;

    // maintain the parent path when forked
    public MWB_Path dummyMainPath = null;

    private Vector3 previousMotionDirection;
    private Vector3 previousPosition;

    public int pathIndex;

    // data for creating animation clip
    //public List<TransformData> transformData = new List<TransformData>();
    public TransformDataSegment transformDataSegment = new TransformDataSegment();
    public List<TransformData> TransformData
    {
        get
        {
            return transformDataSegment.transformData;
        }
    }

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();

        GetComponent<Renderer>().material.color = new Color(0.5f, 1, 1);
    }

    public void SetSubPath(MWB_Path parentPath)
    {
        if (parentPath == null)
        {
            Debug.LogError("dummy object : " + name + " , parentPath is null");
            return;
        }
        dummyMainPath = MWB_PathFactory.Instance.GetSubPath(parentPath);

        objectSource.selectablePaths.Add(dummyMainPath);
        pathIndex = objectSource.selectablePaths.Count - 1;

        pathNumber++;
    }

    public void RecordPosition()
    {
        // decide if we need to draw this point
        Vector3 delta = transform.position - previousPosition;
        delta.Normalize();

        bool isOnStraightLine = ((1.0f - Vector3.Dot(delta, previousMotionDirection)) <= float.Epsilon);

        if (!isOnStraightLine)
        {
            dummyMainPath.AddVertex(transform.position);

            if (dummyMainPath.Vertices.Count >= 2)
            {
                dummyMainPath.AddLineData(
                    dummyMainPath.Vertices[dummyMainPath.Vertices.Count - 2],
                    dummyMainPath.Vertices[dummyMainPath.Vertices.Count - 1],
                    pathIndex
                    );

                //Debug.Log(pathIndex + " " + objectSource.selectablePaths.FindIndex(x => x == dummyMainPath));
            }

            dotNumber++;
        }

        previousMotionDirection = delta;
        previousPosition = transform.position;

        // record transform data
        transformDataSegment.AddTransformData(transform.localPosition, transform.localRotation, transform.localScale);
    }

    // PROBLEM : trigger too many times , not as expected
    // Update : problem fixed by avoiding setting object active/disactive

    private void OnCollisionEnter(Collision collision)
    {
        // TODO : make a logical threshold formula, rather than a random constant

        // if smaller than threshold , ignore it < IMPULSE THRESHOLD should have a function to calculate >
        //if (objectSource.gameObject.name == "Sphere")
        //{
        //    if (collision.impulse.magnitude > rigidbody.mass)
        //        biggerThanMass++;
        //    else
        //        smallerThanMass++;

        //    Debug.Log("biggerThanMass : " + biggerThanMass + " , smallerThanMass : " + smallerThanMass);
        //}

        if (collision.collider.tag == "MWB_Ignore")
            return;

        if (collision.impulse.magnitude < rigidbody.mass * Manager.CollisionThresholdCoef)
            return;

        Manager.RegisterAsCollidedInThisFrame(this, collision);
    }
}