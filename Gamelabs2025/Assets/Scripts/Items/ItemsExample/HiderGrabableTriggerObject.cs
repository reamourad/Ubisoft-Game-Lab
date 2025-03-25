using System;
using GogoGaga.OptimizedRopesAndCables;
using UnityEngine;

public class HiderGrabableTriggerObject : MonoBehaviour, IHiderGrabableItem, ITriggerItem
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            OnTriggerActivated?.Invoke(this);
        }
    }
    

    public Rope rope { get; set; }

    public event Action<ITriggerItem> OnTriggerActivated;
}
