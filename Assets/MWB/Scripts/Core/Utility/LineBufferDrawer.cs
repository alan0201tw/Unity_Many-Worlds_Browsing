using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineBufferDrawer {

    public const int SizeOfLineData = 28;

    // size : 12 * 2 + 4 = 28
    public struct LineData
    {
        public Vector3 Begin;
        public Vector3 End;
        public int LineStripIndex;

        public LineData(Vector3 begin, Vector3 end, int stripIndex)
        {
            Begin = begin;
            End = end;
            LineStripIndex = stripIndex;
        }
    }

    public const int SizeOfColorData = 16;

    // size : 16 * 1 = 16
    public struct ColorData
    {
        public Color Color;

        public ColorData(Color col)
        {
            Color = col;
        }
    }

    private ComputeBuffer m_LineBuffer;
    private ComputeBuffer m_ColorBuffer;

    private Material m_LineMaterial;
    private int m_LineCount;

    public void CreateLineBuffer(List<LineData> lineData, List<ColorData> colorPalatte)
    {
        m_LineCount = lineData.Count;

        m_LineBuffer = new ComputeBuffer(lineData.Count, 28);
        m_ColorBuffer = new ComputeBuffer(colorPalatte.Count, 16);

        m_LineBuffer.SetData(lineData.ToArray());
        m_ColorBuffer.SetData(colorPalatte.ToArray());

        m_LineMaterial.SetBuffer("buf_line", m_LineBuffer);
        m_LineMaterial.SetBuffer("buf_color", m_ColorBuffer);
    }

    public void SetColors(List<ColorData> colorData)
    {
        if (m_ColorBuffer != null)
            m_ColorBuffer.SetData(colorData.ToArray());

        if (m_LineMaterial)
            m_LineMaterial.SetBuffer("buf_color", m_ColorBuffer);
    }

    public void ReleaseLineBuffer()
    {
        if (m_LineBuffer != null)
            m_LineBuffer.Release();

        if (m_ColorBuffer != null)
            m_ColorBuffer.Release();

        //MonoBehaviour.DestroyImmediate(m_LineMaterial);
    }

    public void Render()
    {
        if (m_LineMaterial == null)
            Init();

        m_LineMaterial.SetPass(0);
        Graphics.DrawProcedural(MeshTopology.Points, m_LineCount);

        //CjLib.DebugUtil.DrawBox(Vector3.zero, Quaternion.identity, Vector3.one * 20.0f, Color.blue);
    }

    public void Init()
    {
        createMaterial();
    }

    private void createMaterial()
    {
        m_LineMaterial = new Material(Shader.Find("MWB/GeomtryLineShader"));
    }
}
