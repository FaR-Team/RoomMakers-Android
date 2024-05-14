using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Playables;

public class RoomFurnitures : MonoBehaviour
{
    public Dictionary<Vector2, PlacementData> PlacementDatasInPosition = new();
    
    // TODO: que no se tenga que asignar aca la referencia al prefab
    [SerializeField] protected ComboPopUp popUpPrefab;

    public bool PlaceFurniture(Vector2 position, FurnitureData furnitureData)
    {
        var originalData = furnitureData.originalData;

        List<Vector2> positionToOccupy = CalculatePositions(position, furnitureData.size);

        bool canPlace = !positionToOccupy.Any(x => PlacementDatasInPosition.ContainsKey(x) || Physics2D.OverlapCircle(x, 0.2f));
        bool placeOnTop = false;

        if (canPlace)
        {
            foreach (var pos in positionToOccupy)
            {
                PlacementDatasInPosition[pos] = new PlacementData(positionToOccupy, furnitureData);
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
                           && PlacementDatasInPosition[pos].IsCompatibleWith(originalData)
                           && PlacementDatasInPosition[pos].occupiedPositions.All(occupied => PlacementDatasInPosition[occupied].instantiatedFurnitureOnTop == null);
                // Si queremos que se puedan poner varios encima de algo compatible, capaz al remover chequear si hay varias datas y con cual es compatible

                placeOnTop = canPlace;

                break;
            }
        }

        if (!canPlace) return false;

        var finalPos = GridManager.PositionToCellCenter(position);

        // Guardamos el objeto que instanciamos en en cada PlacementData
        FurnitureObjectBase furniturePrefab = Instantiate(furnitureData.prefab, finalPos, Quaternion.Euler(furnitureData.VectorRotation)).GetComponent<FurnitureObjectBase>();
        furniturePrefab.CopyFurnitureData(furnitureData);
        
        GivePoints(furnitureData, positionToOccupy, placeOnTop, finalPos, furniturePrefab);

        return true;
    }

    protected virtual void GivePoints(FurnitureData data, List<Vector2> positionToOccupy, bool placeOnTop, Vector2 finalPos, FurnitureObjectBase furnitureObject)
    {
        if (!placeOnTop)
        {
            if (TryGetComponent(out MainRoom room))
            {
                MainRoom.instance.availableTiles -= data.originalData.size.x * data.originalData.size.y;
            }

            positionToOccupy.ForEach(pos => PlacementDatasInPosition[pos].instantiatedFurniture = furnitureObject);
        }
        else
        {
            // Si el objeto va encima de otro, lo guardamos en el PlacementData y damos doble score por combo

            int totalCombo = 0;
            
            TopFurnitureObject topObject = (TopFurnitureObject) furnitureObject;
            BottomFurnitureObject bottomObject = (BottomFurnitureObject) PlacementDatasInPosition[finalPos]
                .instantiatedFurniture;
            
            // Nos fijamos si el objeto de arriba no hizo combo aún
            if(!topObject.ComboDone)
            {
                // Calculamos y sumamos los puntos por cada tile en el que se hace combo
                totalCombo += bottomObject.MakeCombo(positionToOccupy.ToArray());

                // Si hubo al menos un tile en el que se hizo combo, gastamos el combo tambien en el objeto de arriba
                if (totalCombo > 0)
                {
                    topObject.MakeCombo();
                    
                    PlayerController.instance.Inventory.UpdateMoney(totalCombo);
                    House.instance.UpdateScore(totalCombo);
                    
                    ComboPopUp.Create(popUpPrefab, totalCombo, PlayerController.instance.transform.position, new Vector2(0f, 1.2f));
                }
            }
            
            PlacementDatasInPosition[finalPos].occupiedPositions.ForEach(pos =>
            {
                PlacementDatasInPosition[pos].instantiatedFurnitureOnTop = topObject;
                PlacementDatasInPosition[pos].furnitureOnTopData = data;
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
            PlacementDatasInPosition[pos].furnitureOnTopData = null;
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