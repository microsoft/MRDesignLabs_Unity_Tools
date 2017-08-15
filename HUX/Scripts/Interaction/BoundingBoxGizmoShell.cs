//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
using HUX.Buttons;
using System.Collections.Generic;
using UnityEngine;

namespace HUX.Interaction
{
    /// <summary>
    /// Draws a bounding box gizmo in the style of the hololens shell
    /// </summary>
    [ExecuteInEditMode]
    public class BoundingBoxGizmoShell : BoundingBoxGizmo
    {
        #region public

        /// <summary>
        /// If true, bounding box edges will display when active
        /// To more closely mimic shell behavior, set this to false
        /// </summary>
        public bool DisplayEdgesWhenSelected = true;

        /// <summary>
        /// Whether to clamp handle size between HandleScaleMin / Max
        /// This may result in bunched-up handles so use with caution
        /// </summary>
        public bool ClampHandleScale = false;

        /// <summary>
        /// User-defined min size for handle
        /// Useful when using the bounding box with extremely small objects
        /// This value is ignored if ClampHandleSize is false
        /// </summary>
        public float HandleScaleMin = 0.1f;

        /// <summary>
        /// User-defined min size for handle
        /// Useful when using the bounding box with extremely large objects
        /// This value is ignored if ClampHandleSize is false
        /// </summary>
        public float HandleScaleMax = 2.0f;
        
        /// <summary>
        /// Mesh used to draw scale handles
        /// </summary>
        public Mesh BoxMesh;
        
        /// <summary>
        /// Mesh used to draw rotate handles
        /// </summary>
        public Mesh SphereMesh;

        /// <summary>
        /// Material used for drawing handles (must have _Color property)
        /// </summary>
        public Material HandleMaterial;

        /// <summary>
        /// Color of active handles
        /// </summary>
        public Color ActiveColor = Color.blue;

        /// <summary>
        /// Color of inactive handles
        /// </summary>
        public Color InactiveColor = Color.gray;

        /// <summary>
        /// Color of the handle being manipulated by player
        /// </summary>
        public Color TargetColor = Color.red;

        #endregion

        #region private

        [SerializeField]
        private Renderer edgeRenderer;

        [SerializeField]
        private int activeHandleIndex = -1;

        private Vector3[] handlePositions;
        private Material edgeMaterial;
        private List<Matrix4x4> cubeHandleMatrixes = new List<Matrix4x4>();
        private List<Matrix4x4> sphereHandleMatrixes = new List<Matrix4x4>();
        private Bounds localBounds = new Bounds();
        private MaterialPropertyBlock propertyBlock;
        private int colorID;

        #endregion

        #region initialization

        void OnEnable()
        {
            edgeMaterial = new Material(HandleMaterial);
            edgeRenderer.material = edgeMaterial;
            HandleMaterial.enableInstancing = true;
            propertyBlock = new MaterialPropertyBlock();
            colorID = Shader.PropertyToID("_EmissionColor");
        }

        void OnDisable()
        {
            if (Application.isPlaying)
            {
                GameObject.Destroy(edgeMaterial);
            }
            else
            {
                GameObject.DestroyImmediate(edgeMaterial);
            }
        }

        #endregion

        #region handle drawing
        
        protected override void DrawGizmoObjects()
        {
            if (boundingBox.Target == null)
            {
                edgeRenderer.enabled = false;
                return;
            }

            // Get our manipulate bb
            BoundingBoxManipulate manipulate = boundingBox.GetComponent<BoundingBoxManipulate>();

            // Reset our scale - only scaleTransform can be changed
            transform.localScale = Vector3.one;

            // Get the positions of our handles
            localBounds.size = scaleTransform.localScale;
            localBounds.center = Vector3.zero;
            cubeHandleMatrixes.Clear();
            sphereHandleMatrixes.Clear();
            if (manipulate.FlattenedAxis == BoundingBox.FlattenModeEnum.DoNotFlatten)
            {
                localBounds.GetCornerAndMidPointPositions(transform, ref handlePositions);
            }
            else
            {
                switch (manipulate.FlattenedAxis)
                {
                    case BoundingBox.FlattenModeEnum.FlattenX:
                        localBounds.GetCornerAndMidPointPositions2D(transform, ref handlePositions, BoundsExtentions.Axis.X);
                        break;

                    case BoundingBox.FlattenModeEnum.FlattenY:
                        localBounds.GetCornerAndMidPointPositions2D(transform, ref handlePositions, BoundsExtentions.Axis.Y);
                        break;

                    case BoundingBox.FlattenModeEnum.FlattenZ:
                        localBounds.GetCornerAndMidPointPositions2D(transform, ref handlePositions, BoundsExtentions.Axis.Z);
                        break;
                }
            }

            // Pos / rot / scale for our handles
            // Scale is based on smallest dimension to ensure handles don't overlap
            // Rotation is just the rotation of our gizmo
            Vector3 pos = Vector3.zero;
            Quaternion rotation = transform.rotation;
            float smallestDimension = Mathf.Min(Mathf.Min(localBounds.size.x, localBounds.size.y), localBounds.size.z);
            if (ClampHandleScale)
            {
                smallestDimension = Mathf.Clamp(smallestDimension, HandleScaleMin, HandleScaleMax);
            }
            Vector3 scale = Vector3.one * smallestDimension;

           // Get the index of our active handle so we can draw it with a different material
            activeHandleIndex = -1;
            if (manipulate.ActiveHandle != null && manipulate.ActiveHandle.HandleType != BoundingBoxHandle.HandleTypeEnum.Drag)
            {
                if (manipulate.ActiveHandle.HandleTypeFlattened == BoundingBoxHandle.HandleTypeFlattenedEnum.None)
                    activeHandleIndex = (int)manipulate.ActiveHandle.HandleType;
                else
                    activeHandleIndex = (int)manipulate.ActiveHandle.HandleTypeFlattened;
            }

            // If we're not accepting input, just draw the box bounds
            if (!manipulate.AcceptInput)
            {
                edgeRenderer.enabled = DisplayEdgesWhenSelected;
                edgeMaterial.SetColor("_EmissionColor", InactiveColor);
            }
            else
            {
                Debug.Log("Switching current operation " + manipulate.CurrentOperation);
                switch (manipulate.CurrentOperation)
                {
                    default:
                    case BoundingBoxManipulate.OperationEnum.None:
                        // No visible bounds
                        // No visible handles
                        edgeRenderer.enabled = false;
                        break;

                    case BoundingBoxManipulate.OperationEnum.Drag:
                        // Target bounds
                        // Inactive scale handles
                        // Inactive rotate handles (based on permitted operations)
                        edgeRenderer.enabled = true;
                        edgeMaterial.SetColor("_EmissionColor", manipulate.ManipulatingNow ? TargetColor : ActiveColor);

                        // Get all our handle positions
                        if (manipulate.FlattenedAxis == BoundingBox.FlattenModeEnum.DoNotFlatten)
                        {
                            GetUnflattenedHandleMatrixes(manipulate.PermittedOperations, handlePositions, rotation, scale, cubeHandleMatrixes, sphereHandleMatrixes, activeHandleIndex);
                        }
                        else
                        {
                            GetFlattenedHandleMatrixes(manipulate.PermittedOperations, manipulate.FlattenedAxis, handlePositions, rotation, scale, cubeHandleMatrixes, sphereHandleMatrixes, activeHandleIndex);
                        }
                        // Draw our handles
                        DrawHandleMeshes(cubeHandleMatrixes, BoxMesh, InactiveColor);
                        DrawHandleMeshes(sphereHandleMatrixes, SphereMesh, InactiveColor);
                        break;

                    case BoundingBoxManipulate.OperationEnum.RotateY:
                    case BoundingBoxManipulate.OperationEnum.RotateZ:
                    case BoundingBoxManipulate.OperationEnum.RotateX:
                        // Visible bounds
                        // Inactive scale handles
                        // Active / Target rotate handles (based on permitted operations)
                        edgeRenderer.enabled = true;
                        edgeMaterial.SetColor("_EmissionColor", InactiveColor);

                        // Get all our handle positions
                        if (manipulate.FlattenedAxis == BoundingBox.FlattenModeEnum.DoNotFlatten)
                        {
                            GetUnflattenedHandleMatrixes(manipulate.PermittedOperations, handlePositions, rotation, scale, cubeHandleMatrixes, sphereHandleMatrixes, activeHandleIndex);
                        }
                        else
                        {
                            GetFlattenedHandleMatrixes(manipulate.PermittedOperations, manipulate.FlattenedAxis, handlePositions, rotation, scale, cubeHandleMatrixes, sphereHandleMatrixes, activeHandleIndex);
                        }
                        // Draw our handles
                        DrawHandleMeshes(cubeHandleMatrixes, BoxMesh, InactiveColor);
                        DrawHandleMeshes(sphereHandleMatrixes, SphereMesh, ActiveColor);
                        DrawTargetMesh(activeHandleIndex, SphereMesh, handlePositions, rotation, scale, manipulate.ManipulatingNow ? TargetColor : ActiveColor);
                        break;

                    case BoundingBoxManipulate.OperationEnum.ScaleUniform:
                        // Inactive bounds
                        // Active / Target scale handles
                        // Inactive rotate handles  (based on permitted operations)
                        edgeRenderer.enabled = true;
                        edgeMaterial.SetColor("_EmissionColor", InactiveColor);

                        // Get all our handle positions
                        if (manipulate.FlattenedAxis == BoundingBox.FlattenModeEnum.DoNotFlatten)
                        {
                            GetUnflattenedHandleMatrixes(manipulate.PermittedOperations, handlePositions, rotation, scale, cubeHandleMatrixes, sphereHandleMatrixes, activeHandleIndex);
                        }
                        else
                        {
                            GetFlattenedHandleMatrixes(manipulate.PermittedOperations, manipulate.FlattenedAxis, handlePositions, rotation, scale, cubeHandleMatrixes, sphereHandleMatrixes, activeHandleIndex);
                        }
                        // Draw our handles
                        DrawHandleMeshes(cubeHandleMatrixes, BoxMesh, ActiveColor);
                        DrawHandleMeshes(sphereHandleMatrixes, SphereMesh, InactiveColor);
                        DrawTargetMesh(activeHandleIndex, BoxMesh, handlePositions, rotation, scale, manipulate.ManipulatingNow ? TargetColor : ActiveColor);
                        break;
                }
            }
        }

        private void DrawTargetMesh(int targetIndex, Mesh mesh, Vector3[] positions, Quaternion rotation, Vector3 scale, Color color)
        {
            if (targetIndex < 0)
                return;

            propertyBlock.SetColor(colorID, color);
            Graphics.DrawMesh(mesh, Matrix4x4.TRS(positions[targetIndex], rotation, scale), HandleMaterial, PhysicsLayer, Camera.current, 0, propertyBlock, UnityEngine.Rendering.ShadowCastingMode.Off, false);
        }

        private void DrawHandleMeshes(List<Matrix4x4> matrixes, Mesh mesh, Color color)
        {
            propertyBlock.SetColor(colorID, color);
            Graphics.DrawMeshInstanced(mesh, 0, HandleMaterial, matrixes, propertyBlock, UnityEngine.Rendering.ShadowCastingMode.Off, false, PhysicsLayer);
        }

        private void GetFlattenedHandleMatrixes(
            BoundingBoxManipulate.OperationEnum permittedOperations,
            BoundingBoxManipulate.FlattenModeEnum flattenAxis,
            Vector3[] positions,
            Quaternion rotation,
            Vector3 scale,
            List<Matrix4x4> cubeMatrixes,
            List<Matrix4x4> sphereMatrixes,
            int targetIndex)
        {
            // Get all our handle positions for cubes
            if ((permittedOperations & BoundingBoxManipulate.OperationEnum.ScaleUniform) == BoundingBoxManipulate.OperationEnum.ScaleUniform)
            {
                for (int i = BoundsExtentions.LT; i <= BoundsExtentions.RB; i++)
                {
                    if (i == targetIndex)
                        continue;

                    cubeMatrixes.Add(Matrix4x4.TRS(handlePositions[i], rotation, scale));
                }
            }

            switch (flattenAxis)
            {
                case BoundingBox.FlattenModeEnum.FlattenX:
                default:
                    break;

                case BoundingBox.FlattenModeEnum.FlattenY:
                    break;

                case BoundingBox.FlattenModeEnum.FlattenZ:
                    break;
            }
            // Get all our handle positions for rotation
            if ((permittedOperations & BoundingBoxManipulate.OperationEnum.RotateX) == BoundingBoxManipulate.OperationEnum.RotateX)
            {
                if (BoundsExtentions.LT_RT != targetIndex)
                    sphereMatrixes.Add(Matrix4x4.TRS(handlePositions[BoundsExtentions.LT_RT], rotation, scale));

                if (BoundsExtentions.RB_LB != targetIndex)
                    sphereMatrixes.Add(Matrix4x4.TRS(handlePositions[BoundsExtentions.RB_LB], rotation, scale));
            }

            if ((permittedOperations & BoundingBoxManipulate.OperationEnum.RotateY) == BoundingBoxManipulate.OperationEnum.RotateY)
            {
                if (BoundsExtentions.LB_LT != targetIndex)
                    sphereMatrixes.Add(Matrix4x4.TRS(handlePositions[BoundsExtentions.LB_LT], rotation, scale));

                if (BoundsExtentions.RT_RB != targetIndex)
                    sphereMatrixes.Add(Matrix4x4.TRS(handlePositions[BoundsExtentions.RT_RB], rotation, scale));
            }

            if ((permittedOperations & BoundingBoxManipulate.OperationEnum.RotateZ) == BoundingBoxManipulate.OperationEnum.RotateZ)
            {
                if (BoundsExtentions.LT_RT != targetIndex)
                    sphereMatrixes.Add(Matrix4x4.TRS(handlePositions[BoundsExtentions.LT_RT], rotation, scale));

                if (BoundsExtentions.RB_LB != targetIndex)
                    sphereMatrixes.Add(Matrix4x4.TRS(handlePositions[BoundsExtentions.RB_LB], rotation, scale));
            }
        }

        private void GetUnflattenedHandleMatrixes(
            BoundingBoxManipulate.OperationEnum permittedOperations, 
            Vector3[] positions, 
            Quaternion rotation, 
            Vector3 scale, 
            List<Matrix4x4> cubeMatrixes, 
            List<Matrix4x4> sphereMatrixes, 
            int targetIndex)
        {
            // Get all our handle positions for cubes
            if ((permittedOperations & BoundingBoxManipulate.OperationEnum.ScaleUniform) == BoundingBoxManipulate.OperationEnum.ScaleUniform)
            {
                for (int i = BoundsExtentions.LBF; i <= BoundsExtentions.RTB; i++)
                {
                    if (i == targetIndex)
                        continue;

                    cubeMatrixes.Add(Matrix4x4.TRS(handlePositions[i], rotation, scale));
                }
            }
            // Get all our handle positions for rotation
            if ((permittedOperations & BoundingBoxManipulate.OperationEnum.RotateX) == BoundingBoxManipulate.OperationEnum.RotateX)
            {
                for (int i = BoundsExtentions.LTF_RTF; i <= BoundsExtentions.RBB_LBB; i++)
                {
                    if (i == targetIndex)
                        continue;

                    sphereMatrixes.Add(Matrix4x4.TRS(handlePositions[i], rotation, scale));
                }
            }

            if ((permittedOperations & BoundingBoxManipulate.OperationEnum.RotateY) == BoundingBoxManipulate.OperationEnum.RotateY)
            {
                for (int i = BoundsExtentions.LTF_LBF; i <= BoundsExtentions.RTF_RBF; i++)
                {
                    if (i == targetIndex)
                        continue;

                    sphereMatrixes.Add(Matrix4x4.TRS(handlePositions[i], rotation, scale));
                }
            }

            if ((permittedOperations & BoundingBoxManipulate.OperationEnum.RotateZ) == BoundingBoxManipulate.OperationEnum.RotateZ)
            {
                for (int i = BoundsExtentions.RBF_RBB; i <= BoundsExtentions.LTF_LTB; i++)
                {
                    if (i == targetIndex)
                        continue;

                    sphereMatrixes.Add(Matrix4x4.TRS(handlePositions[i], rotation, scale));
                }
            }
        }

        #endregion
    }
}