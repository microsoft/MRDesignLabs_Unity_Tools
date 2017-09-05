//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
using System.Collections.Generic;
using UnityEngine;

namespace MRDL.Design
{
    public abstract class LineBase : MonoBehaviour
    {
        public enum SpaceEnum
        {
            Global,
            Local,
        }

        public SpaceEnum Space = SpaceEnum.Global;
        [Range(0f, 1f)]
        public float LineStartClamp = 0f;
        [Range(0f, 1f)]
        public float LineEndClamp = 1f;

        public virtual bool Loops
        {
            get
            {
                return loops;
            }
        }

        public AnimationCurve DistortionStrength = AnimationCurve.Linear(0f, 1f, 1f, 1f);

        // Abstract
        public abstract int NumPoints { get; }

        protected abstract void SetPointInternal(int pointIndex, Vector3 point);

        protected abstract Vector3 GetPointInternal(float normalizedDistance);

        protected abstract Vector3 GetPointInternal(int pointIndex);

        // Convenience
        public void SetFirstPoint (Vector3 point)
        {
            SetPoint(0, point);
        }

        public void SetLastPoint (Vector3 point)
        {
            SetPoint(NumPoints - 1, point);
        }

        // Public
        public Vector3 GetPoint(float normalizedDistance)
        {
            if (distorters == null)
                FindDistorters();

            normalizedDistance = ClampedDistance(normalizedDistance);
            switch (Space)
            {
                case SpaceEnum.Local:
                    return transform.TransformPoint(DistortPoint(GetPointInternal(normalizedDistance), normalizedDistance));

                case SpaceEnum.Global:
                default:
                    return DistortPoint(GetPointInternal(normalizedDistance), normalizedDistance);
            }
        }

        public Vector3 GetPoint (int pointIndex)
        {
            if (pointIndex < 0 || pointIndex >= NumPoints)
                throw new System.IndexOutOfRangeException();

            switch (Space)
            {
                case SpaceEnum.Local:
                    return transform.TransformPoint(GetPointInternal(pointIndex));

                case SpaceEnum.Global:
                default:
                   return GetPointInternal(pointIndex);
            }
        }

        public void SetPoint (int pointIndex, Vector3 point)
        {
            if (pointIndex < 0 || pointIndex >= NumPoints)
                throw new System.IndexOutOfRangeException();

            switch (Space)
            {
                case SpaceEnum.Local:
                    SetPointInternal(pointIndex, transform.InverseTransformPoint(point));
                    break;

                case SpaceEnum.Global:
                default:
                    SetPointInternal(pointIndex, point);
                    break;
            }
        }

        //public Vector3 GetDirection(float normalizedDistance)

        //protected abstract Vector3 GetDirectionInternal (float normalizedDistance);

        // Private
        private Vector3 DistortPoint (Vector3 point, float normalizedDistance)
        {
            float strength = DistortionStrength.Evaluate(normalizedDistance);
            for (int i = 0; i < distorters.Length; i++)
            {
                // Components may be added or removed
                if (distorters[i] != null)
                {
                    point = distorters[i].DistortPoint(point, strength);
                }
            }
            return point;
        }

        private float ClampedDistance(float normalizedDistance)
        {
            return Mathf.Lerp(LineStartClamp, LineEndClamp, Mathf.Clamp01(normalizedDistance));
        }

        private void FindDistorters()
        {
            // Get all of the distorters attached to this gameobject
            // Sort by distort order
            Component[] distorterComponents = gameObject.GetComponents(typeof(IDistorter));
            List<IDistorter> distorterList = new List<IDistorter>();
            for (int i = 0; i < distorterComponents.Length; i++)
            {
                distorterList.Add((IDistorter)distorterComponents[i].GetComponent(typeof(IDistorter)));
            }
            distorterList.Sort(delegate (IDistorter d1, IDistorter d2)
            {
                return d1.DistortOrder.CompareTo(d2.DistortOrder);
            });
            distorters = distorterList.ToArray();
        }

        protected virtual void OnEnable()
        {
            // Reset this every time we're enabled
            // This will help to ensure that our distorters list is updated
            distorters = null;
        }
        
        private IDistorter[] distorters;

        [SerializeField]
        protected bool loops = false;
    }
}