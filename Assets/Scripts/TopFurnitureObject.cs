using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TopFurnitureObject : FurnitureObjectBase
{
    [SerializeField] private GameObject comboStar;
    private bool comboDone;
    private SpriteRenderer spriteRenderer;

    public bool ComboDone => comboDone;
    
    protected override void Awake()
    {
        base.Awake();
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
        
        furnitureData.rotationStep = newData.rotationStep;
        furnitureData.VectorRotation = newData.VectorRotation;
        
        gameObject.transform.rotation = Quaternion.Euler(furnitureData.VectorRotation);
        
        if (comboStar != null)
            comboStar.transform.eulerAngles = Vector3.zero;
        
        furnitureData.currentStackLevel = newData.currentStackLevel;
        
        if (furnitureData.originalData.isStackable && 
            furnitureData.currentStackLevel > 0 &&
            furnitureData.originalData.stackLevelSprites != null &&
            furnitureData.originalData.stackLevelSprites.Length >= furnitureData.currentStackLevel)
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = furnitureData.originalData.stackLevelSprites[furnitureData.currentStackLevel - 1];
                
                if (spriteRenderer.transform != transform)
                {
                    spriteRenderer.transform.rotation = transform.rotation;
                }
            }
        }
        
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    public void MakeCombo()
    {
        comboDone = true;
        
        if (furnitureData == null)
        {
            furnitureData = new FurnitureData();
        }
        
        furnitureData.comboDone = true;
    }

    public void CheckAndUpdateSprite(BottomFurnitureObject bottomFurniture)
    {        
        if (furnitureData.originalData.hasComboSprite && 
            furnitureData.originalData.comboTriggerFurniture == bottomFurniture.originalData)
        {
            ChangeToComboSprite();
            CopyRotation(bottomFurniture);
        }
        else
        {
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
            
            spriteRenderer.sprite = furnitureData.originalData.sprites[1];
        }
        
    }

    private void ResetToDefaultSprite()
    {
        if (furnitureData.originalData.sprites != null && 
            furnitureData.originalData.sprites.Length > 0 &&
            spriteRenderer != null)
        {
            spriteRenderer.sprite = furnitureData.originalData.sprites[0];
        }
    }

    void CopyRotation(FurnitureObjectBase furnitureToCopy)
    {
        furnitureData.rotationStep = furnitureToCopy.Data.rotationStep;
        furnitureData.VectorRotation = furnitureToCopy.Data.VectorRotation;
        
        gameObject.transform.rotation = furnitureToCopy.gameObject.transform.rotation;
        
        if (spriteRenderer != null && spriteRenderer.transform != transform)
        {
            spriteRenderer.transform.rotation = furnitureToCopy.gameObject.transform.rotation;
        }
        
        if (comboStar != null)
        {
            comboStar.transform.eulerAngles = Vector3.zero;
        }
    }
    
    public void EnableStar(GameState newState)
    {
        comboStar.SetActive(newState == GameState.Editing && comboDone);
    }
}
