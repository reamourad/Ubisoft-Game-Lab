using UnityEngine;

public abstract class Item : MonoBehaviour
{
    
    public abstract string itemName { get; }
    public abstract string itemDescription { get; }
    
    //any item in the game can be triggered 
    public abstract void OnTrigger(); 
}
