//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
using System;
using UnityEngine;

namespace MRDL.Design
{
    public class Bezeir : LineBase
    {
        [Serializable]
        public struct PointSet
        {
            public Vector3 Point1;
            public Vector3 Point2;
            public Vector3 Point3;
            public Vector3 Point4;
        }

        public PointSet Points;

        public override int NumPoints
        {
            get
            {
                return 4;
            }
        }

        protected override Vector3 GetPointInternal(int pointIndex)
        {
            switch (pointIndex)
            {
                case 0:
                    return Points.Point1;

                case 1:
                    return Points.Point2;

                case 2:
                    return Points.Point3;

                case 3:
                    return Points.Point4;

                default:
                    return Vector3.zero;
            }
        }

        protected override void SetPointInternal(int pointIndex, Vector3 point)
        {
            switch (pointIndex)
            {
                case 0:
                    Points.Point1 = point;
                    break;

                case 1:
                    Points.Point2 = point;
                    break;

                case 2:
                    Points.Point3 = point;
                    break;

                case 3:
                    Points.Point4 = point;
                    break;

                default:
                    break;
            }
        }

        protected override Vector3 GetPointInternal(float normalizedDistance)
        {
            return InterpolateBezeirPoints(Points.Point1, Points.Point2, Points.Point3, Points.Point4, normalizedDistance);
        }
        
        public static Vector3 InterpolateBezeirPoints (Vector3 point1, Vector3 point2, Vector3 point3, Vector3 point4, float normalizedDistance)
        {
            float invertedDistance = 1f - normalizedDistance;
            return
                invertedDistance * invertedDistance * invertedDistance * point1 +
                3f * invertedDistance * invertedDistance * normalizedDistance * point2 +
                3f * invertedDistance * normalizedDistance * normalizedDistance * point3 +
                normalizedDistance * normalizedDistance * normalizedDistance * point4;
        }
    }
}