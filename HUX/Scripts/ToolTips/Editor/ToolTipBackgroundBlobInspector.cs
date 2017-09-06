using HUX;
using UnityEditor;
using UnityEngine;

namespace MRDL.ToolTips
{
    [CustomEditor(typeof(ToolTipBackgroundBlob))]
    public class ToolTipBackgroundBlobInspector : Editor
    {
        public override void OnInspectorGUI() {
            ToolTipBackgroundBlob ttb = (ToolTipBackgroundBlob)target;
            ToolTip tt = ttb.GetComponent<ToolTip>();
            if (tt == null) {
                HUXEditorUtils.ErrorMessage("This component requires a ToolTip component to work.", AddToolTip, "Add ToolTip Component");
                return;
            }

            SerializedProperty toolTipProp = serializedObject.FindProperty("toolTip");
            if (toolTipProp.objectReferenceValue == null) {
                toolTipProp.objectReferenceValue = tt;
            }

            HUXEditorUtils.BeginSectionBox("Target Components");
            if (ttb.PositionTarget == null || ttb.RotationTarget == null || ttb.DistortionTarget == null) {
                if (GUILayout.Button("Create default targets")) {
                    ttb.PositionTarget = tt.ContentParentTransform;
                    ttb.RotationTarget = new GameObject("BackgroundBlob").transform;
                    ttb.RotationTarget.parent = tt.ContentParentTransform;
                    ttb.DistortionTarget = ttb.RotationTarget;
                }
            }
            ttb.PositionTarget = HUXEditorUtils.DropDownComponentField<Transform>("Position", ttb.PositionTarget, tt.ContentParentTransform, false);
            if (ttb.PositionTarget == null) {
                HUXEditorUtils.ErrorMessage("You must set a position target.");
                HUXEditorUtils.EndSectionBox();
                HUXEditorUtils.SaveChanges(target, serializedObject);
                return;
            }
            ttb.RotationTarget = HUXEditorUtils.DropDownComponentField<Transform>("Rotation", ttb.RotationTarget, tt.ContentParentTransform, false);
            if (ttb.RotationTarget == null) {
                HUXEditorUtils.ErrorMessage("You must set a rotation target.");
                HUXEditorUtils.EndSectionBox();
                HUXEditorUtils.SaveChanges(target, serializedObject);
                return;
            }
            ttb.DistortionTarget = HUXEditorUtils.DropDownComponentField<Transform>("Distortion", ttb.DistortionTarget, tt.ContentParentTransform, false);
            if (ttb.DistortionTarget == null) {
                HUXEditorUtils.ErrorMessage("You must set a distortion target.");
                HUXEditorUtils.EndSectionBox();
                HUXEditorUtils.SaveChanges(target, serializedObject);
                return;
            }
            ttb.AttachPointOffset = HUXEditorUtils.DropDownComponentField<Transform>("Attach Point Offset", ttb.AttachPointOffset, tt.ContentParentTransform, false);
            if (ttb.AttachPointOffset != null) {
                HUXEditorUtils.WarningMessage("Setting the attach point offset will change the default position of the tool tip line.");
            }

            Renderer positionTargetRenderer = ttb.PositionTarget.GetComponent<Renderer>();
            Renderer rotationTargetRenderer = ttb.RotationTarget.GetComponent<Renderer>();
            Renderer distortionTargetRenderer = ttb.DistortionTarget.GetComponent<Renderer>();
            if (positionTargetRenderer == null && rotationTargetRenderer == null && distortionTargetRenderer == null)
                HUXEditorUtils.WarningMessage("None of your targets have renderers attached to them. (This may be intentional.)");

            HUXEditorUtils.EndSectionBox();

            HUXEditorUtils.BeginSectionBox("Blob Settings");
            ttb.BlobInertia = EditorGUILayout.Slider("Inertia", ttb.BlobInertia, 0, ToolTipBackgroundBlob.MaxInertia);
            ttb.BlobDistortion = EditorGUILayout.Slider("Distortion", ttb.BlobDistortion, 0, ToolTipBackgroundBlob.MaxDistortion);
            ttb.BlobRotation = EditorGUILayout.Slider("Rotation", ttb.BlobRotation, 0, ToolTipBackgroundBlob.MaxRotation);
            ttb.PositionCorrectionStrength = EditorGUILayout.Slider("Position Correction", ttb.PositionCorrectionStrength, ToolTipBackgroundBlob.MinPositionCorrection, ToolTipBackgroundBlob.MaxPositionCorrection);
            ttb.DistortionCorrectionStrength = EditorGUILayout.Slider("Distortion Correction", ttb.DistortionCorrectionStrength, ToolTipBackgroundBlob.MinDistortionCorrection, ToolTipBackgroundBlob.MaxDistortionCorrection);
            ttb.RotationCorrectionStrength = EditorGUILayout.Slider("Rotation Correction", ttb.RotationCorrectionStrength, ToolTipBackgroundBlob.MinRotationCorrection, ToolTipBackgroundBlob.MaxRotationCorrection);
            ttb.BlobOffset = EditorGUILayout.Vector3Field("Offset", ttb.BlobOffset);
            HUXEditorUtils.BeginSubSectionBox("Presets");
            if(GUILayout.Button ("Balloon")) {
                ttb.BlobInertia = 0.25f;
                ttb.BlobDistortion = 0.75f;
                ttb.BlobRotation = 1f;
                ttb.PositionCorrectionStrength = 5f;
                ttb.DistortionCorrectionStrength = 4f;
                ttb.RotationCorrectionStrength = 0.1f;
                ttb.BlobOffset = new Vector3(0f, -0.15f, 0f);
            }
            HUXEditorUtils.EndSubSectionBox();
            HUXEditorUtils.EndSectionBox();

            HUXEditorUtils.SaveChanges(target, serializedObject);
        }

        private void AddToolTip() {
            ToolTipBackgroundBlob ttb = (ToolTipBackgroundBlob)target;
            ToolTip tt = ttb.gameObject.AddComponent<ToolTip>();
            SerializedProperty toolTipProp = serializedObject.FindProperty("toolTip");
            toolTipProp.objectReferenceValue = tt;
        }
    }
}