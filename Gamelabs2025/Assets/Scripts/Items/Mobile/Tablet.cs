using Items.Interfaces;
using Unity.VisualScripting;
using UnityEngine;

namespace Items
{
    public class Tablet : MonoBehaviour, IUsableItem
    {
        private CameraPreviewer cameraPreviewer;
        public void UseItem(bool isUsing)
        {
            if (isUsing && cameraPreviewer == null)
            {
                cameraPreviewer = Instantiate(Resources.Load("Camera/CameraViewer")).GetComponent<CameraPreviewer>();
                cameraPreviewer.Open();
            }
        }
    }
   
}