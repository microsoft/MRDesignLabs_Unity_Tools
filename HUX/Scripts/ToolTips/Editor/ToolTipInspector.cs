using HUX;
using MRDL.Design;
using System;
using UnityEditor;
using UnityEngine;

namespace MRDL.ToolTips
{
    [CustomEditor(typeof(ToolTip))]
    public class ToolTipInspector : Editor {
        public static bool ShowToolTipsHelp = true;

        SerializedProperty pivotProp;
        SerializedProperty anchorProp;
        SerializedProperty labelProp;
        SerializedProperty contentParentProp;

        public override void OnInspectorGUI() {
            ShowToolTipsHelp = GUILayout.Toggle(ShowToolTipsHelp, "Show help");

            ToolTip tt = (ToolTip)target;
            Type toolTipType = tt.GetType();

            HUXEditorUtils.BeginSectionBox("Components");
            pivotProp = serializedObject.FindProperty("pivot");
            anchorProp = serializedObject.FindProperty("anchor");
            labelProp = serializedObject.FindProperty("label");
            contentParentProp = serializedObject.FindProperty("contentParent");

            //HUXEditorUtils.ToolTip(ShowToolTipsHelp, toolTipType, "pivot");
            pivotProp.objectReferenceValue = HUXEditorUtils.DropDownGameObjectField("Pivot", pivotProp.objectReferenceValue as GameObject, tt.transform);
            //HUXEditorUtils.ToolTip(ShowToolTipsHelp, toolTipType, "anchor");
            anchorProp.objectReferenceValue = HUXEditorUtils.DropDownGameObjectField("Anchor", anchorProp.objectReferenceValue as GameObject, tt.transform);
            //HUXEditorUtils.ToolTip(ShowToolTipsHelp, toolTipType, "label");
            labelProp.objectReferenceValue = HUXEditorUtils.DropDownGameObjectField("Label", labelProp.objectReferenceValue as GameObject, tt.transform);
            //HUXEditorUtils.ToolTip(ShowToolTipsHelp, toolTipType, "contentParent");
            contentParentProp.objectReferenceValue = HUXEditorUtils.DropDownGameObjectField("Content Parent", contentParentProp.objectReferenceValue as GameObject, tt.transform);
            HUXEditorUtils.EndSectionBox();

            if (pivotProp.objectReferenceValue == null || anchorProp.objectReferenceValue == null || labelProp.objectReferenceValue == null || contentParentProp.objectReferenceValue == null) {
                HUXEditorUtils.ErrorMessage("Not all components were found. You can auto-generate these components if you like.", AutoGenerateComponents, "Auto-Generate Components");
                HUXEditorUtils.SaveChanges(target, serializedObject);
                return;
            }

            HUXEditorUtils.BeginSectionBox("Content");
            SerializedProperty backgroundPaddingProp = serializedObject.FindProperty("backgroundPadding");
            SerializedProperty backgroundOffsetProp = serializedObject.FindProperty("backgroundOffset");
            SerializedProperty fontSizeProp = serializedObject.FindProperty("fontSize");

            //HUXEditorUtils.ToolTip(ShowToolTipsHelp, toolTipType, "toolTipText");
            tt.ToolTipText = EditorGUILayout.TextArea(tt.ToolTipText);
            //HUXEditorUtils.ToolTip(ShowToolTipsHelp, toolTipType, "contentScale");
            tt.ContentScale = EditorGUILayout.Slider("Content Scale", tt.ContentScale, 0.01f, 3f);
            //HUXEditorUtils.ToolTip(ShowToolTipsHelp, toolTipType, "backgroundPadding");
            backgroundPaddingProp.vector2Value = EditorGUILayout.Vector2Field("Background padding", backgroundPaddingProp.vector2Value);
            //HUXEditorUtils.ToolTip(ShowToolTipsHelp, toolTipType, "backgroundOffset");
            backgroundOffsetProp.vector3Value = EditorGUILayout.Vector3Field("Background offset", backgroundOffsetProp.vector3Value);
            HUXEditorUtils.EndSectionBox();


            HUXEditorUtils.BeginSectionBox("State");
            GUILayout.Toggle(tt.IsOn, "Is On");
            tt.TipState = (ToolTip.TipDisplayModeEnum)EditorGUILayout.EnumPopup("Tip State", tt.TipState);
            EditorGUILayout.LabelField("Group Tip State: " + tt.GroupTipState);
            EditorGUILayout.LabelField("Master Tip State: " + tt.MasterTipState);
            HUXEditorUtils.EndSectionBox();

            HUXEditorUtils.BeginSectionBox("Additional components");
            SerializedProperty lineProp = serializedObject.FindProperty("toolTipLine");
            SerializedProperty attachPointProp = serializedObject.FindProperty("attachPointType");
            

            lineProp.objectReferenceValue = HUXEditorUtils.DropDownComponentField<Line>("Line", lineProp.objectReferenceValue as Line, tt.transform, true);
            EditorGUILayout.PropertyField(attachPointProp);

            HoloToolkit.Unity.Billboard bb = tt.Pivot.GetComponent<HoloToolkit.Unity.Billboard>();
            if (bb == null)
            {
                if (GUILayout.Button ("Add default billboard to content?"))
                {
                    bb = tt.Pivot.AddComponent<HoloToolkit.Unity.Billboard>();
                    bb.PivotAxis = HoloToolkit.Unity.PivotAxis.Y;
                }
            }

            if (lineProp.objectReferenceValue == null)
            {
                Line line = tt.gameObject.GetComponent<Line>();
                if (line == null)
                {
                    if (GUILayout.Button("Add Default Line?"))
                    {
                        Line toolTipLine = tt.gameObject.AddComponent<Line>();
                        LineUnity toolTipLineRenderer = tt.gameObject.AddComponent<LineUnity>();
                        toolTipLineRenderer.Target = toolTipLine;
                    }
                }
                else
                {
                    lineProp.objectReferenceValue = line;
                }
            }
            ToolTipBackground[] backgrounds = tt.gameObject.GetComponents<ToolTipBackground>();
            if (backgrounds.Length == 0)
            {
                if (GUILayout.Button("Add Default Background?"))
                {
                    ToolTipBackgroundMesh toolTipBackground = tt.gameObject.AddComponent<ToolTipBackgroundMesh>();
                }

            }
            HUXEditorUtils.EndSectionBox();
            HUXEditorUtils.SaveChanges(target, serializedObject);
        }

        private void AutoGenerateComponents() {
            ToolTip tt = (ToolTip)target;

            if (string.IsNullOrEmpty (tt.ToolTipText))
            {
                tt.ToolTipText = "Text";
            }
            
            Transform pivotTransform = tt.transform.Find("Pivot");
            if (pivotTransform == null) {
                GameObject newPivot = new GameObject("Pivot");
                newPivot.transform.parent = tt.transform;
                newPivot.transform.localPosition = Vector3.up * 0.2f;
                newPivot.transform.localScale = Vector3.one;
                newPivot.transform.localRotation = Quaternion.identity;
                pivotTransform = newPivot.transform;
                pivotProp.objectReferenceValue = newPivot;
            }

            Transform anchorTransform = tt.transform.Find("Anchor");
            if (anchorTransform == null) {
                GameObject newAnchor = new GameObject("Anchor");
                newAnchor.transform.parent = tt.transform;
                newAnchor.transform.localPosition = Vector3.zero;
                newAnchor.transform.localScale = Vector3.one;
                newAnchor.transform.localRotation = Quaternion.identity;
                anchorTransform = newAnchor.transform;
                anchorProp.objectReferenceValue = newAnchor;
            }

            Transform contentParentTransform = pivotTransform.Find("ContentParent");
            if (contentParentTransform == null) {
                GameObject newContentParent = new GameObject("ContentParent");
                newContentParent.transform.parent = pivotTransform;
                newContentParent.transform.localPosition = Vector3.zero;
                newContentParent.transform.localScale = Vector3.one;
                newContentParent.transform.localRotation = Quaternion.identity;
                contentParentTransform = newContentParent.transform;
                contentParentProp.objectReferenceValue = newContentParent;
            }

            Transform labelTransform = contentParentTransform.Find("Label");
            if (labelTransform == null) {
                GameObject newLabel = new GameObject("Label");
                newLabel.transform.parent = contentParentTransform;
                newLabel.transform.localPosition = Vector3.zero;
                newLabel.transform.localScale = Vector3.one;
                newLabel.transform.localRotation = Quaternion.identity;
                labelTransform = newLabel.transform;
                // Add a text mesh component since we'll need it
                newLabel.AddComponent<TextMesh>();
                labelProp.objectReferenceValue = newLabel;
            }
        }

        public void OnSceneGUI()
        {
            if (Application.isPlaying)
                return;

            ToolTip tt = (ToolTip)target;
            ToolTipConnector ttc = tt.GetComponent<ToolTipConnector>();
            if (ttc == null)
            {
                ttc = tt.gameObject.AddComponent<ToolTipConnector>();
            }

            if (tt.Anchor == null || tt.ContentParentTransform == null)
                return;

            if (Event.current.type == EventType.MouseDown)
            {
                mouseDown = true;
            }
            else if (Event.current.type == EventType.MouseUp)
            {
                mouseDown = false;
                recordingUndo = false;
            }

            float edgeSize = tt.LocalContentSize.x * tt.ContentParentTransform.lossyScale.x * 0.6f;

            // If the tooltip connector will permit handle / anchor movement, do those here
            bool doPivot = false;
            bool doAnchor = false;
            switch (ttc.PivotMode)
            {
                case ToolTipConnector.PivotModeEnum.Automatic:
                    switch (ttc.FollowType)
                    {
                        case ToolTipConnector.FollowTypeEnum.AnchorOnly:
                            doPivot = true;
                            break;

                        default:
                            break;
                    }
                    break;

                case ToolTipConnector.PivotModeEnum.Manual:
                    doPivot = true;
                    break;
            }

            if (doPivot) {
                // Draw a tool for moving the pivot
                Vector3 pivotPosition = tt.PivotPosition;
                Vector3 newPivotPosition = pivotPosition;
                pivotPosition += (tt.ContentParentTransform.right * edgeSize);
                Handles.color = Color.white;
                Handles.Label(pivotPosition, "Move pivot");
                Handles.DrawDottedLine(pivotPosition, tt.PivotPosition, 2f);
                newPivotPosition = Handles.PositionHandle(pivotPosition, tt.ContentParentTransform.rotation);
                newPivotPosition = tt.PivotPosition + (newPivotPosition - pivotPosition);
                // Draw a line from the anchor handle to the actual anchor so we know what's up
                //Handles.color = Color.white;
                //Handles.DrawDottedLine(pivotPosition, tt.AnchorPosition, 2f);
                if (newPivotPosition != tt.PivotPosition)
                {
                    if (mouseDown && !recordingUndo)
                    {
                        recordingUndo = true;
                        Undo.RegisterCompleteObjectUndo(target, "Pivot");
                    }
                    tt.PivotPosition = newPivotPosition;
                    EditorUtility.SetDirty(target);
                }
                Handles.color = Color.Lerp(Color.yellow, Color.clear, 0.5f);
                Handles.DrawSphere(0, tt.PivotPosition, Quaternion.identity, tt.transform.lossyScale.x * 0.025f);

            }

            switch (ttc.FollowType)
            {
                case ToolTipConnector.FollowTypeEnum.AnchorOnly:
                    break;

                default:
                    doAnchor = true;
                    break;
            }            

            if (doAnchor)
            {
                Vector3 anchorPosition = tt.AnchorPosition;
                Vector3 newAnchorPosition = anchorPosition;
                anchorPosition += (tt.Pivot.transform.right * edgeSize);
                Handles.color = Color.white;
                Handles.Label(anchorPosition, "Move anchor");
                Handles.DrawDottedLine(anchorPosition, tt.AnchorPosition, 2f);
                newAnchorPosition = Handles.PositionHandle(anchorPosition, tt.Pivot.transform.rotation);
                newAnchorPosition = tt.Anchor.transform.position + (newAnchorPosition - anchorPosition);

                if (newAnchorPosition != tt.Anchor.transform.position)
                {
                    if (mouseDown && !recordingUndo)
                    {
                        recordingUndo = true;
                        Undo.RegisterCompleteObjectUndo(target, "Anchor");
                    }
                    tt.Anchor.transform.position = newAnchorPosition;
                    EditorUtility.SetDirty(target);
                }
                Handles.color = Color.Lerp(Color.yellow, Color.clear, 0.5f);
                Handles.DrawSphere(0, tt.AnchorPosition, Quaternion.identity, tt.transform.lossyScale.x * 0.025f);

            }
        }

        private bool recordingUndo = false;
        private bool mouseDown = false;
    }
}