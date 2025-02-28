using System;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class ReactionItemImpl : MonoBehaviour, IReactionItem
{
    [SerializeField] private Transform wireAnchor;
    private Renderer renderer;
    private Material originalMaterial;
    [SerializeField] Material GhostMaterial;

    private void OnEnable()
    {
        HiderController.OnHoldingWireItem += HandlePlayerHoldingWire;
    }

    private void OnDisable()
    {
        HiderController.OnHoldingWireItem -= HandlePlayerHoldingWire;

    }

    private void Start()
    {
        renderer = GetComponent<Renderer>();
        originalMaterial = renderer.material;
    }

    public Transform WireAnchor => wireAnchor;

    public void OnTrigger()
    {

    }

    private void HandlePlayerHoldingWire(bool isHolding)
    {
        renderer.material = isHolding ? GhostMaterial : originalMaterial;
    }
}