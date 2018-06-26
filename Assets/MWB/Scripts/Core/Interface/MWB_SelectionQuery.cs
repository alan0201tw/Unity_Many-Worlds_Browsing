using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class MWB_SelectionQuery
{
    public enum SelectionType
    {
        None,
        Point,
        Quad
    }

    private static MWB_SelectionQuery instance;
    public static MWB_SelectionQuery Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new MWB_SelectionQuery(); //FindObjectOfType<MWB_SelectionQuery>();
            }

            if (instance == null)
            {
                Debug.LogError("SelectionQuery is needed.");
            }

            return instance;
        }
    }

    [Header("Reference")]
    [SerializeField]
    private Camera m_Camera;
    
    public List<MWB_SelectablePath> m_SeletablePathList = new List<MWB_SelectablePath>();

    private List<int> m_SelectedPathsIndex = new List<int>();
    public List<int> SelectedPathsIndex { get { return m_SelectedPathsIndex; } }

    private List<MWB_SelectablePath> m_SelectedPaths = new List<MWB_SelectablePath>();
    public List<MWB_SelectablePath> SelectedPaths { get { return m_SelectedPaths; } }

    public int SelectedPathCount { get { return SelectedPaths.Count; } }
    public MWB_Path SelectedPath { get { return (SelectedPathCount > 0)? m_SelectedPaths[0] as MWB_Path : null; } }
    
    [Header("Settings")]
    public int CoroutineMaxLoopCount = 1024;
    public float PointSelectionSqrMagnitudeThreshold = 20.0f;

    public float PointSelectionSqrMagnitudeEndThreshold = 5.0f;

    //private SelectionType m_SelectionType = SelectionType.None;

    private LineBufferDrawer m_LineDrawer = new LineBufferDrawer();

    private GPUSelectionQuery m_Query = new GPUSelectionQuery();

    private List<LineBufferDrawer.LineData> m_LineDatas = new List<LineBufferDrawer.LineData>();

    bool m_IsColorInit = false;
    private List<LineBufferDrawer.ColorData> m_ColorDatas = new List<LineBufferDrawer.ColorData>();

    MWB_SelectionQuery()
    {
        UnityEditor.EditorApplication.update += onEditorApplicationUpdate;

        m_Query.Init();
    }

    public void SetCamera(Camera camera)
    {
        m_Camera = camera;
    }

    public void ChangeSelectablePathList(List<MWB_SelectablePath> paths)
    {
        //DeselectAll();
        cleanSelectedPathList();

        CleanRegisteredPaths();
        AddRangePaths(paths);

        foreach (var index in m_SelectedPathsIndex)
        {
            var path = m_SeletablePathList[index];
            reSelectPrevSelectedPath(path);
        }

        // update data required by line drawer

        if (!m_IsColorInit)
            initColorData();

        initLineData();

        // sent data to line drawer
        m_LineDrawer.Init();
        m_LineDrawer.ReleaseLineBuffer();
        m_LineDrawer.CreateLineBuffer(m_LineDatas, m_ColorDatas);

        //Debug.Log("PATH SIZE: " + paths.Count);
    }

    private void initColorData()
    {
        m_ColorDatas.Clear();

        int lineIndex = 0;
        foreach (var path in m_SeletablePathList)
        {
            if (path.IsRendering == false)
                continue;

            // set color palatte data for linestrip
            //Color col = (path.IsSelected) ? MWB_SelectablePath.SelectColor : MWB_SelectablePath.DeselectColor;
            m_ColorDatas.Add(new LineBufferDrawer.ColorData(MWB_SelectablePath.DeselectColor));

            lineIndex++;
        }

        m_IsColorInit = true;
    }

    private void updateColorData()
    {
        m_LineDrawer.SetColors(m_ColorDatas);
    }

    private void initLineData()
    {
        m_LineDatas.Clear();

        foreach(var path in m_SeletablePathList)
        {
            if (path.IsRendering == false)
                continue;
            
            m_LineDatas.AddRange(path.LineDatas);
        }
    }


    public void RenderLines()
    {
        m_LineDrawer.Render();
    }

    public void AddRangePaths(List<MWB_SelectablePath> paths)
    {
        m_SeletablePathList.AddRange(paths);

        int index = 0;
        foreach(var path in m_SeletablePathList)
        {
            path.IndexInQuery = index;
            path.OnSelect += onPathSelectedCallback;
            path.OnDeselect += onPathDeselectedCallback;
            index++;
        }
    }

    public void CleanRegisteredPaths()
    {
        foreach(var path in m_SeletablePathList)
        {
            path.OnSelect -= onPathSelectedCallback;
            path.OnDeselect -= onPathDeselectedCallback;
        }

        m_SeletablePathList.Clear();
    }
    

    public void SelectPaths(List<MWB_SelectablePath> paths)
    {
        foreach(var path in paths)
        {
            selectPath(path);
        }
    }

    private void selectPath(MWB_SelectablePath path)
    {
        path.Select();
        m_SelectedPaths.Add(path);
        m_SelectedPathsIndex.Add(path.IndexInQuery);
    }
    
    private void reSelectPrevSelectedPath(MWB_SelectablePath path)
    {
        path.Select();
        m_SelectedPaths.Add(path);
    }

    private void onPathSelectedCallback(int index)
    {
        m_ColorDatas[index] = new LineBufferDrawer.ColorData(MWB_SelectablePath.SelectColor); 
    }

    private void onPathDeselectedCallback(int index)
    {
        m_ColorDatas[index] = new LineBufferDrawer.ColorData(MWB_SelectablePath.DeselectColor);
    }

    private void onEditorApplicationUpdate()
    {
        /*
        if (m_QueryThreads.Count > 0)
        {
            bool finished = false;
            foreach (var thread in m_QueryThreads)
            {
                if (!thread.IsAlive)
                {
                    thread
                }
            }
        }
        */
        /*
        if (m_SelectionType == SelectionType.Point)
        {
            if (m_QueryThread != null && !m_QueryThread.IsAlive)
            {
                while (m_SelectedPathQueue.Count > 0)
                {
                    var path = m_SelectedPathQueue.Dequeue();
                    selectPath(path);
                }
                updateColorData();
            }
        }
        */
        /*
        else if (m_SelectionType == SelectionType.Quad)
        {
            if (m_QueryThreads.Count > 0)
            {
                if (m_ThreadsTokenCounter == 0)
                {
                    Debug.Log("Quad Selected Path Count: " + m_SelectedPathQueue.Count);
                    while (m_SelectedPathQueue.Count > 0)
                    {
                        var path = m_SelectedPathQueue.Dequeue();
                        selectPath(path);
                    }
                    m_QueryThreads.Clear();
                    updateColorData();
                }
            }
        }*/
    }

    public void PositiveQuadSelection(Vector3 start, Vector3 end)
    {
        //m_SelectionType = SelectionType.Quad;

        DeselectAll();

        Matrix4x4 world2Screen = m_Camera.projectionMatrix * m_Camera.worldToCameraMatrix;
        int width = m_Camera.pixelWidth;
        int height = m_Camera.pixelHeight;
        
        Rect screenRect = MWBTest.Utils.GetScreenRectYFlip(start, end);

        m_Query.SetupConst(world2Screen, width, height);
        var results = m_Query.QuadSelection(m_LineDatas, screenRect);

        for (int i = 0; i < results.Length; i++)
        {
            if (results[i] > 0)
            {
                var path = m_SeletablePathList[m_LineDatas[i].LineStripIndex];
                selectPath(path);
            }
        }
        updateColorData();
    }

    public void PositivePointSelection(Vector3 point)
    {
        //m_SelectionType = SelectionType.Point;

        DeselectAll();

        Matrix4x4 world2Screen = m_Camera.projectionMatrix * m_Camera.worldToCameraMatrix;
        int width = m_Camera.pixelWidth;
        int height = m_Camera.pixelHeight;

        m_Query.SetupConst(world2Screen, width, height);
        var results = m_Query.PointSelection(m_LineDatas, point);

        float closest = float.PositiveInfinity;
        int closestIndex = -1;

        for (int i = 0; i < results.Length; i++)
        {
            var dis = results[i];
            if (dis < closest && dis < PointSelectionSqrMagnitudeThreshold)
            {
                closest = results[i];
                closestIndex = i;

                if (dis < PointSelectionSqrMagnitudeEndThreshold)
                    break;
            }
        }

        if (closestIndex > 0)
        {
            var path = m_SeletablePathList[m_LineDatas[closestIndex].LineStripIndex];
            selectPath(path);
        }
        updateColorData();
    }

    // clean selected path and index
    public void DeselectAll()
    {
        /* Deselect */
        foreach (var line in m_SelectedPaths)
        {
            line.Deselect();
        }
        
        m_SelectedPathsIndex.Clear();
        cleanSelectedPathList();
    }

    private void cleanSelectedPathList()
    {
        m_SelectedPaths.Clear();
    }
    
}

