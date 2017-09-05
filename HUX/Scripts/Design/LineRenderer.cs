//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
using UnityEngine;

namespace MRDL.Design
{
    public abstract class LineRenderer : MonoBehaviour
    {
        public enum StepModeEnum
        {
            Interpolated,   // Draw points based on NumLineSteps
            FromSource,     // Draw only the points available in the source - use this for hard edges
        }

        /*public enum InterpolationModeEnum
        {
            ManualStepNum,  // Specify the number of interpolation steps manually
            FromLength,     // Create steps based on total length of line
        }*/

        public Gradient LineColor;
        public AnimationCurve LineWidth = AnimationCurve.Linear(0f, 1f, 1f, 1f);
        [Range(0f, 1f)]
        public float WidthMultiplier = 0.025f;
        [Range(0, 100)]
        public int NumLineSteps = 10;
        [Range(0f, 10f)]
        public float ColorOffset = 0f;
        [Range(0f, 10f)]
        public float WidthOffset = 0f;
        [Range(0f, 10f)]
        public float RotationOffset = 0f;
        [Range(0.001f,1f)]
        public float StepLength = 0.01f;

        public StepModeEnum StepMode = StepModeEnum.Interpolated;

        //public InterpolationModeEnum InterpolationMode = InterpolationModeEnum.ManualStepNum;

        public virtual LineBase Target
        {
            get
            {
                return source;
            }
            set
            {
                source = value;
            }
        }

        protected virtual Color GetColor (float normalizedDistance)
        {
            if (LineColor == null)
                LineColor = new Gradient();

            return LineColor.Evaluate(Mathf.Repeat(normalizedDistance + ColorOffset, 1f));
        }

        protected virtual float GetWidth(float normalizedDistance)
        {
            if (LineWidth == null)
                LineWidth = AnimationCurve.Linear(0f, 1f, 1f, 1f);

            return LineWidth.Evaluate(Mathf.Repeat(normalizedDistance + WidthOffset, 1f)) * WidthMultiplier;
        }

        protected virtual Quaternion GetRotation(float normalizedDistance)
        {
            return Quaternion.identity;
        }

        protected virtual void OnEnable()
        {
            if (source == null)
                source = gameObject.GetComponent<LineBase>();
        }

        #if UNITY_EDITOR
        protected virtual void OnDrawGizmos()
        {
            if (Application.isPlaying)
                return;
            
            if (source == null)
                source = gameObject.GetComponent<LineBase>();
            
            if (source != null && source.enabled)
            {

                // This is essentially how you'll want to render lines
                Vector3 firstPos = Vector3.zero;
                Vector3 lastPos = Vector3.zero;
                Vector3 currentPos = Vector3.zero;
                Color gColor = GetColor(0f);
                gColor.a = 0.5f;
                Gizmos.color = gColor;

                switch (StepMode)
                {
                    case StepModeEnum.FromSource:
                        firstPos = source.GetPoint(0);
                        lastPos = firstPos;
                        gColor = GetColor(0);
                        gColor.a = 0.5f;
                        Gizmos.color = gColor;
                        Gizmos.DrawSphere(firstPos, GetWidth(0) / 2);
                        for (int i = 1; i < source.NumPoints; i++)
                        {
                            float normalizedDistance = (1f / (source.NumPoints - 1)) * i;
                            currentPos = source.GetPoint(i);
                            gColor = GetColor(normalizedDistance);
                            gColor.a = 0.5f;
                            Gizmos.color = gColor;
                            Gizmos.DrawLine(lastPos, currentPos);
                            Gizmos.DrawSphere(currentPos, GetWidth(normalizedDistance) / 2);
                            lastPos = currentPos;
                        }
                        if (source.Loops)
                        {
                            Gizmos.DrawLine(lastPos, firstPos);
                        }
                        break;

                    case StepModeEnum.Interpolated:
                        firstPos = source.GetPoint(0f);
                        lastPos = firstPos;
                        gColor = GetColor(0);
                        gColor.a = 0.5f;
                        Gizmos.color = gColor;
                        Gizmos.DrawSphere(currentPos, GetWidth(0) / 2);
                        for (int i = 1; i < NumLineSteps; i++)
                        {
                            float normalizedDistance = (1f / (NumLineSteps - 1)) * i;
                            currentPos = source.GetPoint(normalizedDistance);
                            gColor = GetColor(normalizedDistance);
                            gColor.a = 0.5f;
                            Gizmos.color = gColor;
                            Gizmos.DrawLine(lastPos, currentPos);
                            Gizmos.DrawSphere(currentPos, GetWidth(normalizedDistance) / 2);
                            lastPos = currentPos;
                        }
                        if (source.Loops)
                        {
                            Gizmos.DrawLine(lastPos, firstPos);
                        }
                        break;
                }
            }
        }
        #endif

        [SerializeField]
        protected LineBase source;
    }
}