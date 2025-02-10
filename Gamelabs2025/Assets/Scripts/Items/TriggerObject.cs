using System;
using UnityEngine;

public class TriggerObject : InteractiveItem
{
    public override string itemName => "Simple Trigger Object";
    public override string itemDescription => "Used for testing trigger objects.";



    public override void OnTrigger()
    {
        Debug.Log("The object is triggered.");
    }
}
