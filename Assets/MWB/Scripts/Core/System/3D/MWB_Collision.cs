using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct MWB_Collision
{
    public uint FrameIndex;
    public Vector3 Position;
    public Quaternion Rotation;
    public Vector3 Velocity;
    public Vector3 AngularVelocity;
    public Collision Collision;

    public MWB_Collision(Collision collision)
    {
        FrameIndex = 0;
        Position = Vector3.zero;
        Rotation = Quaternion.identity;
        Velocity = Vector3.zero;
        AngularVelocity = Vector3.zero;
        this.Collision = collision;
    }

    public MWB_Collision(Collision collision, Vector3 velocity, Vector3 angularVelocity)
    {
        FrameIndex = 0;
        Position = Vector3.zero;
        Rotation = Quaternion.identity;
        Velocity = velocity;
        AngularVelocity = angularVelocity;
        this.Collision = collision;
    }
}

public struct MWB_Data
{
    uint FrameIndex;
    Vector3 Position;
    Quaternion Rotation;
    Vector3 Velocity;
    Quaternion AngularVelocity;
}

