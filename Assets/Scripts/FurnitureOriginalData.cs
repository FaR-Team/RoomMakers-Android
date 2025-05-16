using System;
using UnityEngine;

[CreateAssetMenu(fileName = "new Furniture", menuName = "Muebles")]
public class FurnitureOriginalData : ScriptableObject
{
    public string Name;
    public string es_Name;
    public int price;
    public Vector2Int size;
    public GameObject prefab;
    public TypeOfSize typeOfSize;
    public FurnitureOriginalData[] compatibles;
    public FurnitureOriginalData requiredBase;
    public Sprite[] sprites;
    public Sprite indicatorSprite;
    public bool hasComboSprite = false; 
    [Tooltip("If hasComboSprite is true, this furniture will change its sprite when combined with this specific furniture")]
    public FurnitureOriginalData comboTriggerFurniture;

    [Header("Stacking Properties")]
    public bool isStackReceiver = false; 
    public bool isStackable = false;
    public int maxStackLevel = 4;
    [Tooltip("Sprites for each stack level (index 0 for level 1, index 1 for level 2, etc.)")]
    public Sprite[] stackLevelSprites; 

    public bool wallObject;
    
    [Header("Tagging System")]
    public RoomTag furnitureTag = RoomTag.None;
    public int tagMatchBonusPoints = 50;
}
public enum TypeOfSize
{
    one_one, two_two, two_one, three_one
}