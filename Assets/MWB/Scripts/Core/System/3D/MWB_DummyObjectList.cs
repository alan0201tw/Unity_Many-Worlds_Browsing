using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MWB_DummyObjectList
{
    private List<MWB_DummyObject> m_DummyObjects = new List<MWB_DummyObject>();
    public List<MWB_DummyObject> MWB_DummyObjects { get { return m_DummyObjects; } private set { m_DummyObjects = value; } }

    public void AddToDummyObjectList(MWB_DummyObject mwbDummyObject)
    {
        MWB_DummyObjects.Add(mwbDummyObject);
    }

    public void AddToDummyObjectList(List<MWB_DummyObject> mwbDummyObjects)
    {
        MWB_DummyObjects.AddRange(mwbDummyObjects);
    }

    public void Clear()
    {
        MWB_DummyObjects.Clear();
    }
}