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
    public Sprite[] sprites;
    public bool hasComboSprite = false; 
    [Tooltip("If hasComboSprite is true, this furniture will change its sprite when combined with this specific furniture")]
    public FurnitureOriginalData comboTriggerFurniture;
    
    [Header("Tagging System")]
    public RoomTag furnitureTag = RoomTag.None;
    public int tagMatchBonusPoints = 50; // Points awarded when furniture tag matches room tag
    public bool isLabeler = false; // Whether this furniture is a labeler item
    public bool hasReceivedTagBonus = false;
}
public enum TypeOfSize
{
    one_one, two_two, three_three, two_one, three_two
}