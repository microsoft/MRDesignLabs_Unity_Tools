//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
using HUX.Interaction;
using UnityEditor;

namespace HUX
{
    [CustomEditor(typeof(BoundingBoxTarget))]
    public class BoundingBoxTargetInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            BoundingBoxTarget bbt = (BoundingBoxTarget)target;

            HUXEditorUtils.DrawFilterTagField(serializedObject, "TagOnSelected");
            HUXEditorUtils.DrawFilterTagField(serializedObject, "TagOnDeselected");

            /*EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Bounds method", GUILayout.MaxWidth(100));
            bbt.BoundsCalculationMethod = (BoundingBox.BoundsCalculationMethodEnum)EditorGUILayout.EnumPopup(bbt.BoundsCalculationMethod, GUILayout.MaxWidth(155));
            switch (bbt.BoundsCalculationMethod)
            {
                case BoundingBox.BoundsCalculationMethodEnum.MeshFilterBounds:
                    HUXEditorUtils.DrawSubtleMiniLabel("Uses all MeshFilters to calculate bounds. This setting is more accurate (especially for flat objects), but will only calculate bounds for mesh-based objects.");
                    break;

                case BoundingBox.BoundsCalculationMethodEnum.RendererBounds:
                    HUXEditorUtils.DrawSubtleMiniLabel("Uses all Renderers to calculate bounds. This setting is less accurate, but can calculate bounds for objects like particle systems.");
                    break;
            }
            EditorGUILayout.EndHorizontal();*/

            bbt.PermittedOperations = (BoundingBoxManipulate.OperationEnum) HUXEditorUtils.EnumCheckboxField<BoundingBoxManipulate.OperationEnum>(
                "Permitted Operations",
                bbt.PermittedOperations,
                "Default",
                BoundingBoxManipulate.OperationEnum.ScaleUniform | BoundingBoxManipulate.OperationEnum.RotateY | BoundingBoxManipulate.OperationEnum.Drag,
                BoundingBoxManipulate.OperationEnum.Drag);

            bbt.FlattenPreference = (BoundingBox.FlattenModeEnum)EditorGUILayout.EnumPopup("Flattening mode", bbt.FlattenPreference);

            bbt.ShowAppBar = EditorGUILayout.Toggle("Toolbar Display", bbt.ShowAppBar);


            HUXEditorUtils.SaveChanges(target, serializedObject);
        }
    }
}