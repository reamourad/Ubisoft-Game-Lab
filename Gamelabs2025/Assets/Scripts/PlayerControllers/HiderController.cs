using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class HiderController : PlayerController
{
    [SerializeField] private int grabRange;
    
    public Transform grabPlacement; 
    private IGrabableItem lookingAtObject = null; 
    private IGrabableItem grabbedObject = null; 
    [SerializeField] private InScreenUI inScreenUI;
    
    

    // Update is called once per frame
    void FixedUpdate()
    {
        base.FixedUpdate();

        lookingAtObject = null;
        //TODO: @Rea this is shit please redo everything tomrrow thanks!
        // Raycast from the center of the camera's view

        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit hit, grabRange))
        {
            lookingAtObject = hit.collider.GetComponent<IGrabableItem>();
        }

        // Check if the object has the IGrabbable interface

        if (lookingAtObject != null)
        {
            inScreenUI.toolTipText.gameObject.SetActive(true);
            inScreenUI.toolTipText.text = "Press " +
                                          InputReader.GetCurrentBindingText(InputReader.Instance.inputMap.Gameplay
                                              .Grab) + " to grab  " + lookingAtObject.gameObject.name;
        }
        else
        {
            inScreenUI.toolTipText.gameObject.SetActive(false);
        }
    }
    
    //when a item is hit and is not equal to previous item, 
    //if it goes to null 

    public override void OnGrab()
    {
        if (lookingAtObject == null) { return; }

        if (grabbedObject == null)
        {
            grabbedObject = lookingAtObject;
            grabbedObject.gameObject.transform.position = grabPlacement.position;
            grabbedObject.gameObject.transform.SetParent(grabPlacement);
        }
        else
        {
            grabbedObject.gameObject.transform.SetParent(null);
            grabbedObject.gameObject.transform.position = lookingAtObject.gameObject.transform.position;

            grabbedObject = lookingAtObject;
            grabbedObject.gameObject.transform.position = grabPlacement.position;
            grabbedObject.gameObject.transform.SetParent(grabPlacement);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(playerCamera.transform.position, playerCamera.transform.forward * grabRange);
    }
}
