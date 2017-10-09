//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace MRDL.Design
{
    public class LineMeshes : LineRenderer
    {
        readonly string InvisibleShaderName = "HUX/InvisibleShader";

        public enum RotationModeEnum
        {
            None,                           // Don't rotate
            Velocity,                       // Use velocity
            Vector,                         // Use rotation vector (first array value)
            VectorBlend,                    // Blend rotation vectors (all array values)
            VectorAndVelocity,              // Blend first rotation vector with velocity
            VectorBlendAndVelocity,         // Blend all rotation vectors with velocity
        }

        public Mesh LineMesh;

        public Material LineMaterial;

        public Vector3 UpVector = Vector3.up;

        public Vector3[] RotationVectors = new Vector3[] { Vector3.up };
        [Range(0f, 1f)]
        public float VelocityBlend = 0.5f;
        
        public string ColorProp = "_Color";

        public RotationModeEnum RotationMode = RotationModeEnum.Velocity;
        [Range(0.001f, 0.2f)]
        public float VelocitySearchRange = 0.01f;

        protected override void OnEnable()
        {
            base.OnEnable();

            if (linePropertyBlock == null)
            {
                LineMaterial.enableInstancing = true;
                linePropertyBlock = new MaterialPropertyBlock();
                colorID = Shader.PropertyToID(ColorProp);
            }

            if (onWillRenderHelper == null)
            {   // OnWillRenderObject won't be called unless there's a renderer attached
                // and if the renderer's bounds are visbile.
                // So we create a simple 1-triangle mesh to ensure it's always called.
                // Hackey, but it works.
                onWillRenderHelper = gameObject.AddComponent<MeshRenderer>();
                onWillRenderHelper.receiveShadows = false;
                onWillRenderHelper.shadowCastingMode = ShadowCastingMode.Off;
                onWillRenderHelper.lightProbeUsage = LightProbeUsage.Off;
                onWillRenderHelper.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;

                onWillRenderMesh = new Mesh();
                onWillRenderMesh.vertices = meshVertices;
                onWillRenderMesh.triangles = new int[] { 0, 1, 2 };

                MeshFilter helperMeshFilter = gameObject.AddComponent<MeshFilter>();
                helperMeshFilter.sharedMesh = onWillRenderMesh;

                // Create an 'invisible' material so the mesh doesn't show up pink
                onWillRenderMat = new Material(Shader.Find(InvisibleShaderName));
                onWillRenderHelper.sharedMaterial = onWillRenderMat;
            }
        }

        private void Update()
        {
            executeCommandBuffer = false;

            if (source.enabled)
            {
                if (meshTransforms == null || meshTransforms.Length != NumLineSteps)
                {
                    meshTransforms = new Matrix4x4[NumLineSteps];
                }

                if (colorValues == null || colorValues.Length != NumLineSteps)
                {
                    colorValues = new Vector4[NumLineSteps];
                    linePropertyBlock.Clear();
                }

                for (int i = 0; i < NumLineSteps; i++)
                {
                    float normalizedDistance = (1f / (NumLineSteps - 1)) * i;
                    colorValues[i] = GetColor(normalizedDistance);
                    meshTransforms[i] = Matrix4x4.TRS(source.GetPoint(normalizedDistance), GetRotation(normalizedDistance), Vector3.one * GetWidth(normalizedDistance));
                }

                linePropertyBlock.SetVectorArray(colorID, colorValues);

                executeCommandBuffer = true;
            }
        }

        private void OnDisable()
        {
            foreach (KeyValuePair<Camera, CommandBuffer> cam in cameras)
            {
                if (cam.Key != null)
                {
                    cam.Key.RemoveCommandBuffer(CameraEvent.AfterForwardOpaque, cam.Value);
                }
            }
            cameras.Clear();
        }

        private void OnWillRenderObject()
        {
            Camera cam = Camera.current;
            CommandBuffer buffer = null;
            if (!cameras.TryGetValue(cam, out buffer))
            {
                buffer = new CommandBuffer();
                buffer.name = "Line Mesh Renderer " + cam.name;
                cam.AddCommandBuffer(CameraEvent.AfterForwardOpaque, buffer);
                cameras.Add(cam, buffer);
            }

            buffer.Clear();
            if (executeCommandBuffer)
            {
                buffer.DrawMeshInstanced(LineMesh, 0, LineMaterial, 0, meshTransforms, meshTransforms.Length, linePropertyBlock);
            }
        }

        protected override Quaternion GetRotation(float normalizedDistance)
        {
            normalizedDistance = (Mathf.Repeat(normalizedDistance + RotationOffset, 1f));

            switch (RotationMode)
            {
                case RotationModeEnum.None:
                default:
                    return Quaternion.identity;

                case RotationModeEnum.Vector:
                    if (RotationVectors.Length == 0)
                        return Quaternion.identity;

                    return Quaternion.LookRotation(RotationVectors[0], UpVector);

                case RotationModeEnum.VectorBlend:
                    return GetVectorBlendRotation(normalizedDistance);

                case RotationModeEnum.Velocity:
                    return GetVelocityRotation(normalizedDistance);

                case RotationModeEnum.VectorBlendAndVelocity:
                    return Quaternion.Lerp(
                        GetVectorBlendRotation(normalizedDistance),
                        GetVelocityRotation(normalizedDistance),
                        VelocityBlend);

                case RotationModeEnum.VectorAndVelocity:
                    return Quaternion.Lerp(
                        Quaternion.LookRotation(RotationVectors[0], UpVector),
                        GetVelocityRotation(normalizedDistance),
                        VelocityBlend);

            }
        }

        private void LateUpdate()
        {
            // Update our helper mesh so OnWillRenderObject will be called
            meshVertices[0] = transform.InverseTransformPoint (source.GetPoint(0.0f));// - transform.position;
            meshVertices[1] = transform.InverseTransformPoint (source.GetPoint(0.5f));// - transform.position;
            meshVertices[2] = transform.InverseTransformPoint (source.GetPoint(1.0f));// - transform.position;
            onWillRenderMesh.vertices = meshVertices;
            onWillRenderMesh.RecalculateBounds();
        }

        private Quaternion GetVelocityRotation (float normalizedDistance)
        {
            float prevNormalizedDistance = Mathf.Clamp01(normalizedDistance - VelocitySearchRange);
            Vector3 currentPos = source.GetPoint(normalizedDistance);
            Vector3 prevPos = source.GetPoint(prevNormalizedDistance);
            Vector3 velocity = (currentPos - prevPos).normalized;
            if (velocity == Vector3.zero)
                return Quaternion.identity;

            return Quaternion.LookRotation(velocity, UpVector);
        }

        private Quaternion GetVectorBlendRotation (float normalizedDistance)
        {
            if (RotationVectors.Length == 0)
                return Quaternion.identity;
            else if (RotationVectors.Length == 1)
                return Quaternion.LookRotation(RotationVectors[0], UpVector);

            float arrayValueLength = 1f / RotationVectors.Length;
            int indexA = Mathf.FloorToInt(normalizedDistance * RotationVectors.Length);
            int indexB = indexA + 1;
            if (indexB >= RotationVectors.Length)
                indexB = 0;

            float blendAmount = (normalizedDistance - (arrayValueLength * indexA)) / arrayValueLength;
            return Quaternion.Lerp(
                Quaternion.LookRotation(RotationVectors[indexA], UpVector),
                Quaternion.LookRotation(RotationVectors[indexB], UpVector),
                blendAmount);
        }

        // Command buffer properties
        private MaterialPropertyBlock linePropertyBlock;
        private int colorID;
        private Matrix4x4[] meshTransforms;
        private Vector4[] colorValues;
        private bool executeCommandBuffer = false;
        private Dictionary<Camera, CommandBuffer> cameras = new Dictionary<Camera, CommandBuffer>();
        // OnWillRenderObject helpers
        private MeshRenderer onWillRenderHelper;
        private Mesh onWillRenderMesh;
        private Material onWillRenderMat;
        private Vector3[] meshVertices = new Vector3[3];
    }
}