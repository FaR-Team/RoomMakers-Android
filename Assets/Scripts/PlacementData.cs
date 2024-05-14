using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlacementData
{
    public List<Vector2> occupiedPositions;

    public FurnitureData furnitureData;
    public FurnitureData furnitureOnTopData;

    public FurnitureObjectBase instantiatedFurniture;
    public TopFurnitureObject instantiatedFurnitureOnTop;
    public PlacementData(List<Vector2> occupiedPositions, FurnitureData furniture)
    {
        this.occupiedPositions = occupiedPositions;

        furnitureData = furniture;
    }

    public bool IsCompatibleWith(FurnitureOriginalData topFurnitureData) //es compatible con, esta data.
    {
        return furnitureData.originalData.compatibles.Contains(topFurnitureData);
    }
}