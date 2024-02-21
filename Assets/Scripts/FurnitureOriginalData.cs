using System;
using UnityEngine;

[CreateAssetMenu(fileName = "new Furniture", menuName = "Muebles")]
public class FurnitureOriginalData : ScriptableObject
{
    public string Name;
    public int price;
    public int priceCombo;
    public Vector2Int size;
    public GameObject prefab;
    public TypeOfSize typeOfSize;
    public FurnitureOriginalData[] compatibles;
    public Sprite[] sprites;
    //Los objetos que son compatibles con este objeto, más no así, los que este son compatibles con.
}
public enum TypeOfSize
{
    one_one, two_two, three_three, two_one, three_two
}