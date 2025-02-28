using UnityEngine;

public interface IReactionItem
{
    public Transform WireAnchor { get; }
    public void OnTrigger();
}
