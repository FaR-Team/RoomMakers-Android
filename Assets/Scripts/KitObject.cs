using System;
using System.Collections;
using UnityEngine;

public class KitObject : FurnitureObjectBase
{
    private int[] originalSortingLayerIDs;
    private int[] originalSortingOrders;
    private bool[] originalEnabledStates;
    private const string HIGHLIGHT_SORTING_LAYER_NAME = "Preview";
    
    private bool hasFurnitureAbove = false;

    [Tooltip("How quickly the kit object blinks when highlighted (seconds per half-blink).")]
    [SerializeField] private float blinkInterval = 0.4f;
    private Coroutine blinkingCoroutine = null;

    [SerializeField] private new SpriteRenderer[] spriteRenderers;

    protected override void Awake()
    {
        base.Awake();

        if (this.spriteRenderers != null && this.spriteRenderers.Length > 0)
        {
            originalSortingLayerIDs = new int[this.spriteRenderers.Length];
            originalSortingOrders = new int[this.spriteRenderers.Length];
            originalEnabledStates = new bool[this.spriteRenderers.Length];

            for (int i = 0; i < this.spriteRenderers.Length; i++)
            {
                SpriteRenderer sr = this.spriteRenderers[i];
                if (sr != null)
                {
                    originalSortingLayerIDs[i] = sr.sortingLayerID;
                    originalSortingOrders[i] = sr.sortingOrder;
                    originalEnabledStates[i] = sr.enabled;
                }
            }
        }
        else
        {
            Debug.LogWarning($"KitObject '{gameObject.name}': 'spriteRenderers' array is not assigned or is empty. Highlighting/blinking will not work correctly for this instance.", this);
            originalSortingLayerIDs = new int[0];
            originalSortingOrders = new int[0];
            originalEnabledStates = new bool[0];
        }
    }

    protected void OnEnable()
    {

        StateManager.OnStateChanged += HandleRelevantChange;
        if (EditingManager.Instance != null)
        {
            EditingManager.Instance.OnHeldItemChanged += HandleRelevantChange;
        }

        UpdateHighlightState();
    }

    protected void OnDisable()
    {

        StateManager.OnStateChanged -= HandleRelevantChange;
        if (EditingManager.Instance != null)
        {
            EditingManager.Instance.OnHeldItemChanged -= HandleRelevantChange;
        }

        if (blinkingCoroutine != null)
        {
            StopCoroutine(blinkingCoroutine);
            blinkingCoroutine = null;
        }
        RevertToOriginalState();
    }

    private void HandleRelevantChange(GameState _){ UpdateHighlightState(); }
    private void HandleRelevantChange(FurnitureOriginalData _){ UpdateHighlightState(); }


    public void UpdateHighlightState()
    {
        if (this.spriteRenderers == null || this.spriteRenderers.Length == 0)
        {
            if (blinkingCoroutine != null)
            {
                StopCoroutine(blinkingCoroutine);
                blinkingCoroutine = null;
            }
            return;
        }

        hasFurnitureAbove = CheckForFurnitureAbove();

        bool shouldHighlight = false;
        GameState currentGameState = StateManager.CurrentGameState;

        if (House.instance != null && House.instance.classicMode)
        {
            shouldHighlight = false;
        }
        else
        {
            FurnitureOriginalData heldItemOriginalData = null;
            if (EditingManager.Instance != null)
            {
                heldItemOriginalData = EditingManager.Instance.CurrentlyHeldFurnitureOriginalData;
            }

            if (currentGameState == GameState.Editing &&
                heldItemOriginalData != null &&
                heldItemOriginalData.requiredBase != null)
            {

                if (heldItemOriginalData.requiredBase == this.furnitureData.originalData)
                {
                    shouldHighlight = true;
                }
            }
        }

        if (shouldHighlight)
        {
            foreach (SpriteRenderer sr in this.spriteRenderers)
            {
                if (sr != null)
                {
                    sr.sortingLayerName = HIGHLIGHT_SORTING_LAYER_NAME;
                }
            }

            if (blinkingCoroutine == null)
            {
                blinkingCoroutine = StartCoroutine(BlinkEffectCoroutine());
            }
        }
        else
        {
            if (blinkingCoroutine != null)
            {
                StopCoroutine(blinkingCoroutine);
                blinkingCoroutine = null;
            }
            RevertToOriginalState();
        }
        
        UpdateVisibility();
    }
    
    private bool CheckForFurnitureAbove()
    {
        Vector2 kitPosition = GridManager.PositionToCellCenter(transform.position);
        
        RoomFurnitures roomFurnitures = FindObjectOfType<RoomFurnitures>();
        if (roomFurnitures != null && roomFurnitures.PlacementDatasInPosition.TryGetValue(kitPosition, out PlacementData placementData))
        {
            return placementData.instantiatedFurniture != null;
        }
        
        return false;
    }

    private void RevertToOriginalState()
    {
        if (this.spriteRenderers == null || this.spriteRenderers.Length == 0) return;
        if (originalSortingLayerIDs == null || originalSortingOrders == null || originalEnabledStates == null) return;

        for (int i = 0; i < this.spriteRenderers.Length; i++)
        {
            SpriteRenderer sr = this.spriteRenderers[i];
            if (sr != null)
            {
                if (i < originalSortingLayerIDs.Length) sr.sortingLayerID = originalSortingLayerIDs[i];
                if (i < originalSortingOrders.Length) sr.sortingOrder = originalSortingOrders[i];
            }
        }
        
        UpdateVisibility();
    }

    private void UpdateVisibility()
    {
        if (hasFurnitureAbove && blinkingCoroutine == null)
        {
            foreach (SpriteRenderer sr in this.spriteRenderers)
            {
                if (sr != null) sr.enabled = false;
            }
        }
        else if (!hasFurnitureAbove && blinkingCoroutine == null)
        {
            for (int i = 0; i < this.spriteRenderers.Length; i++)
            {
                SpriteRenderer sr = this.spriteRenderers[i];
                if (sr != null)
                {
                    if (i < originalEnabledStates.Length) sr.enabled = originalEnabledStates[i];
                    else sr.enabled = true;
                }
            }
        }
    }

    public void OnFurnitureAboveChanged()
    {
        UpdateHighlightState();
    }

    private IEnumerator BlinkEffectCoroutine()
    {
        foreach (SpriteRenderer sr in this.spriteRenderers)
        {
            if (sr != null) sr.enabled = true;
        }

        while (true)
        {
            yield return new WaitForSeconds(blinkInterval);
            foreach (SpriteRenderer sr in this.spriteRenderers)
            {
                if (sr != null) sr.enabled = !sr.enabled;
            }
        }
    }
}
