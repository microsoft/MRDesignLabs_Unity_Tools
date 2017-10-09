//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
using HUX.Utility;
using UnityEngine;

namespace MRDL.Design
{
    public class DistorterSimplex : Distorter
    {
        public float ScaleMultiplier = 10f;
        public float SpeedMultiplier = 1f;
        public float StrengthMultiplier = 0.5f;
        public Vector3 AxisStrength = Vector3.one;
        public Vector3 AxisSpeed = Vector3.one;
        public Vector3 AxisOffset = Vector3.zero;

        public override Vector3 DistortPoint(Vector3 point, float strength)
        {
            point.x = (float)(point.x + (noise.Evaluate((point.x + AxisOffset.x) * ScaleMultiplier, Time.unscaledTime * AxisSpeed.x)) * AxisStrength.x * StrengthMultiplier);
            point.y = (float)(point.y + (noise.Evaluate((point.y + AxisOffset.y) * ScaleMultiplier, Time.unscaledTime * AxisSpeed.y)) * AxisStrength.y * StrengthMultiplier);
            point.z = (float)(point.z + (noise.Evaluate((point.z + AxisOffset.z) * ScaleMultiplier, Time.unscaledTime * AxisSpeed.z)) * AxisStrength.z * StrengthMultiplier);
            return point;
        }

        private FastSimplexNoise noise = new FastSimplexNoise();
    }
}