using System;
using GogoGaga.OptimizedRopesAndCables;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class ReactionItemImpl : MonoBehaviour, IReactionItem
{
    [SerializeField] private Transform wireAnchor;
    private Renderer meshRenderer;
    private Material originalMaterial;
    [SerializeField] Material GhostMaterial;

    /*private void OnEnable()
    {
        HiderController.OnHoldingWireItem += HandlePlayerHoldingWire;
    }

    private void OnDisable()
    {
        HiderController.OnHoldingWireItem -= HandlePlayerHoldingWire;

    }*/

    private void Start()
    {
        meshRenderer = GetComponent<Renderer>();
        originalMaterial = meshRenderer.material;
    }

    public Transform WireAnchor => wireAnchor;

    public void OnTrigger()
    {

    }

    private void HandlePlayerHoldingWire(bool isHolding)
    {
        meshRenderer.material = isHolding ? GhostMaterial : originalMaterial;
    }

    public Rope rope { get; set; }
    public void OnTrigger(ITriggerItem triggerItem)
    {
        Debug.Log("I reacted to the object trigger.");
    }
}