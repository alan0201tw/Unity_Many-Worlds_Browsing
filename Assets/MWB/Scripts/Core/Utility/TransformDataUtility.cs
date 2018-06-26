using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TransformDataUtility
{
    public struct TransformData
    {
        public Vector3 localPosition;
        public Quaternion localRotation;
        public Vector3 localScale;

        public TransformData(Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
        {
            this.localPosition = localPosition;
            this.localRotation = localRotation;
            this.localScale = localScale;
        }
    }

    public class TransformDataSegment
    {
        public TransformDataSegment previousSegment;
        public List<TransformData> transformData = new List<TransformData>();

        public TransformDataSegment()
        {
            previousSegment = null;
        }

        public TransformDataSegment(TransformDataSegment previousSegment)
        {
            this.previousSegment = previousSegment;
        }

        public void AddTransformData(TransformData data)
        {
            transformData.Add(data);
        }

        public void AddTransformData(Vector3 localPosition, Quaternion localRotation, Vector3 localScale)
        {
            transformData.Add(new TransformData
            (
            localPosition,
            localRotation,
            localScale)
            );
        }
    }

    public static class TransformNameUtility
    {
        public static string GetTransformPathName(Transform rootTransform, Transform targetTransform)
        {
            string returnName = targetTransform.name;
            Transform tempObj = targetTransform;

            // it is the root transform
            if (tempObj == rootTransform)
                return "";

            while (tempObj.parent != rootTransform)
            {
                returnName = tempObj.parent.name + "/" + returnName;
                tempObj = tempObj.parent;
            }

            return returnName;
        }
    }

}