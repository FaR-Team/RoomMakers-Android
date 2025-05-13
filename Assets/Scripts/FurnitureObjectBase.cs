using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class FurnitureObjectBase : MonoBehaviour
{
    public FurnitureOriginalData originalData;
    [SerializeField] protected FurnitureData furnitureData;
    protected SpriteRenderer[] spriteRenderers;
    protected bool unpacked;
    protected int currentSpriteIndex = 0;
    protected bool hasReceivedTagBonus = false;

    public FurnitureData Data => furnitureData;
    public bool IsUnpacked => unpacked;
    public bool HasReceivedTagBonus => hasReceivedTagBonus;
    
    public void MarkTagBonusReceived() // TODO: Ver pa que está esto?
    {
        hasReceivedTagBonus = true;
    }
    
    protected virtual void Awake()
    {
        furnitureData = new FurnitureData();
        if (spriteRenderers == null)
            spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
    }

    public void SetUnpackedState(bool unpacked)
    {
        this.unpacked = unpacked;
        if (!this.unpacked)
        {
            if(TryGetComponent(out Animator anim)) anim.enabled = false;
            UpdateSprites(House.instance.GetSpritesBySize(Data.originalData.typeOfSize)); // Update with box sprites
        }
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
        furnitureData.hasReceivedTagBonus = newData.hasReceivedTagBonus;
        
        // Set initial sprite if needed
        if (spriteRenderers == null)
            spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
    }
    
    public virtual void UpdateSprites(Sprite[] sprites)
    {
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            spriteRenderers[i].sprite = sprites[i]; // No agregué null checks porque hay que asegurarnos que nunca sea null alguno
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