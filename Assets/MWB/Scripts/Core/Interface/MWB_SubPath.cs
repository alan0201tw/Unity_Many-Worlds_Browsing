using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MWB_SubPath : MWB_Path
{
    private MWB_Path m_RootPath;

    public override List<Vector3> Vertices 
    { 
        get { return m_RootPath.Vertices.Concat(m_Vertices).ToList(); } 
    }

    

    public override List<LineBufferDrawer.LineData> LineDatas { get { return m_RootPath.LineDatas.Concat(m_LineDatas).ToList(); } }

    public override List<Vector3> RenderVertices 
    { 
        get 
        {
            List<Vector3> head = new List<Vector3>();
            if (m_RootPath.Vertices.Count > 0)
            {
                head.Add(m_RootPath.Vertices[m_RootPath.Vertices.Count - 1]);
            }
            return head.Concat(m_Vertices).ToList();
        } 
    }

    public void SetParentPath(MWB_Path parentPath)
    {
        m_RootPath = parentPath;
    }


    public override void Select()
    {
        base.Select();
    }

    public override void Deselect()
    {
        base.Deselect();
    }

    /*
    void OnDrawGizmos()
    {
        for ( int i = 0; i < Vertices.Count - 1; i++ )
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(Vertices[i], Vertices[i + 1]);
        }
    }*/
}