using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class MWB_SelectablePath
{
    public static Color SelectColor = new Color(1f, 0.92f, 0.016f, 1.0f);
    public static Color DeselectColor = new Color(0, 0, 1, 0.5f);

    protected Shader m_Shader;
    protected Material m_Material;

    protected List<Vector3> m_Vertices = new List<Vector3>();
    public virtual List<Vector3> Vertices { get { return m_Vertices; } set { m_Vertices = value; } }

    protected List<LineBufferDrawer.LineData> m_LineDatas = new List<LineBufferDrawer.LineData>();
    public virtual List<LineBufferDrawer.LineData> LineDatas { get { return m_LineDatas; } }

    public virtual List<Vector3> RenderVertices { get { return m_Vertices; } }

    public bool UsingWorldSpace = true;

    protected bool m_IsSelected = false;
    public bool IsSelected { get { return m_IsSelected; } }

    protected bool m_IsInitialized = true;
    public bool IsInitialized { get { return m_IsInitialized; } }

    public bool IsRendering = false;

    private ComputeBuffer m_BufferData = null;
    //private int m_PreviousBufferCount = -1;

    public int IndexInQuery = 0;

    public Action<int> OnSelect = delegate { };
    public Action<int> OnDeselect = delegate { };

    public MWB_SelectablePath()
    {

    }

    public void Init()
    {
        ClearVertices();
        m_IsSelected = false;

        m_IsInitialized = true;
        IsRendering = false;
    }

    //
    public void Use()
    {
        m_IsInitialized = false;
        IsRendering = true;
    }

    // When used up, call Discard()
    public void Discard()
    {
        Init();
    }

    public void AddVertex(Vector3 vec)
    {
        m_Vertices.Add(vec);
    }

    public void AddVertex3(float x, float y, float z)
    {
        m_Vertices.Add(new Vector3(x, y, z));
    }

    public void AddVertices(List<Vector3> vert)
    {
        m_Vertices.AddRange(vert);
    }

    public void AddLineData(Vector3 begin, Vector3 end, int worldIndex)
    {
        m_LineDatas.Add(new LineBufferDrawer.LineData(begin, end, worldIndex));
    }

    public void ClearVertices()
    {
        m_Vertices.Clear();
    }

    public void AddSpline(Vector3 p0, Vector3 p1, Vector3 p2, int pointSteps = 10)
    {
        for (int i = 1; i <= pointSteps; i++)
        {
            Vector3 point = getSplinePoint(p0, p1, p2, i / (float)pointSteps);
            m_Vertices.Add(point);
        }
    }

    private Vector3 getSplinePoint(Vector3 p0, Vector3 p1, Vector3 p2, float t)
    {
        return Vector3.Lerp(Vector3.Lerp(p0, p1, t), Vector3.Lerp(p1, p2, t), t);
    }

    public virtual void Select()
    {
        m_IsSelected = true;
        OnSelect.Invoke(IndexInQuery);
    }

    public virtual void Deselect()
    {
        m_IsSelected = false;
        OnDeselect.Invoke(IndexInQuery);
    }

    // deprecated
    public virtual void SetLineColor(Color color)
    {
        m_Material.SetColor("_Color", color);
    }

    // deprecated
    void OnDestroy()
    {
        if (m_BufferData != null)
            m_BufferData.Release();
    }
}
