using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GPUSelectionQuery
{
    private ComputeShader m_QueryShader;

    private int m_QuadSelectionKernal;
    private int m_PointSelectionKernal;

    public void Init()
    {
        m_QueryShader = Resources.Load("GPUQueryShader") as ComputeShader;

        m_QuadSelectionKernal = m_QueryShader.FindKernel("QuadSelection");
        m_PointSelectionKernal = m_QueryShader.FindKernel("PointSelection");
    }

    //int _pixelWidth;
    //int _pixelHeight;
    //float4x4 _world2Screen;
    public void SetupConst(Matrix4x4 w2s, int pixelWidth, int pixelHeight)
    {
        m_QueryShader.SetInt("_pixelWidth", pixelWidth);
        m_QueryShader.SetInt("_pixelHeight", pixelHeight);
        m_QueryShader.SetMatrix("_world2Screen", w2s);
    }

    public float[] QuadSelection(List<LineBufferDrawer.LineData> lines, Rect screenRect)
    {
        Vector4 rectFloat4 = new Vector4(screenRect.xMin, screenRect.xMax, screenRect.yMin, screenRect.yMax);

        m_QueryShader.SetVector("_rect", rectFloat4);

        return query(m_QuadSelectionKernal, lines);
    }
    
    public float[] PointSelection(List<LineBufferDrawer.LineData> lines, Vector3 moustPoint)
    {
        m_QueryShader.SetVector("_mousePoint", moustPoint);

        return query(m_PointSelectionKernal, lines);
    }

    private float[] query(int kernel, List<LineBufferDrawer.LineData> lines)
    {
        float[] result = new float[lines.Count];
        
        // setup input
        ComputeBuffer input = new ComputeBuffer(lines.Count, LineBufferDrawer.SizeOfLineData);
        input.SetData(lines.ToArray());
        m_QueryShader.SetBuffer(kernel, "Input", input);

        // setup output
        ComputeBuffer output = new ComputeBuffer(lines.Count, sizeof(float));
        output.SetData(result);
        m_QueryShader.SetBuffer(kernel, "Output", output);

        m_QueryShader.Dispatch(kernel, lines.Count, 1, 1);

        // get result
        output.GetData(result);

        input.Release();
        output.Release();

        return result;
    }
}
