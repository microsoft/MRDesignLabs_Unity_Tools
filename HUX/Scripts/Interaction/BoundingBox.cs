//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
using HUX.Receivers;
using System.Collections.Generic;
using UnityEngine;

namespace HUX.Interaction
{
    /// <summary>
    /// Base class for bounding box objects
    /// </summary>
    public class BoundingBox : InteractionReceiver
    {
        public enum BoundsCalculationMethodEnum
        {
            MeshFilterBounds,   // Better for flattened objects - this mode also treats RectTransforms as quad meshes
            RendererBounds,     // Better for objects with non-mesh renderers
            Colliders,          // Better if you want precise control
            Default,            // Use the default method (RendererBounds)
        }

        public enum FlattenModeEnum
        {
            DoNotFlatten,   // Always use XYZ axis
            FlattenX,       // Flatten the X axis
            FlattenY,       // Flatten the Y axis
            FlattenZ,       // Flatten the Z axis
            FlattenAuto,    // Flatten the smallest relative axis if it falls below threshold
        }

        #region public

        /// <summary>
        /// Layer for drawing & colliders
        /// </summary>
        public int PhysicsLayer = 0;

        /// <summary>
        /// Any renderers on this layer will be ignored when calculating object bounds
        /// </summary>
        public int IgnoreLayer = 2;//Ignore Raycast

        /// <summary>
        /// Flattening behavior setting
        /// </summary>
        public FlattenModeEnum FlattenPreference = FlattenModeEnum.FlattenAuto;

        public BoundsCalculationMethodEnum BoundsCalculationMethod = BoundsCalculationMethodEnum.MeshFilterBounds;

        /// <summary>
        /// The relative % size of an axis must meet before being auto-flattened
        /// </summary>
        public float FlattenAxisThreshold = 0.025f;

        /// <summary>
        /// The relative % size of a flattened axis
        /// </summary>
        public float FlattenedAxisThickness = 0.01f;

        /// <summary>
        /// The target object being manipulated
        /// </summary>
        public virtual GameObject Target
        {
            get
            {
                return target;
            }
            set
            {
                if (target != value)
                {
                    // Send a message to the new / old targets
                    if (value != null)
                    {
                        value.SendMessage("OnTargetSelected", SendMessageOptions.DontRequireReceiver);
                    }
                    if (target != null)
                    {
                        target.SendMessage("OnTargetDeselected", SendMessageOptions.DontRequireReceiver);
                    }
                    target = value;
                }

                if (!isActiveAndEnabled)
                    return;

                if (target != null)
                {
                    CreateTransforms();
                    // Set our transforms to the target immediately
                    targetStandIn.position = target.transform.position;
                    targetStandIn.rotation = target.transform.rotation;
                    targetStandIn.localScale = target.transform.lossyScale;
                    RefreshTargetBounds();
                }
            }
        }

        /// <summary>
        /// The world-space center of the target object's bounds
        /// </summary>
        public Vector3 TargetBoundsCenter
        {
            get
            {
                return targetBoundsWorldCenter;
            }
        }

        /// <summary>
        /// The local scale of the target object's bounds
        /// </summary>
        public Vector3 TargetBoundsLocalScale
        {
            get
            {
                return targetBoundsLocalScale;
            }
        }

        /// <summary>
        /// The current flattened axis, if any
        /// </summary>
        public virtual FlattenModeEnum FlattenedAxis
        {
            get
            {
                return flattenedAxis;
            } protected set
            {
                flattenedAxis = value;
            }
        }

        public override void OnDisable()
        {
            base.OnDisable();
            if (transformHelper != null)
            {
                GameObject.Destroy(transformHelper.gameObject);
            }
        }

        #endregion

        #region private

        /// <summary>
        /// Override so we're not overwhelmed by button gizmos
        /// </summary>
        #if UNITY_EDITOR
        protected override void OnDrawGizmos()
        {
            // nothing
            if (!Application.isPlaying)
            {
                // Do this here to ensure continuous updates in editor
                RefreshTargetBounds();
            }

            if (target != null)
            {
                foreach (Vector3 point in boundsPoints)
                {
                    Gizmos.DrawSphere(target.transform.TransformPoint(point), 0.01f);
                }
            }
        }
        #endif

        protected void CreateTransforms() {
            // Create our transform helpers if they don't exist
            if (transformHelper == null) {
                transformHelper = transform.Find("BoundingBoxTransformHelper");
                if (transformHelper == null)
                    transformHelper = new GameObject("BoundingBoxTransformHelper").transform;

                targetStandIn = transformHelper.Find("TargetStandIn");
                if (targetStandIn == null)
                    targetStandIn = new GameObject("TargetStandIn").transform;

                transformHelper.parent = transform;
                targetStandIn.parent = transformHelper;
            }
        }

        protected virtual void Update()
        {
            if (!Application.isPlaying)
                return;

            RefreshTargetBounds();
        }

        protected virtual void RefreshTargetBounds()
        {
            if (target == null)
            {
                targetBoundsWorldCenter = Vector3.zero;
                targetBoundsLocalScale = Vector3.one;
                return;
            }

            // Get the new target bounds
            boundsPoints.Clear();


            switch (BoundsCalculationMethod)
            {
                case BoundsCalculationMethodEnum.RendererBounds:
                default:
                    Renderer[] renderers = target.GetComponentsInChildren<Renderer>();
                    for (int i = 0; i < renderers.Length; ++i)
                    {
                        var rendererObj = renderers[i];
                        if (rendererObj.gameObject.layer == IgnoreLayer)
                            continue;

                        rendererObj.bounds.GetCornerPositionsFromRendererBounds(ref corners);
                        boundsPoints.AddRange(corners);
                    }
                    break;

                case BoundsCalculationMethodEnum.Colliders:
                    Collider[] colliders = target.GetComponentsInChildren<Collider>();
                    for (int i = 0; i < colliders.Length; i++)
                    {
                        switch (colliders[i].GetType().Name)
                        {
                            case "SphereCollider":
                                SphereCollider sc = colliders[i] as SphereCollider;
                                Bounds sphereBounds = new Bounds(sc.center, Vector3.one * sc.radius * 2);
                                sphereBounds.GetFacePositions(sc.transform, ref corners);
                                boundsPoints.AddRange(corners);
                                break;

                            case "BoxCollider":
                                BoxCollider bc = colliders[i] as BoxCollider;
                                Bounds boxBounds = new Bounds(bc.center, bc.size);
                                boxBounds.GetCornerPositions(bc.transform, ref corners);
                                boundsPoints.AddRange(corners);
                                break;

                            case "MeshCollider":
                                MeshCollider mc = colliders[i] as MeshCollider;
                                Bounds meshBounds = mc.sharedMesh.bounds;
                                meshBounds.GetCornerPositions(mc.transform, ref corners);
                                boundsPoints.AddRange(corners);
                                break;

                            case "CapsuleCollider":
                                CapsuleCollider cc = colliders[i] as CapsuleCollider;
                                Bounds capsuleBounds = new Bounds(cc.center, Vector3.zero);
                                switch (cc.direction)
                                {
                                    case 0:
                                        capsuleBounds.size = new Vector3(cc.height, cc.radius * 2, cc.radius * 2);
                                        break;

                                    case 1:
                                        capsuleBounds.size = new Vector3(cc.radius * 2, cc.height, cc.radius * 2);
                                        break;

                                    case 2:
                                        capsuleBounds.size = new Vector3(cc.radius * 2, cc.radius * 2, cc.height);
                                        break;
                                }
                                capsuleBounds.GetFacePositions(cc.transform, ref corners);
                                boundsPoints.AddRange(corners);
                                break;

                            default:
                                break;
                        }
                    }
                    break;

                case BoundsCalculationMethodEnum.MeshFilterBounds:
                    MeshFilter[] meshFilters = target.GetComponentsInChildren<MeshFilter>();
                    for (int i = 0; i < meshFilters.Length; i++)
                    {
                        var meshFilterObj = meshFilters[i];
                        if (meshFilterObj.gameObject.layer == IgnoreLayer)
                            continue;

                        Bounds meshBounds = meshFilterObj.sharedMesh.bounds;
                        meshBounds.GetCornerPositions(meshFilterObj.transform, ref corners);
                        boundsPoints.AddRange(corners);
                    }
                    RectTransform[] rectTransforms = target.GetComponentsInChildren<RectTransform>();
                    for (int i = 0; i < rectTransforms.Length; i++)
                    {
                        rectTransforms[i].GetWorldCorners(rectTransformCorners);
                        boundsPoints.AddRange(rectTransformCorners);
                    }
                    break;
            }
            
            if (boundsPoints.Count > 0)
            {
                // We now have a list of all points in world space
                // Translate them all to local space
                for (int i = 0; i < boundsPoints.Count; i++)
                {
                    boundsPoints[i] = target.transform.InverseTransformPoint(boundsPoints[i]);
                }

                // Encapsulate the points with a local bounds
                localTargetBounds.center = boundsPoints[0];
                localTargetBounds.size = Vector3.zero;
                foreach (Vector3 point in boundsPoints)
                {
                    localTargetBounds.Encapsulate(point);
                }
            }

            // Store the world center of the target bb
            targetBoundsWorldCenter = target.transform.TransformPoint(localTargetBounds.center);

            // Store the local scale of the target bb
            targetBoundsLocalScale = localTargetBounds.size;
            targetBoundsLocalScale.Scale(target.transform.localScale);

            // Find the maximum size of the new bounds
            float maxAxisThickness = Mathf.Max(Mathf.Max(targetBoundsLocalScale.x, targetBoundsLocalScale.y), targetBoundsLocalScale.z);

            // Now check our flatten behavior
            FlattenModeEnum newFlattenedAxis = FlattenModeEnum.DoNotFlatten;
            switch (FlattenPreference)
            {
                case FlattenModeEnum.DoNotFlatten:
                    // Do nothing
                    break;

                case FlattenModeEnum.FlattenAuto:
                    // Flattening order of preference - z, y, x
                    if (Mathf.Abs(targetBoundsLocalScale.z / maxAxisThickness) < FlattenAxisThreshold) {
                        newFlattenedAxis = FlattenModeEnum.FlattenZ;
                        targetBoundsLocalScale.z = FlattenedAxisThickness * maxAxisThickness;
                    }
                    else if (Mathf.Abs(targetBoundsLocalScale.y / maxAxisThickness) < FlattenAxisThreshold) {
                        newFlattenedAxis = FlattenModeEnum.FlattenY;
                        targetBoundsLocalScale.y = FlattenedAxisThickness * maxAxisThickness;
                    }
                    else if (Mathf.Abs(targetBoundsLocalScale.x / maxAxisThickness) < FlattenAxisThreshold) {
                        newFlattenedAxis = FlattenModeEnum.FlattenX;
                        targetBoundsLocalScale.x = FlattenedAxisThickness * maxAxisThickness;
                    }
                    break;

                case FlattenModeEnum.FlattenX:
                    newFlattenedAxis = FlattenModeEnum.FlattenX;
                    targetBoundsLocalScale.x = FlattenedAxisThickness * maxAxisThickness;
                    break;

                case FlattenModeEnum.FlattenY:
                    newFlattenedAxis = FlattenModeEnum.FlattenY;
                    targetBoundsLocalScale.y = FlattenedAxisThickness * maxAxisThickness;
                    break;

                case FlattenModeEnum.FlattenZ:
                    newFlattenedAxis = FlattenModeEnum.FlattenZ;
                    targetBoundsLocalScale.z = FlattenedAxisThickness * maxAxisThickness;
                    break;
            }

            FlattenedAxis = newFlattenedAxis;
        }

        [SerializeField]
        protected GameObject target;

        /// <summary>
        /// These are used to make complex scaling operations simpler
        /// </summary>
        protected Transform transformHelper = null;
        protected Transform targetStandIn = null;

        protected Vector3 targetBoundsWorldCenter = Vector3.zero;
        protected Vector3 targetBoundsLocalScale = Vector3.zero;

        protected Vector3[] corners = null;
        protected Vector3[] rectTransformCorners = new Vector3[4];
        protected Bounds localTargetBounds = new Bounds();
        protected List<Vector3> boundsPoints = new List<Vector3>();

        protected FlattenModeEnum flattenedAxis = FlattenModeEnum.DoNotFlatten;

        #endregion
    }
}