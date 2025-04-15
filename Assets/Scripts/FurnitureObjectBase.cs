using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class FurnitureObjectBase : MonoBehaviour
{
    public FurnitureOriginalData originalData;
    [SerializeField] protected FurnitureData furnitureData;
    [SerializeField] protected SpriteRenderer spriteRenderer;
    
    protected int currentSpriteIndex = 0;

    public FurnitureData Data => furnitureData;
    
    private void Awake()
    {
        furnitureData = new FurnitureData();
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    public virtual void CopyFurnitureData(FurnitureData newData)
    {
        // Ensure furnitureData is initialized
        if (furnitureData == null)
        {
            furnitureData = new FurnitureData();
        }
        
        furnitureData.size = newData.size;
        furnitureData.prefab = newData.prefab;
        furnitureData.originalData = newData.originalData;
        furnitureData.VectorRotation = newData.VectorRotation;
        furnitureData.rotationStep = newData.rotationStep;
        
        // Set initial sprite if needed
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }
    
    public virtual void UpdateSprite(int spriteIndex)
    {
        if (spriteRenderer == null || furnitureData.originalData == null)
            return;
            
        // Only update sprite if this furniture has combo sprites enabled
        if (furnitureData.originalData.hasComboSprite && 
            furnitureData.originalData.sprites != null && 
            spriteIndex >= 0 && 
            spriteIndex < furnitureData.originalData.sprites.Length)
        {
            spriteRenderer.sprite = furnitureData.originalData.sprites[spriteIndex];
            currentSpriteIndex = spriteIndex;
        }
    }
    
    /*public virtual bool ChangeToComboSprite()
    {
        // Only change sprite if this furniture has combo sprites enabled
        if (furnitureData.originalData.hasComboSprite && 
            furnitureData.originalData.sprites != null && 
            furnitureData.originalData.sprites.Length > 1)
        {
            UpdateSprite(1); // Assuming index 1 is the combo sprite
            return true;
        }
        return false;
    }*/
}