using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Playables;

public class RoomFurnitures : MonoBehaviour
{
    public Dictionary<Vector2, PlacementData> PlacementDatasInPosition = new();

    public bool PlaceFurniture(Vector2 position, FurnitureData furnitureData)
    {
        var data = furnitureData.originalData;

        List<Vector2> positionToOccupy = CalculatePositions(position, furnitureData.size);

        bool canPlace = !positionToOccupy.Any(x => PlacementDatasInPosition.ContainsKey(x) || Physics2D.OverlapCircle(x, 0.2f));
        bool placeOnTop = false;

        if (canPlace)
        {
            foreach (var pos in positionToOccupy)
            {
                PlacementDatasInPosition[pos] = new PlacementData(positionToOccupy, data);
            }
        }
        else
        {
            // Mejorar esta parte, capaz fijarnos si es compatible arriba dentro del Any() y aca retornar
            foreach (var pos in positionToOccupy)
            {
                if (!PlacementDatasInPosition.ContainsKey(pos)) break;

                // Se fija si todas las posiciones que va a ocupar el objeto estan dentro de las posiciones que ocupa el objeto debajo, si es compatible y si no hay ya algo encima
                canPlace = positionToOccupy.Intersect(PlacementDatasInPosition[pos].occupiedPositions).Count() ==
                           positionToOccupy.Count()
                           && PlacementDatasInPosition[pos].furniture.compatibles.Contains(data)
                           && PlacementDatasInPosition[pos].occupiedPositions.All(occupied => PlacementDatasInPosition[occupied].instantiatedFurnitureOnTop == null);
                // Si queremos que se puedan poner varios encima de algo compatible, capaz al remover chequear si hay varias datas y con cual es compatible

                placeOnTop = canPlace;

                break;
            }
        }

        if (!canPlace) return false;

        var finalPos = GridManager.PositionToCellCenter(position);

        // Guardamos el objeto que instanciamos en en cada PlacementData
        GameObject furniturePrefab = Instantiate(furnitureData.prefab, finalPos, Quaternion.Euler(furnitureData.VectorRotation));
        GivePoints(data, positionToOccupy, placeOnTop, finalPos, furniturePrefab);

        return true;
    }

    protected virtual void GivePoints(FurnitureOriginalData data, List<Vector2> positionToOccupy, bool placeOnTop, Vector2 finalPos, GameObject furniturePrefab)
    {
        if (!placeOnTop)
        {
            if (TryGetComponent(out MainRoom room))
            {
                MainRoom.instance.availableTiles -= data.size.x * data.size.y;
            }

            PlayerController.instance.Inventory.UpdateMoney(data.price);
            House.instance.UpdateScore(data.price);
            positionToOccupy.ForEach(pos => PlacementDatasInPosition[pos].instantiatedFurniture = furniturePrefab);
        }
        else
        {
            // Si el objeto va encima de otro, lo guardamos en el PlacementData y damos doble score por combo

            PlayerController.instance.Inventory.UpdateMoney(data.price * 2);
            House.instance.UpdateScore(data.price * 2);

            PlacementDatasInPosition[finalPos].occupiedPositions.ForEach(pos =>
            {
                PlacementDatasInPosition[pos].instantiatedFurnitureOnTop = furniturePrefab;
                PlacementDatasInPosition[pos].furnitureOnTop = data;
            });
        }
    }

    public void RemoveDataInPositions(List<Vector2> positions)
    {
        foreach (var pos in positions)
        {
            PlacementDatasInPosition.Remove(pos);
        }
    }

    public void RemoveTopObjectInPositions(List<Vector2> positions)
    {
        foreach (var pos in positions)
        {
            PlacementDatasInPosition[pos].furnitureOnTop = null;
            //PlacementDatasInPosition[pos].instantiatedFurnitureOnTop = null;
        }
    }

    private List<Vector2> CalculatePositions(Vector2 position, Vector2Int size)
    {
        List<Vector2> returnVal = new();
        for (int x = 0; x < Mathf.Abs(size.x); x++)
        {
            for (int y = 0; y < Mathf.Abs(size.y); y++)
            {
                // Multiplicamos por el signo para que sepa si el size es negativo o positivo
                var newPosition = position + new Vector2(x * Mathf.Sign(size.x), y * Mathf.Sign(size.y));
                returnVal.Add(GridManager.PositionToCellCenter(newPosition));
            }
        }
        return returnVal;
    }

}