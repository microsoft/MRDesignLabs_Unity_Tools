//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
using HUX.Interaction;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HUX
{
    [CustomEditor(typeof(InteractionManager))]
    public class InteractionManagerInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            InteractionManager interactionManager = (InteractionManager)target;

            interactionManager.RecognizableGesures = (UnityEngine.VR.WSA.Input.GestureSettings)HUXEditorUtils.EnumCheckboxField<UnityEngine.VR.WSA.Input.GestureSettings>(
                "Recognizable gestures",
                interactionManager.RecognizableGesures,
                "Default",
                UnityEngine.VR.WSA.Input.GestureSettings.Tap |
                UnityEngine.VR.WSA.Input.GestureSettings.DoubleTap |
                UnityEngine.VR.WSA.Input.GestureSettings.Hold | 
                UnityEngine.VR.WSA.Input.GestureSettings.NavigationX | 
                UnityEngine.VR.WSA.Input.GestureSettings.NavigationY);

            EditorGUILayout.BeginHorizontal();
            interactionManager.SendTapToGlobalReceiver = EditorGUILayout.Toggle("Send Tap to GlobalGestureReceiver", interactionManager.SendTapToGlobalReceiver);
            EditorGUILayout.EndHorizontal();
            if (interactionManager.SendTapToGlobalReceiver && (interactionManager.RecognizableGesures & UnityEngine.VR.WSA.Input.GestureSettings.Tap) == 0)
                interactionManager.SendTapToGlobalReceiver = false;

            EditorGUILayout.BeginHorizontal();
            interactionManager.SendDoubleTapToGlobalReceiver = EditorGUILayout.Toggle("Send Double Tap to GlobalGestureReceiver", interactionManager.SendDoubleTapToGlobalReceiver);
            EditorGUILayout.EndHorizontal();
            if (interactionManager.SendDoubleTapToGlobalReceiver && (interactionManager.RecognizableGesures & UnityEngine.VR.WSA.Input.GestureSettings.DoubleTap) == 0)
                interactionManager.SendDoubleTapToGlobalReceiver = false;

            EditorGUILayout.BeginHorizontal();
            interactionManager.GlobalGestureReceiver = (GameObject)EditorGUILayout.ObjectField("Global Gesture Receiver", interactionManager.GlobalGestureReceiver, typeof(GameObject), true);
            EditorGUILayout.EndHorizontal();

            HUXEditorUtils.SaveChanges(target);
        }
    }
}
