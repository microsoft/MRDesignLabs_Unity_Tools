//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
using HUX.Interaction;
using UnityEditor;
using UnityEngine;

namespace HUX
{
    [CustomEditor(typeof(BoundingBoxManipulate))]
    public class BoundingBoxManipulateInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            BoundingBoxManipulate bbm = (BoundingBoxManipulate)target;
            
            GUI.color = HUXEditorUtils.DefaultColor;

            bbm.Target = (GameObject)EditorGUILayout.ObjectField("Target", bbm.Target, typeof (GameObject), true);
            //bbm.ActiveHandle = (BoundingBoxHandle)EditorGUILayout.ObjectField("Active Handle", bbm.ActiveHandle, typeof(BoundingBoxHandle), true);

            HUXEditorUtils.ArrayField(serializedObject, "Interactibles");

            HUXEditorUtils.BeginSectionBox("Settings");
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Bounds method", GUILayout.MaxWidth(100));
            bbm.BoundsCalculationMethod = (BoundingBox.BoundsCalculationMethodEnum)EditorGUILayout.EnumPopup(bbm.BoundsCalculationMethod, GUILayout.MaxWidth (155));
            switch (bbm.BoundsCalculationMethod)
            {
                case BoundingBox.BoundsCalculationMethodEnum.MeshFilterBounds:
                    HUXEditorUtils.DrawSubtleMiniLabel("Uses all MeshFilters to calculate bounds. This setting is more accurate (especially for flat objects), but will only calculate bounds for mesh-based objects.");
                    break;

                case BoundingBox.BoundsCalculationMethodEnum.Colliders:
                    HUXEditorUtils.DrawSubtleMiniLabel("Uses all Colliders to calculate bounds. This setting is best if you want precise manual control over bounds size.");
                    break;

                case BoundingBox.BoundsCalculationMethodEnum.RendererBounds:
                    HUXEditorUtils.DrawSubtleMiniLabel("Uses all Renderers to calculate bounds. This setting is less accurate, but can calculate bounds for objects like particle systems.");
                    break;
            }
            EditorGUILayout.EndHorizontal();
            bbm.PhysicsLayer = EditorGUILayout.IntSlider("Physics / Rendering Layer", bbm.PhysicsLayer, 0, 32);
            bbm.IgnoreLayer = EditorGUILayout.IntSlider("Ignore Mesh Renderers on this Layer", bbm.IgnoreLayer, 0, 32);
            HUXEditorUtils.EndSectionBox();

            HUXEditorUtils.BeginSectionBox("Manipulation");
            bbm.DragMultiplier = EditorGUILayout.Slider("Drag input scale", bbm.DragMultiplier, 0.01f, 20f);
            bbm.RotateMultiplier = EditorGUILayout.Slider("Rotation input scale", bbm.RotateMultiplier, 0.01f, 20f);
            bbm.ScaleMultiplier = EditorGUILayout.Slider("Scale input scale", bbm.ScaleMultiplier, 0.01f, 20f);
            bbm.MinScalePercentage = EditorGUILayout.Slider("Minimum scale per operation", bbm.MinScalePercentage, 0.05f, 1f);
            HUXEditorUtils.EndSectionBox();

            HUXEditorUtils.BeginSectionBox("Flattening");
            bbm.FlattenPreference = (BoundingBox.FlattenModeEnum)EditorGUILayout.EnumPopup("Flattening preference", bbm.FlattenPreference);
            switch (bbm.FlattenPreference)
            {
                case BoundingBox.FlattenModeEnum.DoNotFlatten:
                    HUXEditorUtils.DrawSubtleMiniLabel("Bounding box will never be flattened no matter how thin the target object gets.");
                    break;

                case BoundingBox.FlattenModeEnum.FlattenAuto:
                    HUXEditorUtils.DrawSubtleMiniLabel("If an axis drops below the relative % threshold, that axis will be flattened to the specified thickness.");
                    bbm.FlattenAxisThreshold = EditorGUILayout.Slider("Flatten axis threshold %", (bbm.FlattenAxisThreshold * 100), 0.01f, 100f) / 100;
                    bbm.FlattenedAxisThickness = EditorGUILayout.Slider("Flattened axis thickness", bbm.FlattenedAxisThickness, 0.001f, 1f);
                    if (bbm.BoundsCalculationMethod == BoundingBox.BoundsCalculationMethodEnum.RendererBounds)
                    {
                        HUXEditorUtils.WarningMessage("The " + bbm.BoundsCalculationMethod + " method may result in distortion for flattened objects. " + BoundingBox.BoundsCalculationMethodEnum.MeshFilterBounds + " method is recommended for this setting.");
                    }
                    EditorGUILayout.EnumPopup("Current flattened axis: ", bbm.FlattenedAxis);
                    break;

                case BoundingBox.FlattenModeEnum.FlattenX:
                case BoundingBox.FlattenModeEnum.FlattenY:
                case BoundingBox.FlattenModeEnum.FlattenZ:
                    HUXEditorUtils.DrawSubtleMiniLabel("The selected axis will be flattened to the specified thickness.");
                    bbm.FlattenedAxisThickness = EditorGUILayout.Slider("Flattened axis thickness", bbm.FlattenedAxisThickness, 0.001f, 1f);
                    if (bbm.BoundsCalculationMethod == BoundingBox.BoundsCalculationMethodEnum.RendererBounds)
                    {
                        HUXEditorUtils.WarningMessage("The " + bbm.BoundsCalculationMethod + " method may result in distortion for flattened objects. " + BoundingBox.BoundsCalculationMethodEnum.MeshFilterBounds + " method is recommended for this setting.");
                    }
                    break;
            }
            HUXEditorUtils.EndSubSectionBox();

            bbm.PermittedOperations = (BoundingBoxManipulate.OperationEnum) HUXEditorUtils.EnumCheckboxField<BoundingBoxManipulate.OperationEnum>(
                "Permitted Operations",
                bbm.PermittedOperations,
                "Default",
                BoundingBoxManipulate.OperationEnum.ScaleUniform | BoundingBoxManipulate.OperationEnum.RotateY | BoundingBoxManipulate.OperationEnum.Drag,
                BoundingBoxManipulate.OperationEnum.Drag);

            if (!Application.isPlaying)
            {
                bbm.AcceptInput = EditorGUILayout.Toggle("Accept Input", bbm.AcceptInput);
                bbm.ManipulatingNow = EditorGUILayout.Toggle("Manipulating Now", bbm.ManipulatingNow);
            }

            HUXEditorUtils.SaveChanges(target, serializedObject);
        }
    }
}