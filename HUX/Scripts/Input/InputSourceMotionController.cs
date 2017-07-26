// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using System.Collections.Generic;

#if UNITY_WSA
using UnityEngine.VR.WSA.Input;
#endif

public class InputSourceMotionController : InputSourceSixDOFBase
{
	// Interaction Manager Input
	private Dictionary<uint, Transform> imDevices = new Dictionary<uint, Transform>();

	// Visuals
	public GameObject DevicePrefabLeft;
	public GameObject DevicePrefabRight;
	public Transform ControllersRoot;

	/// <summary>
    /// Controller state
    /// </summary>
	public class ControllerState
	{
		public Vector3 position;
		public Quaternion rotation;

		public ButtonControlState selectButton = new ButtonControlState();
		public ButtonControlState menuButton = new ButtonControlState();
		public ButtonControlState graspButton = new ButtonControlState();
		public ButtonControlState padTouch = new ButtonControlState();
		public ButtonControlState padButton = new ButtonControlState();
		public ButtonControlState tumbstickButton = new ButtonControlState();

		public Vector2ControlState selectGrasp = new Vector2ControlState();
		public Vector2ControlState joystick = new Vector2ControlState();
		public Vector2ControlState pad = new Vector2ControlState();

		public Transform Transform;
	}

	// State for each controller.  Need to add logic here to decide which targeting ray to use?
	// Or create a focuser for both?
	public ControllerState LeftController = new ControllerState();
	public ControllerState RightController = new ControllerState();

    // Subscribe to the controller events
    private void Awake()
    {
#if UNITY_WSA
        InteractionManager.SourceDetected += WSASourceDetected;
        InteractionManager.SourceLost += WSASourceLost;
        InteractionManager.SourceUpdated += WSASourceUpdate;

        InteractionManager.SourcePressed += WSAPressed;
        InteractionManager.SourceReleased += WSAReleased;

  		// Add any already detected devices to the list
		AddDevicesFromCurrentReading(InteractionManager.GetCurrentReading());
#endif
    }

    // Cleanup controller event subscriptions
    private void OnDestroy()
    {
#if UNITY_WSA
        InteractionManager.SourceDetected -= WSASourceDetected;
        InteractionManager.SourceLost -= WSASourceLost;
        InteractionManager.SourceUpdated -= WSASourceUpdate;

        InteractionManager.SourcePressed -= WSAPressed;
        InteractionManager.SourceReleased -= WSAReleased;

        // Remove tracked devices
        foreach (var sourceState in InteractionManager.GetCurrentReading())
        {
            uint id = sourceState.source.id;
            this.RemoveDevice(id);
        }
#endif
    }

#if UNITY_WSA

    private void WSAPressed(InteractionManager.SourceEventArgs eventArgs)
    {
        Debug.Log(eventArgs.pressKind + " press on controller id: " + eventArgs.state.source.id);
    }

    private void WSAReleased(InteractionManager.SourceEventArgs eventArgs)
    {
        Debug.Log(eventArgs.pressKind + " press on controller id: " + eventArgs.state.source.id);
    }

    private void WSASourceUpdate(InteractionManager.SourceEventArgs eventArgs)
    {
        this.UpdateInteractionSourceState(eventArgs.state);
    }

    private void WSASourceLost(InteractionManager.SourceEventArgs eventArgs)
    {
        uint id = eventArgs.state.source.id;

        this.RemoveDevice(id);
    }

    private void WSASourceDetected(InteractionManager.SourceEventArgs eventArgs)
    {
        uint id = eventArgs.state.source.id;
		this.AddDevice(id, eventArgs.state.source.handedness);
    }

    private void AddDevice(uint id, InteractionSourceHandedness handedness)
    {
        if (!imDevices.ContainsKey(id))
        {
			Debug.Log("AddDevice: " + id + ", InteractionSourceHandedness: " + handedness + ", int: " + (int)handedness);

			GameObject go = CreateDevicePrefab(ControllersRoot, handedness != InteractionSourceHandedness.Right);
			go.name = "Controller " + id;
            imDevices[id] = go.transform;
        }
    }

    private void RemoveDevice(uint id)
    {
        if (imDevices.ContainsKey(id))
        {
            Destroy(imDevices[id].gameObject);
            imDevices.Remove(id);			
        }

        // this.console.UpdateProperty("Detected Motion Controllers", string.Format("{0}", this.imDevices.Keys.Count));
    }

	//=====================================================================
	// Common
	//=====================================================================


	GameObject CreateDevicePrefab(Transform root, bool bLeftHand)
	{
		GameObject prefab = bLeftHand ? DevicePrefabLeft : DevicePrefabRight;
		GameObject result = null;
		if( prefab != null )
		{
            result = Instantiate(prefab, root) as GameObject;
		}
		return result;
	}

	void AddDevicesFromCurrentReading(InteractionSourceState[] reading)
	{
		foreach (var sourceState in InteractionManager.GetCurrentReading())
		{
			uint id = sourceState.source.id;

			if (id != 0)
				//if (sourceState.source.supportsPointing)
				//sourceState.source.handedness != InteractionSourceHandedness.Unknown)
			{
				this.AddDevice(id, sourceState.source.handedness);
			}
		}
	}

	//ToDo gather and move interation common to better place
	private void UpdateInteractionSourceState(InteractionSourceState sourceState)
	{
		uint id = sourceState.source.id;

		ControllerState controllerState = (sourceState.source.handedness != InteractionSourceHandedness.Right)
			? LeftController : RightController;

		// Get the incoming device
		if (imDevices.ContainsKey(id))
		{
			var sourcePose = sourceState.sourcePose;
			Vector3 position = Vector3.zero;
			Quaternion rotation = Quaternion.identity;

			if (sourcePose.TryGetPosition(out position) &&
				sourcePose.TryGetRotation(out rotation))
			{
				SetTransform(imDevices[id], position, rotation);

				controllerState.position = position;
				controllerState.rotation = rotation;
				controllerState.Transform = imDevices[id];
			}
		}


		// Buttons
		controllerState.selectButton.ApplyState(sourceState.selectPressed);
		controllerState.menuButton.ApplyState(sourceState.menuPressed);
		controllerState.graspButton.ApplyState(sourceState.grasped);
		controllerState.padButton.ApplyState(sourceState.controllerProperties.touchpadPressed);
		controllerState.padTouch.ApplyState(sourceState.controllerProperties.touchpadTouched);
		controllerState.tumbstickButton.ApplyState(sourceState.controllerProperties.thumbstickPressed);

		// Axes (plus buttons)
		controllerState.selectGrasp.ApplyPos(
			sourceState.selectPressed,
			new Vector2((float)sourceState.selectPressedValue, 0)); // Where is grasped amount??

		controllerState.joystick.ApplyPos(
			sourceState.controllerProperties.thumbstickPressed,
			new Vector2((float)sourceState.controllerProperties.thumbstickX,
			(float)sourceState.controllerProperties.thumbstickY));

		controllerState.pad.ApplyPos(
			sourceState.controllerProperties.touchpadPressed,
			new Vector2((float)sourceState.controllerProperties.touchpadX,
			(float)sourceState.controllerProperties.touchpadY));
	}

	private void SetTransform(Transform t, Vector3 position, Quaternion rotation)
	{
		t.localPosition = position;
		t.localRotation = rotation;
	}
#endif  // UNITY_WSA

    public override Vector3 position
	{
		get
		{
			return RightController.position;
		}
	}

	public override Quaternion rotation
	{
		get
		{
			return RightController.rotation;
		}
	}

	// Buttons come from InputSourceNetMouse
	public override bool buttonSelectPressed
	{
		get
		{
			return RightController.selectButton.pressed || LeftController.selectButton.pressed;
		}
	}

	public override bool buttonMenuPressed
	{
		get
		{
			return RightController.menuButton.pressed || LeftController.menuButton.pressed;

		}
	}

	public override bool buttonAltSelectPressed
	{
		get
		{
			return false;
		}
	}
}

