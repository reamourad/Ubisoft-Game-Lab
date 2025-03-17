using Items.Interfaces;
using UnityEngine;

public interface IReactionItem : IConnectable
{
    public Transform WireAnchor { get; }
    public void OnTrigger();
}
