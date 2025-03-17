using FishNet.Object;
using UnityEngine;

public class SeekerWorldDummy : NetworkBehaviour
{
    [SerializeField] private NetworkObject itemReference;
    [SerializeField] private Sprite icon;
    
    [SerializeField] private GameObject highlight;
    
    public Sprite Icon => icon;
    public NetworkObject ItemReference => itemReference;

    public void Highlight(bool show)
    {
        if(highlight)
            highlight.SetActive(show);
    }

    public void OnPickedUp()
    {
        RPC_ServerDummyPickedUp();
    }

    [ServerRpc(RequireOwnership = false)]
    public void RPC_ServerDummyPickedUp()
    {
        Despawn();
    }
}
