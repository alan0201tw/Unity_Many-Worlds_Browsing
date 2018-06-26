using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MWB_Path : MWB_SelectablePath
{
    MWB_DummyObject m_PhysicsDummy;

    public MWB_DummyObject SourceDummyObject { get { return m_PhysicsDummy; } set { m_PhysicsDummy = value; } }

    public override void Select()
    {
        base.Select();
        // TO DO
    }

    public override void Deselect()
    {
        base.Deselect();
        // TO DO
    }
}