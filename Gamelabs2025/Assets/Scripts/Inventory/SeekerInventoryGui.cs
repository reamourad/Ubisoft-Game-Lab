using UnityEngine;
using UnityEngine.UI;

namespace Player.Inventory
{
    [System.Serializable]
    public class SeekerInventoryGuiContainer
    {
        public Transform ItemParent;
        public Image Icon;
        public GameObject Selector;
    }
    public class SeekerInventoryGui : MonoBehaviour
    {
        [SerializeField]
        private SeekerInventoryGuiContainer[] Inventory = new SeekerInventoryGuiContainer[2];
        private GameObject currentSelector;
        
        public void SetItemData(int id, Sprite icon)
        {
            Inventory[id].ItemParent.gameObject.SetActive(true);
            Inventory[id].Icon.sprite = icon;
        }

        public void RemoveItemData(int id)
        {
            Inventory[id].ItemParent.gameObject.SetActive(false);
        }

        public void Equip(int id)
        {
            if(currentSelector != null)
                currentSelector.SetActive(false);
            
            Inventory[id].Selector.SetActive(true);
            currentSelector = Inventory[id].Selector;
        }
    }
}