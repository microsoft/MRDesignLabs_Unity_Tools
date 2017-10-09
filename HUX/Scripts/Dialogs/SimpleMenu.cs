//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
using HUX.Receivers;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HUX.Dialogs
{
    [Serializable]
    public abstract class SimpleMenuButton
    {
        public string Name;
        public int Index;
        public InteractionReceiver Target;

        public bool IsEmpty
        {
            get
            {
                return string.IsNullOrEmpty(Name);
            }
        }
    }

    /// <summary>
    /// Base class for menu that automatically generates buttons from a template
    /// </summary>
    public abstract class SimpleMenu<T> : MonoBehaviour where T : SimpleMenuButton
    {
        /// <summary>
        /// How many buttons can be added to the menu
        /// </summary>
        [Range(1,64)]
        public int MaxButtons = 10;

        public GameObject ButtonPrefab;

        public virtual T[] Buttons
        {
            get
            {
                return buttons;
            }
        }

        #if UNITY_EDITOR
        /// <summary>
        /// Used by inspectors
        /// </summary>
        public virtual void EditorRefreshButtons()
        {
            if (buttons == null)
                 buttons = new T[MaxButtons];
            else if (buttons.Length != MaxButtons)
                Array.Resize<T>(ref buttons, MaxButtons);
        }
        #endif

        [SerializeField]
        protected Transform buttonParent;

        [SerializeField]
        protected T[] buttons;

        protected GameObject[] instantiatedButtons;

        protected virtual void OnEnable()
        {
            if (buttons == null)
                buttons = new T[MaxButtons];
            else if (buttons.Length != MaxButtons)
                Array.Resize<T>(ref buttons, MaxButtons);

            GenerateButtons();
        }

        protected virtual GameObject CreateButton(T template)
        {
            GameObject newButton = GameObject.Instantiate(ButtonPrefab, buttonParent);
            newButton.name = template.Name;
            newButton.transform.localPosition = Vector3.zero;
            newButton.transform.localRotation = Quaternion.identity;

            // Register the button with the interaction receiver, if it's set
            if (template.Target != null)
                template.Target.RegisterInteractible(newButton);

            return newButton;
        }

        protected virtual void GenerateButtons ()
        {
            if (instantiatedButtons != null)
                return;

            List<GameObject> instantiatedButtonsList = new List<GameObject>();
            int buttonIndex = 0;
            for (int i = 0; i < buttons.Length; i++)
            {
                if (!buttons[i].IsEmpty)
                {
                    buttons[i].Index = buttonIndex;
                    buttonIndex++;
                    instantiatedButtonsList.Add(CreateButton(buttons[i]));
                }
            }
            instantiatedButtons = instantiatedButtonsList.ToArray();
        }
    }
}