using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New item", menuName = "Items")]
public class ItemData : FurnitureOriginalData
{
    public ItemType type;
    public Sprite ShopSprite;
    public int buyPrice;
}

public enum ItemType
{
    Tagger,
    PipelineKit,
    OutletKit,
    Sledgehammer,
}