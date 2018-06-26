using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RigidbodyUtility 
{
    public static void CopyRigidbodyData(ref Rigidbody dataDestination, Rigidbody dataSource)
    {
        dataDestination.velocity = dataSource.velocity;
        dataDestination.angularVelocity = dataSource.angularVelocity;
        dataDestination.drag = dataSource.drag;
        dataDestination.angularDrag = dataSource.angularDrag;
        dataDestination.mass = dataSource.mass;
        dataDestination.useGravity = dataSource.useGravity;
        dataDestination.isKinematic = dataSource.isKinematic;
        dataDestination.freezeRotation = dataSource.freezeRotation;
        dataDestination.centerOfMass = dataSource.centerOfMass;
        dataDestination.inertiaTensorRotation = dataSource.inertiaTensorRotation;
        dataDestination.detectCollisions = dataSource.detectCollisions;
        dataDestination.position = dataSource.position;
        dataDestination.rotation = dataSource.rotation;
        dataDestination.interpolation = dataSource.interpolation;
        dataDestination.solverIterations = dataSource.solverVelocityIterations;
        dataDestination.sleepThreshold = dataSource.sleepThreshold;
        dataDestination.maxAngularVelocity = dataSource.maxAngularVelocity;
    }
}

public struct RigidbodyState
{
    public Vector3 velocity;
    public Vector3 angularVelocity;
    // needed extension for MWB_System
    public Vector3 addingImpulse;

    public RigidbodyState(Rigidbody rigidbody, Vector3 addingImpulse)
    {
        velocity = rigidbody.velocity;
        angularVelocity = rigidbody.angularVelocity;
        this.addingImpulse = addingImpulse;
    }

    public void SetRigidbodyState(ref Rigidbody rigidbody)
    {
        rigidbody.velocity = velocity;
        rigidbody.angularVelocity = angularVelocity;
        rigidbody.AddForce(addingImpulse);
    }

    public void GetRigidbodyState(Rigidbody rigidbody, Vector3 addingImpulse)
    {
        velocity = rigidbody.velocity;
        angularVelocity = rigidbody.angularVelocity;
        this.addingImpulse = addingImpulse;
    }
}