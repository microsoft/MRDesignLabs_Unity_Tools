using HUX;
using UnityEditor;
using UnityEngine;

namespace MRDL.ToolTips
{
    [CustomEditor(typeof(ToolTipBackgroundCorners))]
    public class ToolTipBackgroundCornersInspector : Editor
    {
        public override void OnInspectorGUI() {
                        
            ToolTipBackgroundCorners ttb = (ToolTipBackgroundCorners)target;
            ToolTip tt = ttb.GetComponent<ToolTip>();
            
            if (tt == null) {
                HUXEditorUtils.ErrorMessage("This component requires a ToolTip component to work.", AddToolTip, "Add ToolTip Component");
                return;
            }

            SerializedProperty toolTipProp = serializedObject.FindProperty("toolTip");
            if (toolTipProp.objectReferenceValue == null) {
                toolTipProp.objectReferenceValue = tt;
            }

            Transform newCorner = null;

            HUXEditorUtils.BeginSectionBox("Corner Transforms");
            EditorGUILayout.BeginHorizontal();
            SerializedProperty cornerTopLeftProp = serializedObject.FindProperty("cornerTopLeft");
            cornerTopLeftProp.objectReferenceValue = HUXEditorUtils.DropDownComponentField<Transform>("Top Left", cornerTopLeftProp.objectReferenceValue as Transform, tt.ContentParentTransform, false);
            if (cornerTopLeftProp.objectReferenceValue == null) {
                if (GUILayout.Button ("Create")) {
                    newCorner = new GameObject("TopLeft").transform;
                    newCorner.parent = tt.ContentParentTransform;
                    cornerTopLeftProp.objectReferenceValue = newCorner;
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            SerializedProperty cornerTopRightProp = serializedObject.FindProperty("cornerTopRight");
            cornerTopRightProp.objectReferenceValue = HUXEditorUtils.DropDownComponentField<Transform>("Top Right", cornerTopRightProp.objectReferenceValue as Transform, tt.ContentParentTransform, false);
            if (cornerTopRightProp.objectReferenceValue == null) {
                if (GUILayout.Button("Create")) {
                    newCorner = new GameObject("TopRight").transform;
                    newCorner.parent = tt.ContentParentTransform;
                    cornerTopRightProp.objectReferenceValue = newCorner;
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            SerializedProperty cornerBotLeftProp = serializedObject.FindProperty("cornerBotLeft");
            cornerBotLeftProp.objectReferenceValue = HUXEditorUtils.DropDownComponentField<Transform>("Bottom Left", cornerBotLeftProp.objectReferenceValue as Transform, tt.ContentParentTransform, false);
            if (cornerBotLeftProp.objectReferenceValue == null) {
                if (GUILayout.Button("Create")) {
                    newCorner = new GameObject("BotLeft").transform;
                    newCorner.parent = tt.ContentParentTransform;
                    cornerBotLeftProp.objectReferenceValue = newCorner;
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            SerializedProperty cornerBotRightProp = serializedObject.FindProperty("cornerBotRight");
            cornerBotRightProp.objectReferenceValue = HUXEditorUtils.DropDownComponentField<Transform>("Bottom Right", cornerBotRightProp.objectReferenceValue as Transform, tt.ContentParentTransform, false);
            if (cornerBotRightProp.objectReferenceValue == null) {
                if (GUILayout.Button("Create")) {
                    newCorner = new GameObject("BotRight").transform;
                    newCorner.parent = tt.ContentParentTransform;
                    cornerBotRightProp.objectReferenceValue = newCorner;
                }
            }
            EditorGUILayout.EndHorizontal();

            if (cornerTopLeftProp.objectReferenceValue == null
                && cornerTopRightProp.objectReferenceValue == null
                && cornerBotLeftProp.objectReferenceValue == null
                && cornerBotRightProp.objectReferenceValue == null) {
                HUXEditorUtils.ErrorMessage("You need to set at least one corner transform.");
            } else if (cornerTopLeftProp.objectReferenceValue == null
                || cornerTopRightProp.objectReferenceValue == null
                || cornerBotLeftProp.objectReferenceValue == null
                || cornerBotRightProp.objectReferenceValue == null) {
                HUXEditorUtils.WarningMessage("Not all corner transforms are set. (This may be intentional)");
            }

            SerializedProperty cornerScaleProp = serializedObject.FindProperty("cornerScale");
            EditorGUILayout.PropertyField(cornerScaleProp);

            HUXEditorUtils.EndSectionBox();
            
            HUXEditorUtils.SaveChanges(target, serializedObject);
        }

        private void AddToolTip() {
            ToolTipBackgroundCorners ttb = (ToolTipBackgroundCorners)target;
            ToolTip tt = ttb.gameObject.AddComponent<ToolTip>();
            SerializedProperty toolTipProp = serializedObject.FindProperty("toolTip");
            toolTipProp.objectReferenceValue = tt;
        }
    }
}