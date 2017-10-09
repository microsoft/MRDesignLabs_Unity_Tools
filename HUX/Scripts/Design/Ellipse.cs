//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
using UnityEngine;

namespace MRDL.Design
{
    public class Ellipse : LineBase
    {
        public int Resolution = 36;
        public Vector2 Radius = new Vector2(1f, 1f);

        public override int NumPoints
        {
            get
            {
                Resolution = Mathf.Clamp(Resolution, 0, 2048);
                return Resolution;
            }
        }

        protected override Vector3 GetPointInternal(float normalizedDistance)
        {
            return GetEllipsePoint(Radius.x, Radius.y, normalizedDistance * 2f * Mathf.PI);
        }

        protected override Vector3 GetPointInternal(int pointIndex)
        {
            float angle = ((float)pointIndex / Resolution) * 2f * Mathf.PI;
            return GetEllipsePoint(Radius.x, Radius.y, angle);
        }

        protected override void SetPointInternal(int pointIndex, Vector3 point)
        {
            // Does nothing for an ellipse
            return;
        }

        public static Vector3 GetEllipsePoint (float radiusX, float radiusY, float angle)
        {
           return new Vector3(radiusX * Mathf.Cos(angle), radiusY * Mathf.Sin(angle), 0.0f);
        }
    }
}