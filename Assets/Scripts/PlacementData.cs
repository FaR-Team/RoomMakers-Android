using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlacementData
{
    public List<Vector2> occupiedPositions;

    public FurnitureOriginalData furniture;
    public FurnitureOriginalData furnitureOnTop;

    public GameObject instantiatedFurniture;
    public GameObject instantiatedFurnitureOnTop;
    public PlacementData(List<Vector2> occupiedPositions, FurnitureOriginalData furniture)
    {
        this.occupiedPositions = occupiedPositions;
        this.furniture = furniture;
    }

    public bool IsCompatibleWith(FurnitureOriginalData furnitureData) //es compatible con, esta data.
    {
        return furniture.compatibles.Contains(furnitureData);
    }
}