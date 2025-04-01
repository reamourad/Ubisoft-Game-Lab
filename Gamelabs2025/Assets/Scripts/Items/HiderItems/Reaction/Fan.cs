using FishNet.Object;
using GogoGaga.OptimizedRopesAndCables;
using Player.Data;
using UnityEngine;

namespace Player.Items.HiderItems.Reaction
{
    public class Fan: NetworkBehaviour, IReactionItem, IHiderGrabableItem
    {
        [SerializeField] private StationaryEffect effects;
        [SerializeField] private float fxKillTime = 5;
        [SerializeField] private GameObject wind;
        [SerializeField] private float range;
        public Rope rope { get; set; }
        public void OnTrigger(ITriggerItem triggerItem)
        {
            Debug.Log("Wind Triggered");
            OnServerBlow();
        }
        
        [Server]
        private void OnServerBlow()
        {
            /*RPC_OnClientExplode();
            ApplyWindEffect();
            StartCoroutine(DelayedDespawn());*/
        }
        
        [Server]
        private void ApplyWindEffect()
        {
            /*var colliders = Physics.OverlapBox(transform.position, range);
            foreach (var collider in colliders)
            {
                if (ValidCollider(collider, out StationaryObjectBase stationaryObject))
                {
                    stationaryObject.ApplyStationaryEffect(effects);
                }
            }*/
        }
    }
}