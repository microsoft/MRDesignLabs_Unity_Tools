//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
using UnityEngine;

namespace MRDL.Design
{
    public interface IDistorter
    {
        Vector3 DistortPoint(Vector3 point, float strength);
        int DistortOrder { get; set; }
    }

    public abstract class Distorter : MonoBehaviour, IDistorter
    {
        public abstract Vector3 DistortPoint(Vector3 point, float strength);

        public int DistortOrder
        {
            get { return distortOrder; }
            set { distortOrder = value; }
        }

        [SerializeField]
        protected int distortOrder = 0;
    }
}