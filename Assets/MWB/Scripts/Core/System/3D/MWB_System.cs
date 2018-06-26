using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TransformDataUtility;
using AnimationClipUtility;

using DummyCollisionPair = System.Collections.Generic.KeyValuePair<MWB_DummyObject, UnityEngine.Collision>;
using System.Linq;

public class MWB_System : MonoBehaviour
{
    private MWB_ObjectList m_objectList = new MWB_ObjectList();
    //private MWB_DummyObjectList m_collidingDummyObjects = new MWB_DummyObjectList();
    private Dictionary<MWB_DummyObject, Collision> m_collidingDummyObjects = new Dictionary<MWB_DummyObject, Collision>();

    private MWB_DummyObjectList focusingDummyObjectList;
    private List<MWB_DummyObjectList> forkedDummyObjectLists;
    //private int worldCount = 0;

    // record the frame number when a dummy list is forked
    private Dictionary<MWB_DummyObjectList, int> forkedFrameNumber = new Dictionary<MWB_DummyObjectList, int>();

    // record all forked / generated dummy object parent
    private List<GameObject> dummyParentList = new List<GameObject>();
    private GameObject dummyMasterParent;

    public Action OnSystemBegin = delegate { };
    public Action<float> OnSystemUpdateProgress = delegate { };
    public Action OnSystemComplete = delegate { };

    public Action OnSystemForcedTerminate = delegate { };

    private float m_Progress;   // 0 ~ 1
    public float Progress
    {
        get { return m_Progress; }
        private set
        {
            m_Progress = Mathf.Clamp01(value);
            OnSystemUpdateProgress(value);
        }
    }
    
    public float TimeStepPerFrame;
    public int SimulateFrameCount = 300;
    public int WorldForkedOnCollision = 1;

    int MaxLoopPerFrame = 30;
    int LoopCounter = 0;

    // use the last 2 layers
    readonly int MWBRunningLayer = 30;
    readonly int MWBWaitingLayer = 31;

    int registerPathCount = 0;

    [SerializeField]
    private float m_CollisionThresholdCoef = 5f;
    public float CollisionThresholdCoef { get { return m_CollisionThresholdCoef; } }

    private bool isForceStop = false;

    private void OnEnable()
    {
        // clost physic autoSimulation
        Physics.autoSimulation = false;
        TimeStepPerFrame = Time.fixedDeltaTime;

        // set the 2 layers to our desire relation
        Physics.IgnoreLayerCollision(MWBRunningLayer, MWBWaitingLayer, true);
        Physics.IgnoreLayerCollision(MWBWaitingLayer, MWBWaitingLayer, true);
    }

    void Start()
    {
        // Find all WMB_Objects in children
        MWB_Object[] objs = GetComponentsInChildren<MWB_Object>();
        foreach (var obj in objs)
        {
            RegisterMWB_Object(obj);
        }
    }

    public void Simulate()
    {
        if (MWBEditor.EnableMWBEditorMode(this))
        {
            CleanUp();
            StartCoroutine(Run());
        }
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.S))
        {
            Terminate();
        }
    }

    public void Terminate()
    {
        isForceStop = true;

        Debug.Log("Forcing Terminate");
    }
    
    private void terminate()
    {
        OnSystemForcedTerminate();
        CleanUp();
    }

    //public int GetWorldIndex(MWB_DummyObjectList world)
    //{
    //    return forkedDummyObjectLists.FindIndex(x => x.GetHashCode() == world.GetHashCode());
    //}

    public void Preview(MWB_Path selectedPath, int frameIndex)
    {
        if (frameIndex < 0 || frameIndex > SimulateFrameCount+1)
        {
            Debug.LogWarning("frameIndex invalid!");
            return;
        }

        if (selectedPath == null)
        {
            Debug.LogWarning("selectedPath invalid!");
            return;
        }

        var world = selectedPath.SourceDummyObject.correspondingDummyList;

        foreach (MWB_DummyObject dummy in world.MWB_DummyObjects)
        {
            Stack<TransformDataSegment> dataSegmentStack = new Stack<TransformDataSegment>();
            TransformDataSegment currentDataSegment = dummy.transformDataSegment;

            while (currentDataSegment != null)
            {
                dataSegmentStack.Push(currentDataSegment);
                // travese upward in fork tree
                currentDataSegment = currentDataSegment.previousSegment;
            }

            int currentFrameIndex = 0;

            while (dataSegmentStack.Count > 0)
            {
                currentDataSegment = dataSegmentStack.Pop();
                if (currentDataSegment == null)
                    break;

                if (currentFrameIndex + currentDataSegment.transformData.Count > frameIndex)
                {
                    //TODO set the objects' transform
                    dummy.objectSource.transform.localPosition = currentDataSegment.transformData[frameIndex - currentFrameIndex].localPosition;
                    dummy.objectSource.transform.localRotation = currentDataSegment.transformData[frameIndex - currentFrameIndex].localRotation;
                    dummy.objectSource.transform.localScale = currentDataSegment.transformData[frameIndex - currentFrameIndex].localScale;
                    break;
                }
                else
                {
                    currentFrameIndex += currentDataSegment.transformData.Count;
                }
            }
        }
    }

    public MWB_ObjectList GetObjectList()
    {
        return m_objectList;
    }

    public void RegisterMWB_Object(MWB_Object mwb_object)
    {
        m_objectList.AddToObjectList(mwb_object);

        // give the registering mwbObject its transform name
        mwb_object.hierachyName = TransformNameUtility.GetTransformPathName(transform, mwb_object.transform);

        mwb_object.Init();
    }

    // Register as collided only if dummy is in current iterated dummy list
    public void RegisterAsCollidedInThisFrame(MWB_DummyObject mwbObject, Collision collision)
    {
        if (focusingDummyObjectList.MWB_DummyObjects.Contains(mwbObject))
        {
            if (!m_collidingDummyObjects.ContainsKey(mwbObject))
            {
                m_collidingDummyObjects.Add(mwbObject, collision);
            }
        }
    }

    // !!! IMPORTANT : If a collision already happens and you disactive object then active , OnCollisionEnter will trigger again
    public IEnumerator Run()
    {
        OnSystemBegin.Invoke();

        dummyMasterParent = new GameObject("_TempDummyMasterParent");

        // might need a lifetime for forked MWB_System
        forkedDummyObjectLists = new List<MWB_DummyObjectList>
        {
            GenerateDummyObjects(m_objectList , true)
        };
        // set original object to disactive, and set to wait layer to avoid unnecessary collision
        foreach (MWB_Object mwbObject in m_objectList.MWB_Objects)
        {
            mwbObject.gameObject.SetActive(false);
            mwbObject.gameObject.layer = MWBWaitingLayer;
        }
        // for each forked dummyObjectList, check if the system needs to be forked
        for (int i = 0; i < forkedDummyObjectLists.Count; i++)
        {
            if (isForceStop)
            {
                break;
            }

            MWB_DummyObjectList dummyObjectList = forkedDummyObjectLists[i];
            // Set focusing dummyObjectList
            focusingDummyObjectList = dummyObjectList;

            foreach (MWB_DummyObject dummyObject in dummyObjectList.MWB_DummyObjects)
            {
                dummyObject.gameObject.layer = MWBRunningLayer;
                // try to turn on dummys
                dummyObject.rigidbody.isKinematic = false;
                // regain maintained rigidbody state ( obtained when forked )
                Rigidbody dummyRigidbody = dummyObject.GetComponent<Rigidbody>();
                dummyObject.rigidbodyState.SetRigidbodyState(ref dummyRigidbody);
                // turn visible
                dummyObject.gameObject.GetComponent<Renderer>().enabled = true;
            }

            // simulation
            for (int currentFrame = 0; currentFrame < SimulateFrameCount; currentFrame++)
            {
                if (forkedFrameNumber.ContainsKey(dummyObjectList))
                {
                    currentFrame = forkedFrameNumber[dummyObjectList];
                    // !!! IMPORTANT !!!
                    forkedFrameNumber.Remove(dummyObjectList);
                    //
                    continue;
                }
                // Record position to path ( when simulating )
                foreach (MWB_DummyObject dummy in dummyObjectList.MWB_DummyObjects)
                {
                    dummy.RecordPosition();
                }

                // simulate
                Physics.Simulate(TimeStepPerFrame);
                // for each collided object , fork dummy objects (X)
                // if this system has a dummy collision, fork once.

                LoopCounter++;
                if (LoopCounter > MaxLoopPerFrame)
                {
                    LoopCounter = 0;
                    yield return null;
                }

                if (m_collidingDummyObjects.Count > 0)
                {
                    for (int k = 0; k < WorldForkedOnCollision; k++)
                    {
                        // fork every object in currentObjectList by their current status
                        MWB_DummyObjectList forkedMWB_DummyObjectList = ForkDummyObjects(dummyObjectList, true , k);
                        
                        // add track to every objects that we just created
                        forkedDummyObjectLists.Add(forkedMWB_DummyObjectList);
                        // track frame
                        forkedFrameNumber.Add(forkedMWB_DummyObjectList, currentFrame);
                    }
                }

                m_collidingDummyObjects.Clear();
            }

            // Set back to waiting for simulation
            foreach (MWB_DummyObject dummyObject in dummyObjectList.MWB_DummyObjects)
            {
                dummyObject.gameObject.layer = MWBWaitingLayer;
                // try to turn off dummys
                dummyObject.rigidbody.isKinematic = true;
                dummyObject.gameObject.GetComponent<Renderer>().enabled = false;

                // register to selectable

                // ??
                //dummyObject.pathIndex = dummyObject.objectSource.selectablePaths.Count;
                // Debug.Log(dummyObject.pathIndex + " " + dummyObject.correspondingDummyList.worldIndex);

                // Do line set up at the end

                //dummyObject.objectSource.selectablePaths.Add(dummyObject.dummyMainPath);

                //
                //MWB_SelectionQuery.Instance.RegisterPath(dummyObject.dummyMainPath);
                registerPathCount++;
            }

            // update
            Progress += 1.0f / forkedDummyObjectLists.Count;
        }

        foreach (MWB_Object mwbObject in m_objectList.MWB_Objects)
        {
            mwbObject.gameObject.SetActive(true);
            mwbObject.gameObject.layer = MWBRunningLayer;

            // for updated line renderer!!!

            // Do line set up at the end
            //mwbObject.SetupLinedata();
        }

        forkedFrameNumber.Clear();
        m_collidingDummyObjects.Clear();

        // TEST PURPOSE
        // AnimationClip clip = MWBDummyClipUtility.GenerateClipFromDummyList(forkedDummyObjectLists[3]);

        // ClipExportUtility.ExportAnimationClip(clip, @"Assets/test.anim");
        //

        // output information

        Debug.Log("MWB_DummyObject.pathNumber = "+MWB_DummyObject.pathNumber);
        Debug.Log("MWB_DummyObject.dotNumber = " + MWB_DummyObject.dotNumber);
        Debug.Log("registerPathCount = " + registerPathCount);

        //
        if (isForceStop)
        {
            terminate();
            isForceStop = false;
        }
        else
        {
            OnSystemComplete.Invoke();
        }
        yield return null;
    }

    public void SaveAnimationClipFromPath(MWB_Path path, string filePath)
    {
        var world = path.SourceDummyObject.correspondingDummyList;
        var clip = MWBDummyClipUtility.GenerateClipFromDummyList(world);
        ClipExportUtility.ExportAnimationClip(clip, filePath);
    }

    private MWB_DummyObjectList ForkDummyObjects(MWB_DummyObjectList dummyObjectList, bool isInitializedAsAble, int worldIndex = 0)
    {
        //Debug.Log("Forking");
        GameObject parent = new GameObject("collide-forked dummy parent"); // this will also instantiate
        // !!! IMPORTANT : set the transform data of dummys' parent to the same as original objects' 
        parent.transform.parent = dummyMasterParent.transform;
        // parent, which is the MWB_SYSTEM
        parent.transform.localPosition = transform.localPosition;
        parent.transform.localRotation = transform.localRotation;
        parent.transform.localScale = transform.localScale;
        // add parent to dummyParent list for clean up !
        dummyParentList.Add(parent);

        MWB_DummyObjectList forkedDummyObjectList = new MWB_DummyObjectList();

        foreach (MWB_DummyObject mwbDummyObject in dummyObjectList.MWB_DummyObjects)
        {
            GameObject forkedObject = Instantiate(mwbDummyObject.gameObject, parent.transform);

            // Set layer
            forkedObject.layer = MWBWaitingLayer;
            forkedObject.SetActive(isInitializedAsAble);
            // initilize as kinematic
            forkedObject.GetComponent<Rigidbody>().isKinematic = true;
            forkedObject.gameObject.GetComponent<Renderer>().enabled = false;
            //
            Destroy(forkedObject.GetComponent<MWB_Object>());

            MWB_DummyObject forkedDummy = forkedObject.GetComponent<MWB_DummyObject>();

            forkedDummy.Manager = this;
            // record object source
            forkedDummy.objectSource = mwbDummyObject.objectSource;
            // record fork source ( initial object has null )
            forkedDummy.forkedSource = mwbDummyObject;
            // record corresponding dummy list
            forkedDummy.correspondingDummyList = forkedDummyObjectList;

            // record previous transform data segment (initial dummy object has null)
            TransformDataSegment parentSegment = mwbDummyObject.transformDataSegment;
            forkedDummy.transformDataSegment.previousSegment = parentSegment;
            mwbDummyObject.transformDataSegment = new TransformDataSegment(parentSegment);

            // use main path to generate two sub
            MWB_Path parentPath = mwbDummyObject.dummyMainPath;

            //mwbDummyObject.RecordPosition(); // !!! Record position to avoid broken line segment

            mwbDummyObject.SetSubPath(parentPath);

            //mwbDummyObject.RecordPosition(); // !!! Record position to avoid broken line segment

            forkedDummy.SetSubPath(parentPath);

            // assign reference of dummy to path
            mwbDummyObject.dummyMainPath.SourceDummyObject = mwbDummyObject;
            forkedDummy.dummyMainPath.SourceDummyObject = forkedDummy;
            //

            // set pathIndex
            //forkedDummy.pathIndex = mwbDummyObject.pathIndex;

            //
            Vector3 additionalImpulse;
            if (m_collidingDummyObjects.ContainsKey(mwbDummyObject))
            {
                // TODO : add a logical additional pulse, rather than just random a force

                Collision collision = m_collidingDummyObjects[mwbDummyObject];

                //additionalImpulse = Vector3.Cross(collision.impulse, Vector3.up) * collision.impulse.magnitude * 5f;
                //additionalImpulse = new Vector3(UnityEngine.Random.Range(-100, 100), 0, UnityEngine.Random.Range(-100, 100));

                Vector3 origin = Vector3.zero;
                int counter = 0;
                foreach (var contactPoint in collision.contacts)
                {
                    origin += contactPoint.point;
                    counter++;
                }
                origin /= counter;

                List<Vector3> perturbation = SinDistributionUtility.CalculatePerturbation(origin, collision.impulse);

                if (perturbation.Count > worldIndex)
                    additionalImpulse = perturbation[worldIndex];
                else
                    additionalImpulse = perturbation[perturbation.Count - 1];
            }
            else
            {
                additionalImpulse = Vector3.zero;
            }
            //

            // maintain rigidbody info , otherwise it'll only copy variables info but not state info
            RigidbodyState rigidbodyState = new RigidbodyState(mwbDummyObject.GetComponent<Rigidbody>() , additionalImpulse);
            forkedDummy.rigidbodyState = rigidbodyState;
            forkedDummyObjectList.AddToDummyObjectList(forkedDummy);
            //
        }

        return forkedDummyObjectList;
    }

    private MWB_DummyObjectList GenerateDummyObjects(MWB_ObjectList objectList, bool isInitializedAsAble)
    {
        MWB_DummyObjectList dummyObjectList = new MWB_DummyObjectList();

        GameObject parent = new GameObject("initial dummy parent");
        parent.transform.parent = dummyMasterParent.transform;
        // !!! IMPORTANT
        parent.transform.localPosition = transform.localPosition;
        parent.transform.localRotation = transform.localRotation;
        parent.transform.localScale = transform.localScale;
        // add parent to dummyParent list for clean up !
        dummyParentList.Add(parent);
        //
        foreach (MWB_Object mwbObject in objectList.MWB_Objects)
        {
            GameObject forkedObject = Instantiate(mwbObject.gameObject, parent.transform);
            // Set layer
            forkedObject.layer = MWBWaitingLayer;
            forkedObject.SetActive(isInitializedAsAble);
            // turning MWB_Object to Dummy Object
            Destroy(forkedObject.GetComponent<MWB_Object>());

            var dummy = forkedObject.AddComponent<MWB_DummyObject>();
            dummy.Manager = this;
            // record object source
            dummy.objectSource = mwbObject;
            // record fork source ( initial object has null )
            dummy.forkedSource = null;
            //record corresponding dummy list
            dummy.correspondingDummyList = dummyObjectList;

            // set pathIndex
            //dummy.pathIndex = 0;

            // record previous transform data segment (initial dummy object has null)
            dummy.transformDataSegment.previousSegment = null;

            // use main path to generate sub path
            dummy.SetSubPath(mwbObject.CurrentMainPath);

            // assign reference of dummy to path
            dummy.dummyMainPath.SourceDummyObject = dummy;
            //
            dummyObjectList.AddToDummyObjectList(dummy);
        }

        return dummyObjectList;
    }

    private void CleanUp()
    {
        m_collidingDummyObjects.Clear();
        focusingDummyObjectList = null;
        forkedFrameNumber.Clear();

        // destroy all dummys used for simulation by destroying their parent
        //foreach (GameObject dummyParent in dummyParentList)
        //{
        //    Destroy(dummyParent);
        //}
        Destroy(dummyMasterParent);
        dummyParentList.Clear();

        MWB_SelectionQuery.Instance.CleanRegisteredPaths();

        // clear all registered object's selectablePaths to empty
        foreach (MWB_Object mwb_Object in m_objectList.MWB_Objects)
        {
            mwb_Object.selectablePaths.Clear();
            mwb_Object.InitializeRootPath();
        }
    }
}