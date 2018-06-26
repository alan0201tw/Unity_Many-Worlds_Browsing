using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GPUDebugger : MonoBehaviour {

    public ComputeShader Shader;

    public List<int> Input;

    public int[] Output;

    private void Awake()
    {
        Output = (int[]) Input.ToArray().Clone();
    }

    private void Start()
    {
        calculate();
    }

    void calculate()
    {
        int kernel = Shader.FindKernel("QuadSelection");

        ComputeBuffer inputBuffer = new ComputeBuffer(Input.Count, sizeof(int));
        inputBuffer.SetData(Input.ToArray());
        ComputeBuffer outputBuffer = new ComputeBuffer(Input.Count, sizeof(int));

        Shader.SetBuffer(kernel, "Input", inputBuffer);
        Shader.SetBuffer(kernel, "Output", outputBuffer);

        Shader.Dispatch(kernel, Input.Count, 1, 1);

        outputBuffer.GetData(Output);

        foreach(var result in Output)
        {
            Debug.Log(result.ToString()); 
        }

        //Debug.Log(Shader.) 

        inputBuffer.Release();
        outputBuffer.Release();
    }
}
