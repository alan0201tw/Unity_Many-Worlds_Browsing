using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MWB_PathFactory
{
    private static MWB_PathFactory instance;
    public static MWB_PathFactory Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new MWB_PathFactory();
            }

            if (instance == null)
            {
                Debug.LogError("MWB_PathFactory is needed.");
            }

            return instance;
        }
    }

    [Header("Prefab")]
    public MWB_Path PathInstance;
    public MWB_SubPath SubPathInstance;

    [Header("Settings")]
    public int InitialPoolSize = 128;
    public int SpawnLimitPerFrame = 64;

    private List<MWB_Path> m_PathsPool = new List<MWB_Path>();
    private List<MWB_SubPath> m_SubPathsPool = new List<MWB_SubPath>();
    
    MWB_PathFactory()
    {
        for (int i = 0; i < InitialPoolSize; i++)
        {
            createNewPath();
            createNewSubPath();
        }
    }

    private MWB_Path createNewPath()
    {
        var path = new MWB_Path(); //Instantiate(PathInstance, Vector3.zero, Quaternion.identity, this.transform);
        path.Init();
        m_PathsPool.Add(path);
        return path;
    }

    private MWB_Path createNewSubPath()
    {
        var subPath = new MWB_SubPath(); //Instantiate(SubPathInstance, Vector3.zero, Quaternion.identity, this.transform);
        subPath.Init();
        m_SubPathsPool.Add(subPath);
        return subPath;
    }

    public MWB_Path GetPath()
    {
        foreach (MWB_Path path in m_PathsPool)
        {
            if (path.IsInitialized)
            {
                path.Use();
                return path;
            }
        }

        for (int i = 0; i < SpawnLimitPerFrame; i++)
        {
            createNewPath();
        }
        MWB_Path ret = m_PathsPool[m_PathsPool.Count - 1];
        ret.Use();

        return ret;
    }

    public MWB_SubPath GetSubPath(MWB_Path parentPath)
    {
        foreach (MWB_SubPath path in m_SubPathsPool)
        {
            if (path.IsInitialized)
            {
                path.Use();
                path.SetParentPath(parentPath);
                return path;
            }
        }

        for (int i = 0; i < SpawnLimitPerFrame; i++)
        {
            createNewSubPath();
        }
        MWB_SubPath ret = m_SubPathsPool[m_SubPathsPool.Count - 1];
        ret.Use();
        ret.SetParentPath(parentPath);

        return ret;
    }
}
