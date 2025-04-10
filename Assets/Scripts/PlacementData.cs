using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlacementData
{
    public List<Vector2> occupiedPositions;

    public FurnitureData furnitureData;
    public FurnitureObjectBase instantiatedFurniture;
    
    public List<TopPlacementData> topPlacementDatas = new List<TopPlacementData>();
    
    public PlacementData(List<Vector2> occupiedPositions, FurnitureData furniture)
    {
        this.occupiedPositions = occupiedPositions;

        furnitureData = furniture;
    }

    public bool IsCompatibleWith(FurnitureOriginalData topFurnitureData) //es compatible con, esta data.
    {
        return furnitureData.originalData.compatibles.Contains(topFurnitureData);
    }

    public void PlaceObjectOnTop(List<Vector2> positions, TopFurnitureObject topFurnitureObject)
    {
        var topPlacementData = new TopPlacementData()
        {
            occupiedPositions = positions,
            furnitureOnTopData = topFurnitureObject.Data,
            instantiatedFurnitureOnTop = topFurnitureObject
        };
        
        topPlacementDatas.Add(topPlacementData);
    }

    public void ClearTopObject(Vector2 pos)
    {
        TopPlacementData data = topPlacementDatas.FirstOrDefault(x => x.occupiedPositions.Contains(pos));
        TopFurnitureObject topObject = data?.instantiatedFurnitureOnTop;
        if (topObject != null) Object.Destroy(topObject.gameObject);
        topPlacementDatas.Remove(data);
    }

    public FurnitureData GetAndClearFirstObject()
    {
        var topData = topPlacementDatas[0];
        var furnitureData = topData.furnitureOnTopData;
        TopFurnitureObject topObject = topData?.instantiatedFurnitureOnTop;
        if (topObject != null) Object.Destroy(topObject.gameObject);
        topPlacementDatas.RemoveAt(0);

        return furnitureData;

    }
    public FurnitureData GetTopFurnitureData(Vector2 pos)
    {
        return topPlacementDatas.FirstOrDefault(x => x.occupiedPositions.Contains(pos))?.furnitureOnTopData;
    }

    public bool HasFreePositions(List<Vector2> positions)
    {
        bool free = true;
        for (int i = 0; i < positions.Count; i++)
        {
            if (topPlacementDatas.Any(data => data.occupiedPositions.Contains(positions[i]))) free = false;
        }

        return free;
    }
}

public class TopPlacementData
{
    public List<Vector2> occupiedPositions;
    public FurnitureData furnitureOnTopData;
    public TopFurnitureObject instantiatedFurnitureOnTop;
}