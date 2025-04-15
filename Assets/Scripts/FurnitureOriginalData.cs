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
    //Los objetos que son compatibles con este objeto, más no así, los que este son compatibles con.
}
public enum TypeOfSize
{
    one_one, two_two, three_three, two_one, three_two
}