using System;
using FishNet.Object;
using GogoGaga.OptimizedRopesAndCables;
using UnityEngine;

public class TripWirePole :NetworkBehaviour, ITriggerItem, IHiderGrabableItem
{
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Rope rope { get; set; }
    public event Action<ITriggerItem> OnTriggerActivated;
}
