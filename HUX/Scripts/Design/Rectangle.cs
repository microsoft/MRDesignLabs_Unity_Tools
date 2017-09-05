//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
using UnityEngine;

namespace MRDL.Design
{
    public class Rectangle : LineBase
    {
        public override int NumPoints
        {
            get
            {
                return 8;
            }
        }

        public override bool Loops
        {
            get
            {
                // Force to loop
                loops = true;
                return loops;
            }
        }

        public Vector2 Dimensions = Vector2.one;
        public float ZOffset;

        /// <summary>
        /// When we get interpolated points we subdivide the square so our sampling has more to work with
        /// </summary>
        /// <param name="normalizedDistance"></param>
        /// <returns></returns>
        protected override Vector3 GetPointInternal(float normalizedDistance)
        {
            if (corners == null || corners.Length != 8)
                corners = new Vector3[8];

            Vector3 offset = Vector3.forward * ZOffset;
            Vector3 top = (Vector3.up * Dimensions.y * 0.5f);
            Vector3 bot = (Vector3.down * Dimensions.y * 0.5f);
            Vector3 left = (Vector3.left * Dimensions.x * 0.5f);
            Vector3 right = (Vector3.right * Dimensions.x * 0.5f);

            SetPointInternal(0, top + left + offset);
            SetPointInternal(1, top + offset);
            SetPointInternal(2, top + right + offset);
            SetPointInternal(3, right + offset);
            SetPointInternal(4, bot + right + offset);
            SetPointInternal(5, bot + offset);
            SetPointInternal(6, bot + left + offset);
            SetPointInternal(7, left + offset);

            return InterpolateArrayPoints(corners, normalizedDistance);
            //return InterpolateCatmullRomPoints(TopLeft, TopRight, BotLeft, BotRight, normalizedDistance);
        }

        protected override void SetPointInternal(int pointIndex, Vector3 point)
        {
            if (corners == null || corners.Length != 8)
                corners = new Vector3[8];

            if (pointIndex <= 7 && pointIndex >= 0)
                corners[pointIndex] = point;
        }

        protected override Vector3 GetPointInternal(int pointIndex)
        {
            if (corners == null || corners.Length != 8)
                corners = new Vector3[8];

            Vector3 offset = Vector3.forward * ZOffset;
            Vector3 top = (Vector3.up * Dimensions.y * 0.5f);
            Vector3 bot = (Vector3.down * Dimensions.y * 0.5f);
            Vector3 left = (Vector3.left * Dimensions.x * 0.5f);
            Vector3 right = (Vector3.right * Dimensions.x * 0.5f);

            SetPointInternal(0, top + left + offset);
            SetPointInternal(1, top + offset);
            SetPointInternal(2, top + right + offset);
            SetPointInternal(3, right + offset);
            SetPointInternal(4, bot + right + offset);
            SetPointInternal(5, bot + offset);
            SetPointInternal(6, bot + left + offset);
            SetPointInternal(7, left + offset);

            if (pointIndex <= 7 && pointIndex >= 0)
                    return corners[pointIndex];

            return Vector3.zero;
        }

        [SerializeField]
        private Vector3[] corners;

        public static Vector3 InterpolateArrayPoints(Vector3[] points, float normalizedDistance) {
            float arrayValueLength = 1f / points.Length;
            int indexA = Mathf.FloorToInt(normalizedDistance * points.Length);
            if (indexA >= points.Length)
                indexA = 0;

            int indexB = indexA + 1;
            if (indexB >= points.Length)
                indexB = 0;

            float blendAmount = (normalizedDistance - (arrayValueLength * indexA)) / arrayValueLength;

            return Vector3.Lerp(points[indexA], points[indexB], blendAmount);
        }

        public static Vector3 InterpolateCatmullRomPoints(Vector3 point1, Vector3 point2, Vector3 point3, Vector3 point4, float normalizedDistance)
        {
            Vector3 p1 = 2f * point2;
            Vector3 p2 = point3 - point1;
            Vector3 p3 = 2f * point1 - 5f * point2 + 4f * point3 - point4;
            Vector3 p4 = -point1 + 3f * point2 - 3f * point3 + point4;
            
            Vector3 point = 0.5f * (p1 + (p2 * normalizedDistance) + (p3 * normalizedDistance * normalizedDistance) + (p4 * normalizedDistance * normalizedDistance * normalizedDistance));

            return point;
        }

    }
}