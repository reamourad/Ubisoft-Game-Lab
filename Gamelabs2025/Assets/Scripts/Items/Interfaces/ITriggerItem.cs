using Items.Interfaces;
using UnityEngine;

public interface ITriggerItem : IConnectable
{

    // Event that will be raised when trigger criteria is met
    event System.Action<ITriggerItem> OnTriggerActivated;

}