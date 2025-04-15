using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TopFurnitureObject : FurnitureObjectBase
{
    [SerializeField] private GameObject comboStar;
    private bool comboDone;
    private SpriteRenderer spriteRenderer;

    public bool ComboDone => comboDone;
    
    private void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }
    
    private void OnEnable()
    {
        StateManager.OnStateChanged += EnableStar;
    }

    private void OnDisable()
    {
        StateManager.OnStateChanged -= EnableStar;
    }

    public override void CopyFurnitureData(FurnitureData newData)
    {
        base.CopyFurnitureData(newData);
        furnitureData.comboDone = newData.comboDone;
        comboDone = furnitureData.comboDone;
        comboStar.transform.eulerAngles = Vector3.zero;
        
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    public void MakeCombo(FurnitureOriginalData bottomFurnitureData)
    {
        comboDone = true;
        
        // Ensure furnitureData is initialized
        if (furnitureData == null)
        {
            furnitureData = new FurnitureData();
        }
        
        furnitureData.comboDone = true;
        
        // Check if this top furniture should change sprite when placed on specific bottom furniture
        if (furnitureData.originalData != null && 
            furnitureData.originalData.hasComboSprite && 
            furnitureData.originalData.comboTriggerFurniture == bottomFurnitureData)
        {
            ChangeToComboSprite();
        }
    }

    public void CheckAndUpdateSprite(FurnitureOriginalData bottomFurnitureData)
    {        
        // Check if this is a specific combo that should trigger sprite change
        if (furnitureData.originalData.hasComboSprite && 
            furnitureData.originalData.comboTriggerFurniture == bottomFurnitureData)
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
        if (furnitureData == null || furnitureData.originalData == null)
        {
            Debug.LogWarning("FurnitureData or OriginalData is null in TopFurnitureObject.ChangeToComboSprite");
            return;
        }
        
        if (furnitureData.originalData.hasComboSprite && 
            furnitureData.originalData.sprites != null && 
            furnitureData.originalData.sprites.Length > 1)
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
                if (spriteRenderer == null)
                {
                    Debug.LogWarning("SpriteRenderer not found in TopFurnitureObject.ChangeToComboSprite");
                    return;
                }
            }
            
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
    
    public void EnableStar(GameState newState)
    {
        comboStar.SetActive(newState == GameState.Editing && comboDone);
    }
}
