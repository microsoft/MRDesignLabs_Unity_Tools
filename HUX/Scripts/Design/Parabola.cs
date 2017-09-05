//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
using UnityEngine;

namespace MRDL.Design
{
    public class Parabola : LineBase
    {
        public Vector3 Start = Vector3.zero;
        public Vector3 End = Vector3.forward;
        public Vector3 UpDirection = Vector3.up;
        public float Height = 1f;

        public override int NumPoints
        {
            get
            {
                return 2;
            }
        }

        protected override Vector3 GetPointInternal(int pointIndex)
        {
            switch (pointIndex)
            {
                case 0:
                    return Start;

                case 1:
                    return End;

                default:
                    return Vector3.zero;
            }
        }

        protected override void SetPointInternal(int pointIndex, Vector3 point)
        {
            switch (pointIndex)
            {
                case 0:
                    Start = point;
                    break;

                case 1:
                    End = point;
                    break;

                default:
                    break;
            }
        }

        protected override Vector3 GetPointInternal(float normalizedDistance)
        {
            return GetPointAlongParabola(Start, End, UpDirection, Height, normalizedDistance);
        }

        public static Vector3 GetPointAlongParabola(Vector3 start, Vector3 end, Vector3 up, float height, float normalizedDistance)
        {
            float parabolaTime = normalizedDistance * 2 - 1;
            Vector3 direction = end - start;
            Vector3 grounded = end - new Vector3(start.x, end.y, start.z);
            Vector3 right = Vector3.Cross(direction, grounded);
            Vector3 pos = start + normalizedDistance * direction;
            pos += ((-parabolaTime * parabolaTime + 1) * height) * up.normalized;
            return pos;
        }
    }
}