using System.Collections.Generic;
using Player;
using UnityEngine;

public class TripWireRope : MonoBehaviour
{
    public List<TripWirePole> connectedPoles = new List<TripWirePole>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    private void OnTriggerEnter(Collider other)
    {
        var playerRole = other.GetComponent<PlayerRole>();
        if (playerRole == null || playerRole.Role == PlayerRole.RoleType.Hider)
            return;
        
        foreach (var pole in connectedPoles)
        {
            if (pole != null)
            {
                pole.OnRopeTriggered();
            }
           
        }
    }
    
    public void ClearConnectedPoles()
    {
        connectedPoles.Clear();
    }
    
    public void SetConnectedPoles(TripWirePole pole1, TripWirePole pole2)
    {
        if (!connectedPoles.Contains(pole1))
            connectedPoles.Add(pole1);
            
        if (!connectedPoles.Contains(pole2))
            connectedPoles.Add(pole2);
    }
}
