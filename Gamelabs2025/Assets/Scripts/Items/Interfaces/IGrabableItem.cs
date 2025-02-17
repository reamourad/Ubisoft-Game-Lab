using UnityEngine;

public interface IGrabableItem
{
    void OnGrab(PlayerController grabber);
    void OnDrop();
    void OnThrow();
}
