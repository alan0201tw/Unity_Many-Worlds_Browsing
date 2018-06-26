using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MWB_ObjectList
{
    private List<MWB_Object> m_MWB_Objects = new List<MWB_Object>();
    public List<MWB_Object> MWB_Objects { get { return m_MWB_Objects; } }

    public void AddToObjectList(MWB_Object mwbObject)
    {
        MWB_Objects.Add(mwbObject);
    }
}