using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MWBTest
{
    public static class Utils
    {
        static Texture2D _whiteTexture;
        public static Texture2D WhiteTexture
        {
            get
            {
                if (_whiteTexture == null)
                {
                    _whiteTexture = new Texture2D(1, 1);
                    _whiteTexture.SetPixel(0, 0, Color.white);
                    _whiteTexture.Apply();
                }

                return _whiteTexture;
            }
        }

        public static void DrawScreenRectBorder(Rect rect, float thickness, Color color)
        {
            // Top
            Utils.DrawScreenRect(new Rect(rect.xMin, rect.yMin, rect.width, thickness), color);
            // Left
            Utils.DrawScreenRect(new Rect(rect.xMin, rect.yMin, thickness, rect.height), color);
            // Right
            Utils.DrawScreenRect(new Rect(rect.xMax - thickness, rect.yMin, thickness, rect.height), color);
            // Bottom
            Utils.DrawScreenRect(new Rect(rect.xMin, rect.yMax - thickness, rect.width, thickness), color);
        }

        public static void DrawScreenRect(Rect rect, Color color)
        {
            GUI.color = color;
            GUI.DrawTexture(rect, WhiteTexture);
            GUI.color = Color.white;
        }

        public static Rect GetScreenRect(Vector3 screenPos0, Vector3 screenPos1)
        {
            // Move origin from bottom left to top left
            screenPos0.y = Screen.height - screenPos0.y;
            screenPos1.y = Screen.height - screenPos1.y;
            // Calculate corners
            var topLeft = Vector3.Min(screenPos0, screenPos1);
            var bottomRight = Vector3.Max(screenPos0, screenPos1);
            // Create Rect
            return Rect.MinMaxRect(topLeft.x, topLeft.y, bottomRight.x, bottomRight.y);
        }

        public static Rect GetScreenRectYFlip(Vector3 screenPos0, Vector3 screenPos1)
        {
            // Calculate corners
            var topLeft = Vector3.Min(screenPos0, screenPos1);
            var bottomRight = Vector3.Max(screenPos0, screenPos1);
            // Create Rect
            return Rect.MinMaxRect(topLeft.x, topLeft.y, bottomRight.x, bottomRight.y);
        }

        public static Bounds GetViewportBounds(Camera camera, Vector3 screenPosition1, Vector3 screenPosition2)
        {
            var v1 = camera.ScreenToViewportPoint(screenPosition1);
            var v2 = camera.ScreenToViewportPoint(screenPosition2);
            var min = Vector3.Min(v1, v2);
            var max = Vector3.Max(v1, v2);
            min.z = camera.nearClipPlane;
            max.z = camera.farClipPlane;

            var bounds = new Bounds();
            bounds.SetMinMax(min, max);
            return bounds;
        }

        public static Vector3 ManualWorldToScreenPoint(Vector3 wp, Matrix4x4 world2Screen, int pixelWith, int pixelHeight)
        {
            // calculate view-projection matrix
            Matrix4x4 mat = world2Screen;

            // multiply world point by VP matrix
            Vector4 temp = mat * new Vector4(wp.x, wp.y, wp.z, 1f);

            if (temp.w == 0f)
            {
                // point is exactly on camera focus point, screen point is undefined
                // unity handles this by returning 0,0,0
                return Vector3.zero;
            }
            else
            {
                // convert x and y from clip space to window coordinates
                temp.x = (temp.x / temp.w + 1f) * .5f * pixelWith;
                temp.y = (temp.y / temp.w + 1f) * .5f * pixelHeight;
                return new Vector3(temp.x, temp.y, wp.z);
            }
        }
    }

}