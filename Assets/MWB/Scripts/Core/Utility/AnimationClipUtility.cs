using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using TransformDataUtility;

namespace AnimationClipUtility
{
    public static class ClipExportUtility
    {
        public static void ExportAnimationClip(AnimationClip animationClip, string exportFileName)
        {
            // if file name is not end with proper sub-file name
            if (!exportFileName.EndsWith(".anim"))
            {
                exportFileName += ".anim";
            }

            animationClip.EnsureQuaternionContinuity();
            AssetDatabase.CreateAsset(animationClip, exportFileName);
        }
    }

    public static class MWBDummyClipUtility
    {
        public static AnimationClip GenerateClipFromDummyList(MWB_DummyObjectList dummyList)
        {
            AnimationClip clip = new AnimationClip();

            foreach (MWB_DummyObject dummy in dummyList.MWB_DummyObjects)
            {
                // stack from dummy itself to its root forked source, and reverse create clips
                Stack<TransformDataSegment> dataSegmentStack = new Stack<TransformDataSegment>();
                TransformDataSegment currentDataSegment = dummy.transformDataSegment;

                while (currentDataSegment != null)
                {
                    dataSegmentStack.Push(currentDataSegment);
                    // travese upward in fork tree
                    currentDataSegment = currentDataSegment.previousSegment;
                }

                // for each recorded Transform data, the time interval is Time.fixedDeltaTime
                float currentRelativeTime = 0f;
                // create curves base on the dummys in the stack

                AnimationCurve localPositionXCurve = new AnimationCurve();
                AnimationCurve localPositionYCurve = new AnimationCurve();
                AnimationCurve localPositionZCurve = new AnimationCurve();

                AnimationCurve localRotationXCurve = new AnimationCurve();
                AnimationCurve localRotationYCurve = new AnimationCurve();
                AnimationCurve localRotationZCurve = new AnimationCurve();
                AnimationCurve localRotationWCurve = new AnimationCurve();

                AnimationCurve localScaleXCurve = new AnimationCurve();
                AnimationCurve localScaleYCurve = new AnimationCurve();
                AnimationCurve localScaleZCurve = new AnimationCurve();

                while (dataSegmentStack.Count > 0)
                {
                    currentDataSegment = dataSegmentStack.Pop();
                    if (currentDataSegment == null)
                        break;

                    for (int i = 0; i < currentDataSegment.transformData.Count; i++)
                    {
                        localPositionXCurve.AddKey(currentRelativeTime, currentDataSegment.transformData[i].localPosition.x);
                        localPositionYCurve.AddKey(currentRelativeTime, currentDataSegment.transformData[i].localPosition.y);
                        localPositionZCurve.AddKey(currentRelativeTime, currentDataSegment.transformData[i].localPosition.z);

                        localRotationXCurve.AddKey(currentRelativeTime, currentDataSegment.transformData[i].localRotation.x);
                        localRotationYCurve.AddKey(currentRelativeTime, currentDataSegment.transformData[i].localRotation.y);
                        localRotationZCurve.AddKey(currentRelativeTime, currentDataSegment.transformData[i].localRotation.z);
                        localRotationWCurve.AddKey(currentRelativeTime, currentDataSegment.transformData[i].localRotation.w);

                        localScaleXCurve.AddKey(currentRelativeTime, currentDataSegment.transformData[i].localScale.x);
                        localScaleYCurve.AddKey(currentRelativeTime, currentDataSegment.transformData[i].localScale.y);
                        localScaleZCurve.AddKey(currentRelativeTime, currentDataSegment.transformData[i].localScale.z);

                        currentRelativeTime += Time.fixedDeltaTime;
                    }
                }
                //
                //Debug.Log("max frame = " + currentRelativeTime / Time.fixedDeltaTime);

                // set curve into the animation clip
                clip.SetCurve(dummy.objectSource.hierachyName, typeof(Transform), "localPosition.x", localPositionXCurve);
                clip.SetCurve(dummy.objectSource.hierachyName, typeof(Transform), "localPosition.y", localPositionYCurve);
                clip.SetCurve(dummy.objectSource.hierachyName, typeof(Transform), "localPosition.z", localPositionZCurve);

                clip.SetCurve(dummy.objectSource.hierachyName, typeof(Transform), "localRotation.x", localRotationXCurve);
                clip.SetCurve(dummy.objectSource.hierachyName, typeof(Transform), "localRotation.y", localRotationYCurve);
                clip.SetCurve(dummy.objectSource.hierachyName, typeof(Transform), "localRotation.z", localRotationZCurve);
                clip.SetCurve(dummy.objectSource.hierachyName, typeof(Transform), "localRotation.w", localRotationWCurve);

                clip.SetCurve(dummy.objectSource.hierachyName, typeof(Transform), "localScale.x", localScaleXCurve);
                clip.SetCurve(dummy.objectSource.hierachyName, typeof(Transform), "localScale.y", localScaleYCurve);
                clip.SetCurve(dummy.objectSource.hierachyName, typeof(Transform), "localScale.z", localScaleZCurve);
            }
            return clip;
        }
    }

}