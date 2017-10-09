using UnityEditor;
using HUX;
using UnityEngine;

namespace MRDL.ToolTips
{
    [CustomEditor(typeof(ToolTipBackgroundMesh))]
    public class ToolTipBackgroundMeshInspector : Editor
    {
        public override void OnInspectorGUI() {
            ToolTipBackgroundMesh ttb = (ToolTipBackgroundMesh)target;
            ToolTip tt = ttb.GetComponent<ToolTip>();
            if (tt == null) {
                HUXEditorUtils.ErrorMessage("This component requires a ToolTip component to work.", AddToolTip, "Add ToolTip Component");
                HUXEditorUtils.SaveChanges(target, serializedObject);
                HUXEditorUtils.EndSectionBox();
                return;
            }
            
            SerializedProperty toolTipProp = serializedObject.FindProperty("toolTip");
            if (toolTipProp.objectReferenceValue == null) {
                toolTipProp.objectReferenceValue = tt;
            }

            HUXEditorUtils.BeginSectionBox("Background Components");
            ttb.BackgroundRenderer = HUXEditorUtils.DropDownComponentField<MeshRenderer>("Mesh Renderer", ttb.BackgroundRenderer, tt.ContentParentTransform);
            if (ttb.BackgroundRenderer == null) {
                HUXEditorUtils.ErrorMessage("You must select or create a mesh renderer under the tool tip's content parent.", CreateMeshRenderer, "Create Mesh Renderer");
                HUXEditorUtils.SaveChanges(target, serializedObject);
                HUXEditorUtils.EndSectionBox();
                return;
            }
            if (ttb.BackgroundTransform == null) {
                ttb.BackgroundTransform = ttb.BackgroundRenderer.transform;
            } else {
                ttb.BackgroundTransform = HUXEditorUtils.DropDownComponentField<Transform>("Transform", ttb.BackgroundTransform, tt.ContentParentTransform);
            }
            if (ttb.BackgroundTransform != ttb.BackgroundRenderer.transform) {
                HUXEditorUtils.WarningMessage("Not using the BackgroundRenderer's transform may result in unexpected behavior.", "Use BackgroundRenderer's transform", UseRendererTransform);
            }
            ttb.Depth = EditorGUILayout.FloatField("Mesh depth", ttb.Depth);
            HUXEditorUtils.EndSectionBox();
            
            HUXEditorUtils.SaveChanges(target, serializedObject);
        }

        private void UseRendererTransform() {
            ToolTipBackgroundMesh ttb = (ToolTipBackgroundMesh)target;
            ttb.BackgroundTransform = ttb.BackgroundRenderer.transform;
        }

        private void AddToolTip() {
            ToolTipBackgroundMesh ttb = (ToolTipBackgroundMesh)target;
            ToolTip tt = ttb.gameObject.AddComponent<ToolTip>();
            SerializedProperty toolTipProp = serializedObject.FindProperty("toolTip");
            toolTipProp.objectReferenceValue = tt;
        }

        private void CreateMeshRenderer() {
            ToolTipBackgroundMesh ttb = (ToolTipBackgroundMesh)target;
            ToolTip tt = ttb.GetComponent<ToolTip>();
            GameObject newBackground = GameObject.CreatePrimitive(PrimitiveType.Quad);
            newBackground.name = "ToolTip Background Mesh";
            newBackground.transform.parent = tt.ContentParentTransform;
            ttb.BackgroundRenderer = newBackground.GetComponent<MeshRenderer>();
        }
    }
}