using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;
using MWBTest;
using System.IO;

[InitializeOnLoad]
public class MWBEditor : EditorWindow
{
    class MWBAnimationWindowStyles
    {
        public static GUIContent playContent = EditorGUIUtility.IconContent("Animation.Play", "|Play the animation.");

        public static GUIContent recordContent = EditorGUIUtility.IconContent("Animation.Record", "|Save current animation to a clip.");

        public static GUIContent prevKeyContent = EditorGUIUtility.IconContent("Animation.PrevKey", "|Go to previous keyframe.");

        public static GUIContent nextKeyContent = EditorGUIUtility.IconContent("Animation.NextKey", "|Go to next keyframe.");

        public static GUIContent firstKeyContent = EditorGUIUtility.IconContent("Animation.FirstKey", "|Go to the beginning of the animation clip.");

        public static GUIContent lastKeyContent = EditorGUIUtility.IconContent("Animation.LastKey", "|Go to the end of the animation clip.");

        public static GUIContent addKeyframeContent = EditorGUIUtility.IconContent("Animation.AddKeyframe", "|Add keyframe.");

        public static GUIContent addEventContent = EditorGUIUtility.IconContent("Animation.AddEvent", "|Add event.");

        public static GUIContent sequencerLinkContent = EditorGUIUtility.IconContent("Animation.SequencerLink", "|Animation Window is linked to Sequence Editor.  Press to Unlink.");

        public static GUIStyle playHead = "AnimationPlayHead";

        public static GUIStyle curveEditorBackground = "CurveEditorBackground";

        public static GUIStyle curveEditorLabelTickmarks = "CurveEditorLabelTickmarks";

        public static GUIStyle eventBackground = "AnimationEventBackground";

        public static GUIStyle eventTooltip = "AnimationEventTooltip";

        public static GUIStyle eventTooltipArrow = "AnimationEventTooltipArrow";

        public static GUIStyle keyframeBackground = "horizontalSlider";//"AnimationKeyframeBackground";

        public static GUIStyle keyframeThumb = "horizontalSliderThumb";

        public static GUIStyle timelineTick = "AnimationTimelineTick";

        public static GUIStyle dopeSheetKeyframe = "Dopesheetkeyframe";

        public static GUIStyle dopeSheetBackground = "DopesheetBackground";

        public static GUIStyle popupCurveDropdown = "PopupCurveDropdown";

        public static GUIStyle popupCurveEditorBackground = "PopupCurveEditorBackground";

        public static GUIStyle popupCurveEditorSwatch = "PopupCurveEditorSwatch";

        public static GUIStyle popupCurveSwatchBackground = "PopupCurveSwatchBackground";

        public static GUIStyle miniToolbar = new GUIStyle(EditorStyles.toolbar);

        public static GUIStyle miniToolbarButton = new GUIStyle(EditorStyles.toolbarButton);

        public static GUIStyle toolbarLabel = new GUIStyle(EditorStyles.toolbarPopup);
    }

    const string c_MenuAnimationName = "MWB/WMB Animation";
    const float c_MousePointSelectionThreshold = 3.0f;

    const float c_HierarchyRectWidth = 160;
    const float c_HierarchyRectShowHeight = 100;
    const float c_HierarchyRectHideHeight = 20;

    const float c_AnimationRectWidth = 320;
    const float c_AnimationRectHeight = 160;

    static Color c_PosSelectionBoxColor = new Color(1, 1, 0);
    //static Color c_NegSelectionBoxColor = new Color(0, 0, 1);

    static MWB_SelectionQuery s_SelectionQuery = null;

    static Vector2 s_HierarchyRectSize = new Vector2(c_HierarchyRectWidth, c_HierarchyRectShowHeight);
    static Rect s_HierarchyRect = new Rect(new Vector2(10, 50), s_HierarchyRectSize);

    static Vector2 s_HierarchyScrollPosition = new Vector2(0, 0);

    //static Vector2 s_AnimationRectSize = new Vector2(c_AnimationRectWidth, c_AnimationRectHeight);
    //static Rect s_AnimationRect = new Rect(new Vector2(500, 400), s_AnimationRectSize);

    [Header("State")]
    static bool s_IsEnabled = false;

    private static MWBEditor s_Instance;
    public static MWBEditor Instance
    {
        get { return s_Instance; }
        private set { s_Instance = value; }
    }

    static bool s_IsHierarchyWindowShown = true;
    //static bool s_IsAnimationWindowShown = false;

    static bool s_IsLeftMouseDragging = false;
    static Vector2 s_MouseStartPosition = Vector2.zero;
    static Vector2 s_MouseCurrentPosition = Vector2.zero;

    [Header("System")]
    static MWB_System s_CurrSystem = null;
    //TODO: MWB_World s_CurrWorld;
    static MWB_ObjectList s_SimulatedObjects;
    static int s_SelectedObjectIndex = -1;
    static List<string> s_SimulatedObjectsName;

    [Header("Animation")]
    private int m_SimulateFrameCount;
    private int m_CurrentFrame = 0;

    private float m_EditorLastFrameTime = 0.0f;

    private float m_SystemProgress = 0.0f;
    private bool m_IsSystemRunning = false;
    private bool m_IsProgressBarShown = false;

    private bool m_IsPreviewPlaying = false;
    public bool IsPreviewPlaying 
    { 
        get { return m_IsPreviewPlaying; } 
        private set 
        {
            if (m_IsPreviewPlaying != value)
            {
                m_EditorLastFrameTime = Time.time;
            }
            m_IsPreviewPlaying = value;
        } 
    }

    public int CurrentFrame 
    { 
        get { return m_CurrentFrame; } 

        private set
        {
            if (value > m_SimulateFrameCount)
                m_CurrentFrame = m_SimulateFrameCount;
            else if (value < 0)
                m_CurrentFrame = 0;
            else
                m_CurrentFrame = value;

            // TODO: selectedPath
            if (s_CurrSystem)
                s_CurrSystem.Preview(MWB_SelectionQuery.Instance.SelectedPath, m_CurrentFrame);
        }
    }
    
    static MWBEditor()
    {
        UnityEditor.EditorApplication.update += Update;
    }

    static void Update()
    {
        if (Instance)
        {
            Instance.onEditorApplicationUpdate();
        }
    }

    [MenuItem(c_MenuAnimationName)]
    public static bool ShowMWBAnimationWindow()
    {
        EditorWindow editorWindow = GetWindow(typeof(MWBEditor), false);
        editorWindow.autoRepaintOnSceneChange = true;
        editorWindow.Show();
        editorWindow.titleContent = new GUIContent("MWB Anim", EditorGUIUtility.FindTexture("SettingsIcon"));

        editorWindow.wantsMouseMove = true;
        return false;
    }

    static void onPlayModeStateChanged(PlayModeStateChange state)
    {
        switch(state)
        {
            case PlayModeStateChange.ExitingPlayMode:
                EditorPrefs.SetBool(c_MenuAnimationName, false);
                Menu.SetChecked(c_MenuAnimationName, false);
                MWBEditor.DisableMWBEditorMode();
                break;
        }
    }

    public static bool EnableMWBEditorMode(MWB_System system)
    {
        if (!EditorApplication.isPlaying)
        {
            SceneView.lastActiveSceneView.ShowNotification(new GUIContent("You can only use MWB Editor Mode in play mode."));
            return s_IsEnabled;
        }

        Selection.activeGameObject = null;

        //Debug.Log("Enter Editor Mode");

        s_SelectionQuery = MWB_SelectionQuery.Instance;
        s_SelectionQuery.SetCamera(SceneView.lastActiveSceneView.camera);

        SceneView.onSceneGUIDelegate += onSceneGUIDelegate;
        SceneView.lastActiveSceneView.ShowNotification(new GUIContent("Enter MWB Editor Mode"));
        EditorApplication.playModeStateChanged += onPlayModeStateChanged;

        setCurrentSystem(system);

        s_IsEnabled = true;

        SceneView.lastActiveSceneView.Repaint();

        return s_IsEnabled;
    }

    public static bool DisableMWBEditorMode()
    {
        //Debug.Log("Leave Editor Mode");

        SceneView.onSceneGUIDelegate -= onSceneGUIDelegate;
        EditorApplication.playModeStateChanged -= onPlayModeStateChanged;

        setCurrentSystem(null);

        s_IsEnabled = false;

        SceneView.lastActiveSceneView.Repaint();

        return s_IsEnabled;
    }

    static void setCurrentSystem(MWB_System system)
    {
        s_CurrSystem = system;

        if (s_CurrSystem != null)
        {
            s_SimulatedObjects = s_CurrSystem.GetObjectList();
            s_SimulatedObjectsName = new List<string>();
            foreach (var obj in s_SimulatedObjects.MWB_Objects)
            {
                s_SimulatedObjectsName.Add(obj.gameObject.name);
            }

            s_CurrSystem.OnSystemBegin += Instance.onSystemBegin;
            s_CurrSystem.OnSystemUpdateProgress += Instance.onSystemUpdateProgress;
            s_CurrSystem.OnSystemComplete += Instance.onSystemComplete;

            s_CurrSystem.OnSystemForcedTerminate += Instance.onSystemtTerminate;
        }
        else
        {
            s_SimulatedObjects = null;
            s_SimulatedObjectsName.Clear();
        }

        initAnimationView();
    }

    static void initAnimationView()
    {
        //Debug.Log("Init Animation View");
        if (Instance)
        {
            if (s_CurrSystem)
            {
                Instance.m_SimulateFrameCount = s_CurrSystem.SimulateFrameCount;
                //Debug.Log("View Count: " + Instance.m_SimulateFrameCount);
            }
            else
            {
                Instance.m_SimulateFrameCount = -1;
                //Debug.Log("View Count: " + Instance.m_SimulateFrameCount);
            }
            Instance.IsPreviewPlaying = false;
            Instance.Repaint();
        }
    }

    static void onSceneGUIDelegate(SceneView sceneView)
    {
        if (SceneView.lastActiveSceneView == null)
            return;

        sceneView = SceneView.lastActiveSceneView;

        int controlID = GUIUtility.GetControlID(FocusType.Passive);

        HandleUtility.AddDefaultControl(controlID);

        // Subwindows
        {

            if (s_CurrSystem != null)
            {
                s_HierarchyRect = 
                    Math3d.ClampToDstRect(
                    GUILayout.Window(0, s_HierarchyRect, hierarchyWindow, new GUIContent("MWB Hierarchy"), GUILayout.MinHeight(17)),
                    sceneView.position);
                //s_AnimationRect = Math3d.ClampToDstRect(GUILayout.Window(1, s_AnimationRect, AnimationWindow, "MWB Animation"), sceneView.position);
            }
        }

        // Mouse event, Quad
        {
            mouseEvent(sceneView);

            drawSelectionBox(sceneView);
        }

        // 3D GUI Handling
        {
            Handles.BeginGUI();

            // set Handles and Gizmos drawing target to selection editor camera
            if (sceneView.camera != null)
            {
                Handles.SetCamera(sceneView.camera);
            }
            // draw paths
            handlesDrawPaths();

            Handles.EndGUI();
        }

        SceneView.lastActiveSceneView.Repaint();
    }

    static void hierarchyWindow(int id)
    {
        // Draw Hierachy Window
        Handles.BeginGUI();

        if (s_IsHierarchyWindowShown)
        {
            GUILayout.BeginVertical();

            drawSystemName();
            drawSimulatedObjectsView();

            GUILayout.EndVertical();
        }

        // Toggle Hierarchy Window
        string toggleHierarchyBtnName = "";
        if (s_IsHierarchyWindowShown)
        {
            toggleHierarchyBtnName = "Hide";
        }
        else
        {
            toggleHierarchyBtnName = "Show";
        }

        if (GUILayout.Button(toggleHierarchyBtnName))
        {
            s_IsHierarchyWindowShown = !s_IsHierarchyWindowShown;

            if (s_IsHierarchyWindowShown)
            {
                s_HierarchyRect.height = c_HierarchyRectShowHeight;
            }
            else
            {
                s_HierarchyRect.height = c_HierarchyRectHideHeight;
            }
        }
        
        GUI.DragWindow();

        Handles.EndGUI();
    }

    static void drawSystemName()
    {
        GUILayout.BeginHorizontal();

        GUILayout.FlexibleSpace();

        GUIStyle style = new GUIStyle("BoldLabel");
        //s_CurrSystem = EditorGUILayout.ObjectField(s_CurrSystem, typeof(MWB_System), true) as MWB_System;
        EditorGUIUtility.SetIconSize(new Vector2(15, 15));
        GUILayout.Label(new GUIContent(" " + s_CurrSystem.gameObject.name, EditorGUIUtility.FindTexture("GameObject Icon")), style);
        GUILayout.FlexibleSpace();

        GUILayout.EndHorizontal();
    }

    static void drawSimulatedObjectsView()
    {
        s_HierarchyScrollPosition =
            GUILayout.BeginScrollView(s_HierarchyScrollPosition, GUILayout.Width(c_HierarchyRectWidth), GUILayout.Height(c_HierarchyRectShowHeight));

        GUIStyle style = new GUIStyle("ProjectBrowserGridLabel");
        int newSelectedIndex =
            GUILayout.SelectionGrid(s_SelectedObjectIndex, s_SimulatedObjectsName.ToArray(), 1, style);

        if (newSelectedIndex != s_SelectedObjectIndex)
        {
            selectObject(newSelectedIndex);
        }

        GUILayout.EndScrollView();
    }

    static void selectObject(int index)
    {
        if (s_SelectedObjectIndex != -1)
            s_SimulatedObjects.MWB_Objects[s_SelectedObjectIndex].OnDeselect();

        var selectedObj = s_SimulatedObjects.MWB_Objects[index];

        selectedObj.OnSelect();
        //Selection.activeGameObject = selectedObj.gameObject;

        s_SelectedObjectIndex = index;
        //Debug.Log("Select object : " + s_SimulatedObjectsName[s_SelectedObjectIndex]);
    }

    static void handlesDrawPaths()
    {
        s_SelectionQuery.RenderLines();
    }

    static void mouseEvent(SceneView sceneView)
    {
        Event curr = Event.current;

        if (!curr.isMouse)
            return;

        Camera camera = sceneView.camera;

        s_MouseCurrentPosition = curr.mousePosition;
        s_MouseCurrentPosition.y = camera.pixelHeight - s_MouseCurrentPosition.y;

        // Mouse Down
        if (curr.type == EventType.MouseDown)
        {
            switch (curr.button)
            {
                case 0:
                    {
                        if (s_IsLeftMouseDragging == false)
                        {
                            s_IsLeftMouseDragging = true;
                            s_MouseStartPosition = s_MouseCurrentPosition;
                        }
                    }
                    break;
                case 1:
                    break;
            }
        }

        // Drag
        if (curr.type == EventType.MouseDrag)
        {
            switch (curr.button)
            {
                case 0:
                    {
                        if (s_IsLeftMouseDragging == false)
                        {
                            s_IsLeftMouseDragging = true;
                            s_MouseStartPosition = s_MouseCurrentPosition;
                        }
                    }
                    break;
            }
        }

        // Mouse Up
        if (curr.type == EventType.MouseUp)
        {
            switch (curr.button)
            {
                case 0:
                    {
                        if (s_IsLeftMouseDragging)
                        {
                            var endPos = s_MouseCurrentPosition;
                            var startPos = s_MouseStartPosition;

                            if ((endPos - startPos).sqrMagnitude < c_MousePointSelectionThreshold)
                            {
                                // first check if selecting object?
                                bool isSelectingObj = false;
                                Ray ray = HandleUtility.GUIPointToWorldRay(curr.mousePosition);
                                RaycastHit hit;
                                if (Physics.Raycast(ray, out hit))
                                {
                                    GameObject hitObj = hit.collider.gameObject;
                                    MWB_Object mwbObj = hitObj.GetComponent<MWB_Object>();
                                    if (mwbObj)
                                    {
                                        int index = s_SimulatedObjects.MWB_Objects.FindIndex(obj => obj == mwbObj);
                                        if(index >= 0)
                                        {
                                            selectObject(index);
                                            isSelectingObj = true;
                                        }
                                    }
                                }

                                if (!isSelectingObj)
                                {
                                    //Debug.Log("Point Selection");
                                    s_SelectionQuery.PositivePointSelection((startPos + endPos) / 2);
                                }
                            }
                            else
                            {
                                //Debug.Log("Quad Selection");
                                s_SelectionQuery.PositiveQuadSelection(startPos, endPos);
                            }
                        }

                        s_IsLeftMouseDragging = false;
                    }
                    break;
                case 1:
                    {
                        s_IsLeftMouseDragging = false;
                        //s_SelectionQuery.DeselectAll();
                    }
                    break;
            }
        }
    }

    static void drawSelectionBox(SceneView sceneView)
    {
        // Draw Selection Box Window
        Handles.BeginGUI();

        Camera camera = sceneView.camera;

        if (s_IsLeftMouseDragging == true)
        {
            var currPos = s_MouseCurrentPosition;
            currPos.y = camera.pixelHeight - currPos.y;

            var startPos = s_MouseStartPosition;
            startPos.y = camera.pixelHeight - startPos.y;

            Rect rect = Utils.GetScreenRectYFlip(startPos, currPos);

            var col = c_PosSelectionBoxColor;
            col.a = 0.4f;
            Utils.DrawScreenRect(rect, col);
            Utils.DrawScreenRectBorder(rect, 3, col);
        }

        Handles.EndGUI();
    }

    void OnEnable()
    {
        Instance = this;
        initAnimationView();
    }

    void OnGUI()
    {
        drawAnimationView(this.position);   // TODO: move to enabled
        if (m_IsProgressBarShown)
            drawProgressBar();
    }

    void saveAnimationFile()
    {
        var path = EditorUtility.SaveFilePanelInProject(
            "Save MWB Animation",
            "clip",
            "anim",
            "Please enter a file name");

        if (path != "")
        {
            s_CurrSystem.SaveAnimationClipFromPath(MWB_SelectionQuery.Instance.SelectedPath as MWB_Path, path);
            //Debug.Log("Save at: " + path);
        }
    }

    void drawAnimationView(Rect position)
    {
        if (!s_IsEnabled)
        {
            EditorGUI.BeginDisabledGroup(true);
        }

        GUILayout.BeginHorizontal();
        {
            GUILayout.BeginVertical();
            {
                GUILayout.BeginHorizontal(EditorStyles.toolbarButton);
                {
                    //GUILayout.Label(m_SimulateFrameCount.ToString());
                    if (GUILayout.Button(MWBAnimationWindowStyles.recordContent, EditorStyles.toolbarButton))
                    {
                        saveAnimationFile();
                    }
                    if (GUILayout.Button(MWBAnimationWindowStyles.firstKeyContent, EditorStyles.toolbarButton))
                    {
                        CurrentFrame = 0;
                    }
                    if (GUILayout.Button(MWBAnimationWindowStyles.prevKeyContent, EditorStyles.toolbarButton))
                    {
                        CurrentFrame--;
                    }

                    IsPreviewPlaying = GUILayout.Toggle(m_IsPreviewPlaying, MWBAnimationWindowStyles.playContent, EditorStyles.toolbarButton);
                    
                    if (GUILayout.Button(MWBAnimationWindowStyles.nextKeyContent, EditorStyles.toolbarButton))
                    {
                        CurrentFrame++;
                    }
                    if (GUILayout.Button(MWBAnimationWindowStyles.lastKeyContent, EditorStyles.toolbarButton))
                    {
                        CurrentFrame = m_SimulateFrameCount;
                    }
                    CurrentFrame = EditorGUILayout.IntField(CurrentFrame, EditorStyles.toolbarTextField, GUILayout.Width(40));
                    GUILayout.FlexibleSpace();

                    //Rect rect = GUILayoutUtility.GetRect(200, 18f);
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }
        GUILayout.EndHorizontal();
        
        if (s_IsEnabled)
        {
            drawAnimationClipSlider(position);
        }
        else
        {
            EditorGUI.EndDisabledGroup();

            GUILayout.BeginVertical();
            {
                GUILayout.FlexibleSpace();
                GUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(new GUIContent("You can only browse an MWB animation in MWB Editor Mode."));
                    GUILayout.FlexibleSpace();
                }
                GUILayout.EndHorizontal();
                GUILayout.FlexibleSpace();
            }
            GUILayout.EndVertical();
        }
    }

    void drawAnimationClipSlider(Rect position)
    {
        GUILayout.BeginVertical();
        {
            GUILayout.FlexibleSpace();
            //GUILayout.BeginHorizontal();
            {
                Vector2 clipAreaMargin = new Vector2(35, 17);
                float clipAreaHeight = 40;
                Rect clipAreaRect = new Rect(0, clipAreaMargin.y, position.width, clipAreaHeight);
                // draw frame ruler / line background
                GUI.BeginGroup(clipAreaRect);
                {
                    // draw ruler area
                    Rect clipRulerAreaRect = new Rect(0, 0, clipAreaRect.width, clipAreaMargin.y);
                    GUI.BeginGroup(clipRulerAreaRect, EditorStyles.toolbarButton);
                    {
                        float maxWidth = clipRulerAreaRect.width - 2 * clipAreaMargin.x;

                        // limiter
                        float framePerHalfSecond = 0.5f / s_CurrSystem.TimeStepPerFrame;
                        int limiter = Mathf.RoundToInt(framePerHalfSecond);
                        int segCount = m_SimulateFrameCount / limiter;
                        float widthOffset = maxWidth / segCount;
                        for (int i = 0; i < segCount + 1; i++)
                        {
                            if (i % 2 == 1)
                            {
                                EditorGUI.DrawRect(new Rect(clipAreaMargin.x + widthOffset * i, clipAreaMargin.y - 3, 1, 3), Color.black);
                            }
                            else
                            {
                                GUI.Label(new Rect(clipAreaMargin.x + widthOffset * i, 0, 100, 100), new GUIContent((i / 2).ToString()));
                                EditorGUI.DrawRect(new Rect(clipAreaMargin.x + widthOffset * i, clipAreaMargin.y - 7, 1, 7), Color.black);
                            }
                        }

                        EditorGUI.DrawRect(new Rect(clipAreaMargin.x + 0, 0, 1, clipAreaMargin.y), Color.grey);
                        EditorGUI.DrawRect(new Rect(clipAreaMargin.x + maxWidth, 0, 1, clipAreaMargin.y), Color.grey);
                    }
                    GUI.EndGroup();

                    // draw frame line area
                    Rect clipLineAreaRect = new Rect(0, clipAreaMargin.y, clipAreaRect.width, clipAreaRect.height);
                    GUI.BeginGroup(clipLineAreaRect);
                    {
                        Rect clipLineBgRect = new Rect(0, 0, clipAreaRect.width, clipAreaRect.height);
                        EditorGUI.DrawRect(clipLineBgRect, new Color(0.6f, 0.6f, 0.6f));

                        float maxWidth = clipRulerAreaRect.width - 2 * clipAreaMargin.x;

                        // limiter
                        int limiter = 1;
                        int segCount = m_SimulateFrameCount / limiter;
                        float widthOffset = maxWidth / segCount;

                        for (int i = 0; i < segCount + 1; i++)
                        {
                            EditorGUI.DrawRect(new Rect(clipAreaMargin.x + widthOffset * i, 0, 1.0f, 10), Color.grey);
                        }
                    }
                    GUI.EndGroup();

                }
                GUI.EndGroup();

                Rect clipSliderRect = new Rect(clipAreaMargin.x, clipAreaMargin.y + 20, position.width - 2 * clipAreaMargin.x, position.height - 2 * clipAreaMargin.y);

                // draw frame slider
                CurrentFrame =
                    Mathf.RoundToInt(GUI.HorizontalSlider(clipSliderRect, CurrentFrame, 0.0f, m_SimulateFrameCount,
                        MWBAnimationWindowStyles.keyframeBackground, MWBAnimationWindowStyles.keyframeThumb));
            }
            //GUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
        }
        GUILayout.EndVertical();
    }

    void onEditorApplicationUpdate()
    {
        if (s_CurrSystem)
        {
            if (m_IsPreviewPlaying)
            {
                
                float timeStep = Time.time - m_EditorLastFrameTime;
                if (timeStep > s_CurrSystem.TimeStepPerFrame)
                {
                    int frameCount = (int)(timeStep / s_CurrSystem.TimeStepPerFrame);
                    CurrentFrame += frameCount;

                    if (CurrentFrame > m_SimulateFrameCount)
                        IsPreviewPlaying = false;

                    m_EditorLastFrameTime += s_CurrSystem.TimeStepPerFrame * frameCount;
                }
            }
        }
    }

    void drawProgressBar()
    {
        if (m_IsSystemRunning)
        {
            if (EditorUtility.DisplayCancelableProgressBar(
                        "MWB System",
                        "Simulating Physics",
                        m_SystemProgress
                    )
                )
            {
                s_CurrSystem.Terminate();
            }
        }
        else
        {
            EditorUtility.ClearProgressBar();
            m_IsProgressBarShown = false;
        }
    }

    void onSystemBegin()
    {
        m_SystemProgress = 0.0f;
        m_IsSystemRunning = true;
        m_IsProgressBarShown = true;
    }

    void onSystemUpdateProgress(float progress)
    {
        m_SystemProgress = progress;
    }

    void onSystemComplete()
    {
        m_IsSystemRunning = false;
    }

    void onSystemtTerminate()
    {
        m_IsSystemRunning = false;
    }
}
