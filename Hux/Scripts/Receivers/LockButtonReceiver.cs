using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HUX.Interaction;
using HUX.Receivers;

public class LockButtonReceiver : InteractionReceiver
{
    public ManipulationManager manipulationManager;

    protected override void OnTapped(GameObject obj, InteractionManager.InteractionEventArgs eventArgs)
    {
        BoundingBoxManipulateWithLock boundingBoxManipulateWithLock = (BoundingBoxManipulateWithLock)manipulationManager.ActiveBoundingBox;
        boundingBoxManipulateWithLock.LockBoundingBox();
        base.OnTapped(obj, eventArgs);
    }
}
