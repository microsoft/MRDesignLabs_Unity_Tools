//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
using HUX.Buttons;
using UnityEditor;
using UnityEngine;

namespace HUX
{
    [CustomEditor(typeof(CompoundButtonSounds))]
    public class CompoundButtonSoundsInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            CompoundButtonSounds soundButton = (CompoundButtonSounds)target;

            GUI.color = Color.white;
            soundButton.Profile = HUXEditorUtils.DrawProfileField<ButtonSoundProfile>(soundButton.Profile);

            if (soundButton.Profile == null)
            {
                HUXEditorUtils.SaveChanges(target);
                return;
            }

            HUXEditorUtils.DrawProfileInspector(soundButton.Profile, soundButton);

            HUXEditorUtils.SaveChanges(target, soundButton.Profile);
        }
    }
}
