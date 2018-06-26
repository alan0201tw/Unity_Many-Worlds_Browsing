using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MWB_System))]
[System.Serializable]
public class MWB_SystemEditor : Editor 
{
    private bool m_IsInternalFolderShown = false;

    private List<Collider> m_PhysicObjs;

    public void OnEnable()
    {
        var cols = (target as MWB_System).transform.GetComponentsInChildren<Collider>();
        m_PhysicObjs = new List<Collider>();
        foreach (var col in cols)
        {
            if (col.GetComponent<Rigidbody>() != null)
            {
                m_PhysicObjs.Add(col);
            }
        }
    }

    public override void OnInspectorGUI()
    {
        MWB_System system = (MWB_System)target;

        GUILayout.BeginVertical();
        {
            //system.TimeStepPerFrame = EditorGUILayout.FloatField(new GUIContent("TimeStepPerFrame"), system.TimeStepPerFrame);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("SimulateFrameCount"), new GUIContent("FrameCount"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("WorldForkedOnCollision"), new GUIContent("WorldForkedOnCollision"));

            //system.SimulateFrameCount = EditorGUILayout.IntField(new GUIContent("FrameCount"), system.SimulateFrameCount);
            //system.WorldForkedOnCollision = EditorGUILayout.IntField(new GUIContent("WorldForkedOnCollision"), system.WorldForkedOnCollision);

            m_IsInternalFolderShown =
                EditorGUILayout.Foldout(m_IsInternalFolderShown, "Physics Setup", true);

            EditorGUI.indentLevel++;
            if (m_IsInternalFolderShown)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_CollisionThresholdCoef"), new GUIContent("Threshold Coefficient"));

                /*serializedObject.FindProperty("m_CollisionThresholdCoef").floatValue = 
                    EditorGUILayout.FloatField(new GUIContent("Threshold Coefficient"), system.CollisionThresholdCoef);*/
            }
            EditorGUI.indentLevel--;

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("You can only simulate in play mode.", MessageType.Warning);
            }

            GUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();

                if (!Application.isPlaying)
                {
                    EditorGUI.BeginDisabledGroup(true);
                }

                if (GUILayout.Button(new GUIContent("Simulate"), GUILayout.Width(150)))
                {
                    system.Simulate();
                }

                if (!Application.isPlaying)
                {
                    EditorGUI.EndDisabledGroup();
                }

                GUILayout.FlexibleSpace();
            }
            GUILayout.EndHorizontal();

            var bgStyle = new GUIStyle();
            bgStyle.normal.background = MakeTex(1, 1, new Color(0.8f, 0.8f, 0.8f));

            GUILayout.BeginVertical(bgStyle);
            {
                drawMWBObjPad();
            }
            GUILayout.EndVertical();
        }
        GUILayout.EndVertical();

        // Apply changes to the serializedProperty - always do this at the end of OnInspectorGUI.
        serializedObject.ApplyModifiedProperties();
    }

    void drawMWBObjPad()
    {
        int index = 0;
        foreach(var col in m_PhysicObjs)
        {
            Color bgColor = (index % 2 == 0) ? new Color(0.8f, 0.8f, 0.8f) : new Color(0.9f, 0.9f, 0.9f);

            var bgStyle = new GUIStyle();
            bgStyle.normal.background = MakeTex(1, 1, bgColor);

            GUILayout.BeginHorizontal(bgStyle);
            {
                var mwbObj = col.GetComponent<MWB_Object>();
                string name = col.name;
                bool hasMwbObj = mwbObj != null;
                bool result = hasMwbObj;

                GUIStyle toggleStyle = new GUIStyle("Toggle");
                result = GUILayout.Toggle(result, "", toggleStyle);

                if (result != hasMwbObj)
                {
                    if (result)
                    {
                        col.gameObject.AddComponent<MWB_Object>();
                    }
                    else
                    {
                        if (mwbObj)
                        {
                            MonoBehaviour.DestroyImmediate(mwbObj);
                        }
                    }
                }

                // draw selection label
                GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
                labelStyle.normal.textColor = Color.black;
                labelStyle.fontSize = 9;
                labelStyle.onFocused = labelStyle.normal;
                labelStyle.alignment = TextAnchor.MiddleLeft;
                GUILayout.Label(name, labelStyle);
            }
            GUILayout.EndHorizontal();

            index++;
        }
    }

    private Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];

        for (int i = 0; i < pix.Length; i++)
            pix[i] = col;

        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();

        return result;
    }
}
