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
        
        RaycastHit hit;
        //TODO: @Rea this is shit please redo everything tomrrow thanks!
        // Raycast from the center of the camera's view
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, grabRange))
        {
            // Check if the object has the IGrabbable interface
            IGrabableItem grabbableObject = hit.collider.GetComponent<IGrabableItem>();
            
            //check if its the same object
            if (grabbableObject == null)
            {
                lookingAtObject = null;
                inScreenUI.toolTipText.text = "";
            }
            
            else if (lookingAtObject != grabbableObject)
            {
                lookingAtObject = grabbableObject;
                //here I would add the grab event but since player input controller is separate idk 
                inScreenUI.toolTipText.text = "Press " + InputReader.GetCurrentBindingText(InputReader.Instance.inputMap.Gameplay.Grab) + " to grab  " + grabbableObject.gameObject.name;
            }

        }
    }
    
    //when a item is hit and is not equal to previous item, 
    //if it goes to null 

    public override void OnGrab()
    {
        if (lookingAtObject != null)
        {
            if (grabbedObject == null)
            {
                grabbedObject = lookingAtObject;
                grabbedObject.gameObject.transform.position = grabPlacement.position;
                this.transform.SetParent(grabPlacement);
            }
            else
            {
                //handle previous grabbed object 
                grabbedObject.gameObject.transform.SetParent(null);
                grabbedObject.gameObject.transform.position = this.transform.position;
    
                grabbedObject = lookingAtObject;
                grabbedObject.gameObject.transform.position = grabPlacement.position;
                this.transform.SetParent(grabPlacement);

                Debug.Log("The object are switched");
            }
        }
    }
}
