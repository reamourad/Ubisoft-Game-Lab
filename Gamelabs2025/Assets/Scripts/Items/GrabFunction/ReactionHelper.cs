using System;
using System.Collections.Generic;
using NUnit.Framework.Constraints;
using UnityEngine;

public class ReactionHelper : MonoBehaviour
{
    public IReactionItem reactionItem;
    private bool isGrabbed = false;
    public Vector3 detectionHalfExtent = new Vector3(15f, 2.5f, 15f);
    public LayerMask detectionLayerMask;
    public GameObject reactionArea;
    private List<Collider> collidersCache;

    public void OnGrabbed()
    {
        isGrabbed = true;
    }

    public void OnReleased()
    {
        isGrabbed = false;
    }

    public void LateUpdate()
    {
        if (isGrabbed)
        {
            foreach (Collider col in collidersCache)
            {
                //check if the items are still in range
                float distance = Vector3.Distance(transform.position, col.gameObject.transform.position);
                if (distance > detectionHalfExtent.magnitude)
                {
                    var triggerHelper = col.GetComponent<TriggerHelper>();
                    if (triggerHelper != null)
                    {
                        triggerHelper.HideTriggerArea();
                    }
                }
            }
            
            Collider[] colliders =Physics.OverlapBox(gameObject.transform.position, detectionHalfExtent, Quaternion.identity, detectionLayerMask);
            foreach (Collider col in colliders)
            {
                if (collidersCache.Contains(col)) continue;
                
                var triggerHelper = col.GetComponent<TriggerHelper>();
                //check if there is a trigger item nearby
                if (triggerHelper != null)
                {
                    triggerHelper.ShowTriggerArea();
                }
            }
        }
    }

    private void ShowReactionArea()
    {
        reactionArea.SetActive(true);
    }
        
    private void HideReactionArea()
    {
        reactionArea.SetActive(false);
    }
}
