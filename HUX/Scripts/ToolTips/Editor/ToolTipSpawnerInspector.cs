using HUX;
using UnityEditor;
using UnityEngine;

namespace MRDL.ToolTips
{
    [CustomEditor(typeof(ToolTipSpawner))]
    public class ToolTipSpawnerInspector : Editor {

        public override void OnInspectorGUI()
        {
            ToolTipSpawner tts = (ToolTipSpawner)target;

            HUXEditorUtils.BeginSectionBox("Tooltip settings");

            tts.ToolTipText = EditorGUILayout.TextField("Text", tts.ToolTipText);
            tts.ToolTipPrefab = (GameObject)EditorGUILayout.ObjectField("Prefab", tts.ToolTipPrefab, typeof(GameObject), false);
            if (tts.ToolTipPrefab == null)
            {
                HUXEditorUtils.SaveChanges(target, serializedObject);
                HUXEditorUtils.ErrorMessage("You must select a tool tip prefab.");
                HUXEditorUtils.EndSectionBox();
                return;
            }

            HUX.Interaction.InteractibleObject io = tts.GetComponent<HUX.Interaction.InteractibleObject>();
            if (io == null)
            {
                HUXEditorUtils.WarningMessage("ToolTipSpawner will not work without an accompanying component that inherits from InteractibleObject.", "Fix now", AddInteractibleObject);
            }

            HUXEditorUtils.BeginSubSectionBox("Behavior");
            tts.AppearBehavior = (ToolTipSpawner.AppearBehaviorEnum)EditorGUILayout.EnumPopup("Appear behavior", tts.AppearBehavior);
            if (tts.AppearBehavior == ToolTipSpawner.AppearBehaviorEnum.AppearOnFocusEnter)
            {
                tts.AppearDelay = EditorGUILayout.Slider("Appear Delay", tts.AppearDelay, 0f, 5f);
            }
            tts.VanishBehavior = (ToolTipSpawner.VanishBehaviorEnum)EditorGUILayout.EnumPopup("Vanish behavior", tts.VanishBehavior);
            if (tts.VanishBehavior == ToolTipSpawner.VanishBehaviorEnum.VanishOnFocusExit)
            {
                tts.VanishDelay = EditorGUILayout.Slider("Vanish Delay", tts.VanishDelay, 0f, 5f);
            }
            HUXEditorUtils.EndSubSectionBox();

            HUXEditorUtils.BeginSubSectionBox("Positioning");
            tts.Anchor = HUXEditorUtils.DropDownComponentField<Transform>("Anchor", tts.Anchor, tts.transform, false);

            if (tts.Anchor == null) {
                HUXEditorUtils.WarningMessage("'" + target.name + "' transform will be used as the anchor. (This may be intentional.)");
            }
            tts.PivotDirectionOrient = (ToolTipConnector.OrientTypeEnum)EditorGUILayout.EnumPopup("Position mode", tts.PivotDirectionOrient);
            tts.PivotMode = (ToolTipConnector.PivotModeEnum)EditorGUILayout.EnumPopup("Pivot mode", tts.PivotMode);
            if (tts.PivotMode == ToolTipConnector.PivotModeEnum.Manual) {
                tts.ManualPivotLocalPosition = EditorGUILayout.Vector3Field("Manual position", tts.ManualPivotLocalPosition);
            } else {
                tts.FollowType = (ToolTipConnector.FollowTypeEnum)EditorGUILayout.EnumPopup("Follow type", tts.FollowType);

                GUIStyle buttonStyle = new GUIStyle(EditorStyles.toolbarButton);
                float buttonSize = 35f;
                buttonStyle.fontSize = 25;
                buttonStyle.fontStyle = FontStyle.Bold;
                buttonStyle.fixedWidth = buttonSize;
                buttonStyle.fixedHeight = buttonSize;
                Color selectedColor = HUXEditorUtils.SuccessColor;
                Color unselectedColor = HUXEditorUtils.DefaultColor;

                GUIStyle middleStyle = new GUIStyle(EditorStyles.helpBox);
                middleStyle.alignment = TextAnchor.UpperCenter;
                middleStyle.stretchWidth = false;

                EditorGUILayout.LabelField("Spawn position:");
                EditorGUILayout.BeginHorizontal(middleStyle);
                GUILayout.FlexibleSpace();
                EditorGUILayout.BeginVertical();
                EditorGUILayout.BeginHorizontal();

                GUI.color = (tts.PivotDirection == ToolTipConnector.PivotDirectionEnum.NorthWest) ? selectedColor : unselectedColor;
                if (GUILayout.Button("↖", buttonStyle)) {
                    tts.PivotDirection = ToolTipConnector.PivotDirectionEnum.NorthWest;
                }
                GUI.color = (tts.PivotDirection == ToolTipConnector.PivotDirectionEnum.North) ? selectedColor : unselectedColor;
                if (GUILayout.Button("⇧", buttonStyle)) {
                    tts.PivotDirection = ToolTipConnector.PivotDirectionEnum.North;
                }
                GUI.color = (tts.PivotDirection == ToolTipConnector.PivotDirectionEnum.NorthEast) ? selectedColor : unselectedColor;
                if (GUILayout.Button("↗", buttonStyle)) {
                    tts.PivotDirection = ToolTipConnector.PivotDirectionEnum.NorthEast;
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                GUI.color = (tts.PivotDirection == ToolTipConnector.PivotDirectionEnum.West) ? selectedColor : unselectedColor;
                if (GUILayout.Button("⇦", buttonStyle)) {
                    tts.PivotDirection = ToolTipConnector.PivotDirectionEnum.West;
                }
                GUI.color = (tts.PivotDirection == ToolTipConnector.PivotDirectionEnum.InFront) ? selectedColor : unselectedColor;
                if (GUILayout.Button("•", buttonStyle)) {
                    tts.PivotDirection = ToolTipConnector.PivotDirectionEnum.InFront;
                }
                GUI.color = (tts.PivotDirection == ToolTipConnector.PivotDirectionEnum.East) ? selectedColor : unselectedColor;
                if (GUILayout.Button("⇨", buttonStyle)) {
                    tts.PivotDirection = ToolTipConnector.PivotDirectionEnum.East;
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                GUI.color = (tts.PivotDirection == ToolTipConnector.PivotDirectionEnum.SouthWest) ? selectedColor : unselectedColor;
                if (GUILayout.Button("↙", buttonStyle)) {
                    tts.PivotDirection = ToolTipConnector.PivotDirectionEnum.SouthWest;
                }
                GUI.color = (tts.PivotDirection == ToolTipConnector.PivotDirectionEnum.South) ? selectedColor : unselectedColor;
                if (GUILayout.Button("⇩", buttonStyle)) {
                    tts.PivotDirection = ToolTipConnector.PivotDirectionEnum.South;
                }
                GUI.color = (tts.PivotDirection == ToolTipConnector.PivotDirectionEnum.SouthEast) ? selectedColor : unselectedColor;
                if (GUILayout.Button("↘", buttonStyle)) {
                    tts.PivotDirection = ToolTipConnector.PivotDirectionEnum.SouthEast;
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                GUI.color = (tts.PivotDirection == ToolTipConnector.PivotDirectionEnum.Manual) ? selectedColor : unselectedColor;
                if (GUILayout.Button("Custom", EditorStyles.toolbarButton, GUILayout.MinWidth(buttonSize * 3), GUILayout.MaxWidth(buttonSize * 3))) {
                    tts.PivotDirection = ToolTipConnector.PivotDirectionEnum.Manual;
                }
                GUI.color = HUXEditorUtils.DefaultColor;
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();

                if (tts.PivotDirection == ToolTipConnector.PivotDirectionEnum.Manual) {
                    tts.ManualPivotDirection = EditorGUILayout.Vector3Field("Manual Direction", tts.ManualPivotDirection).normalized;
                }
                tts.PivotDistance = EditorGUILayout.Slider("Spawn Distance", tts.PivotDistance, 0.001f, 3f);
            }

            HUXEditorUtils.EndSubSectionBox();

            HUXEditorUtils.EndSectionBox();

            HUXEditorUtils.SaveChanges(target, serializedObject);
        }

        private void AddInteractibleObject()
        {
            ToolTipSpawner tts = (ToolTipSpawner)target;
            HUX.Interaction.InteractibleObject io = tts.gameObject.AddComponent<HUX.Interaction.InteractibleObject>();
        }
    }
}
