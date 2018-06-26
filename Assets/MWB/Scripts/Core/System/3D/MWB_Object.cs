using MWBTest;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class MWB_Object : MonoBehaviour
{
    public List<MWB_SelectablePath> selectablePaths = new List<MWB_SelectablePath>();

    public MWB_Path RootPath = null;
    public MWB_Path CurrentMainPath = null;

    //private MWB_System m_Manager;
    //private MWB_PathList m_PathList;

    public delegate void onSelectHandler();

    public onSelectHandler OnSelect;
    public onSelectHandler OnDeselect;

    // used for creating animation clip
    public string hierachyName = "";

    public void Init()
    {
        OnSelect += () =>
        {
            //Debug.Log(name + " OnSelect , selectablePaths.Count = " + selectablePaths.Count);
            //MWB_SelectionQuery.Instance.CleanRegisteredPaths();
            //MWB_SelectionQuery.Instance.RegisterPaths(selectablePaths);

            List<MWB_SelectablePath> previousSelectedPaths = MWB_SelectionQuery.Instance.SelectedPaths;
            // valid world
            List<MWB_DummyObjectList> previousSelectedWorlds = new List<MWB_DummyObjectList>();

            foreach (MWB_SelectablePath previousSelectedPath in previousSelectedPaths)
            {
                MWB_Path t_previousSelectedPath = previousSelectedPath as MWB_Path;
                MWB_DummyObjectList t_world = t_previousSelectedPath.SourceDummyObject.correspondingDummyList;

                previousSelectedWorlds.Add(t_world);
            }

            List<MWB_SelectablePath> newSelectedPaths = new List<MWB_SelectablePath>();
            foreach (MWB_SelectablePath selectablePath in selectablePaths)
            {
                MWB_Path t_selectablePath = selectablePath as MWB_Path;

                if (t_selectablePath.SourceDummyObject != null && previousSelectedWorlds.Contains(t_selectablePath.SourceDummyObject.correspondingDummyList))
                {
                    newSelectedPaths.Add(selectablePath);
                }
            }

            MWB_SelectionQuery.Instance.ChangeSelectablePathList(selectablePaths);

            // pre select some of the paths base on previously selected worlds
            MWB_SelectionQuery.Instance.SelectPaths(newSelectedPaths);
        };
        OnDeselect += () =>
        {
            //Debug.Log(name + " OnDeselect ");
            //MWB_SelectionQuery.Instance.CleanRegisteredPaths();
        };
        //
        MWB_PathFactory Factory = MWB_PathFactory.Instance;
        if (CurrentMainPath == null)
        {
            RootPath = CurrentMainPath = Factory.GetPath();
        }
        else
        {
            var newPath = Factory.GetSubPath(CurrentMainPath);
            CurrentMainPath = newPath;
        }
        //selectablePaths.Add(RootPath);
    }

    public void InitializeRootPath()
    {
        MWB_PathFactory Factory = MWB_PathFactory.Instance;
        RootPath = CurrentMainPath = Factory.GetPath();
        //selectablePaths.Add(RootPath);
    }

    public void SetupLinedata()
    {
        int pathIndex = 0;
        foreach (MWB_SelectablePath selectablePath in selectablePaths)
        {
            // set up line data
            // selectablePath.LineDatas

            //List<LineBufferDrawer.LineData> list = new List<LineBufferDrawer.LineData>();
            for (int i = 0; i < selectablePath.Vertices.Count - 1; i++)
            {
                Vector3 pos0 = selectablePath.Vertices[i];
                Vector3 pos1 = selectablePath.Vertices[i + 1];

                selectablePath.AddLineData(pos0, pos1, pathIndex);
            }
            pathIndex++;
        }
    }
}