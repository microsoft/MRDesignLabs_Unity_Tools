using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HUX.Interaction;

public class BoundingBoxManipulateWithLock :  BoundingBoxManipulate{

    public GameObject ColliderObject;

    public void LockBoundingBox()
    {
        if (ColliderObject.activeSelf)
        {
            ColliderObject.SetActive(false);
        }
        else
        {
            ColliderObject.SetActive(true);
        }        
    }    
}
