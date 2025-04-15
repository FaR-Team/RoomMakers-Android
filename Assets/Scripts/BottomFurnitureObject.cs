using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class BottomFurnitureObject : FurnitureObjectBase
{
    [SerializeField] private int comboValue = 30;
    [SerializeField] private Transform comboStarContainer;
    private HashSet<Vector2Int> localTilesCombos = new();
    
    // Track the current top furniture to know when to change sprites
    private FurnitureOriginalData currentTopFurniture;
    private SpriteRenderer spriteRenderer;

    private void OnEnable()
    {
        Initialize();
        StateManager.OnStateChanged += EnableStars;
    }

    private void OnDisable()
    {
        StateManager.OnStateChanged -= EnableStars;
    }

    private void Initialize()
    {
        comboStarContainer = new GameObject().transform;
        comboStarContainer.SetParent(transform);
        comboStarContainer.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        
        // Get sprite renderer
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    public int MakeCombo(Vector2[] positions, FurnitureOriginalData topFurnitureData)
    {
        int totalCombo = 0;
        for (int i = 0; i < positions.Length; i++)
        {
            Vector2Int local = ConvertWorldToLocalTile(positions[i]);
            if (!localTilesCombos.Contains(local))
            {
                CreateStarIcon(local);
                
                localTilesCombos.Add(local);
                totalCombo += comboValue;
            }
        }
        
        // Always check for sprite change, regardless of whether points were awarded
        CheckAndUpdateSprite(topFurnitureData);
        
        // TODO: Improve, fast fix because it'll stay active after making combo
        comboStarContainer.gameObject.SetActive(false);

        furnitureData.localTileCombos = localTilesCombos;
        
        // Return total combo
        return totalCombo;
    }
    
    private void CheckAndUpdateSprite(FurnitureOriginalData topFurnitureData)
    {
        // Always update the current top furniture
        currentTopFurniture = topFurnitureData;
        
        // Check if this is a specific combo that should trigger sprite change
        if (furnitureData.originalData.hasComboSprite && 
            furnitureData.originalData.comboTriggerFurniture == topFurnitureData)
        {
            // Change to combo sprite
            ChangeToComboSprite();
        }
        else
        {
            // Change back to default sprite
            ResetToDefaultSprite();
        }
    }
    
    private void ChangeToComboSprite()
    {
        if (furnitureData.originalData.hasComboSprite && 
            furnitureData.originalData.sprites != null && 
            furnitureData.originalData.sprites.Length > 1 &&
            spriteRenderer != null)
        {
            spriteRenderer.sprite = furnitureData.originalData.sprites[1]; // Use the combo sprite (index 1)
        }
    }
    
    private void ResetToDefaultSprite()
    {
        if (furnitureData.originalData.sprites != null && 
            furnitureData.originalData.sprites.Length > 0 &&
            spriteRenderer != null)
        {
            spriteRenderer.sprite = furnitureData.originalData.sprites[0]; // Use the default sprite (index 0)
        }
    }

    public Vector2Int ConvertWorldToLocalTile(Vector2 worldPosition)
    {
        // Get local position according to world position
        Vector2 localPosition = worldPosition - (Vector2)transform.position;

        Vector2 aux = localPosition;
        switch (furnitureData.rotationStep)
        {
            case 0:
                break;
            case 1:
                localPosition.x = aux.y;
                localPosition.y = -aux.x;
                break;
            case 2:
                localPosition = -localPosition;
                break;
            case 3:
                localPosition.x = -aux.y;
                localPosition.y = aux.x;
                break;
            default:
                break;
        }

        return Vector2Int.RoundToInt(localPosition);
    }

    public override void CopyFurnitureData(FurnitureData newData)
    {
        base.CopyFurnitureData(newData);
        
        furnitureData.localTileCombos = newData.localTileCombos;
        localTilesCombos = furnitureData.localTileCombos;
        
        // Initialize sprite renderer if not already done
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        
        foreach (var tile in localTilesCombos)
        {
            CreateStarIcon(tile);
        }
    }

    public void EnableStars(GameState newState)
    {
        comboStarContainer.gameObject.SetActive(newState == GameState.Editing);
    }

    public void CreateStarIcon(Vector2Int local)
    {
        Transform star = Instantiate(House.instance.comboStarSprite).transform;
        star.SetParent(comboStarContainer);
        star.localPosition = new Vector3(local.x, local.y, 0);
        star.eulerAngles = Vector3.zero;
    }
}
