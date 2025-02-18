using UnityEngine;

public class HiderController : PlayerController
{
    [SerializeField] private int grabRange;
    
    public Transform grabPlacement; 
    private IGrabableItem lookingAtObject = null; 
    private IGrabableItem grabbedObject = null; 
    

    // Update is called once per frame
    void FixedUpdate()
    {
        base.FixedUpdate();
        
        RaycastHit hit;
        
        // Raycast from the center of the camera's view
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, grabRange))
        {
            // Check if the object has the IGrabbable interface
            IGrabableItem grabbableObject = hit.collider.GetComponent<IGrabableItem>();
            
            if (grabbableObject != null)
            {
                Debug.Log("Grabbing " + grabbableObject.gameObject.name);
            }
        }
    }

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
