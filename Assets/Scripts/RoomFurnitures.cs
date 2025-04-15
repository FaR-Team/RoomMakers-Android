using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.VisualScripting;
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
            PlacementData data = new PlacementData(positionToOccupy, furnitureData);
            foreach (var pos in positionToOccupy)
            {
                PlacementDatasInPosition[pos] = data;
            }
        }
        else
        {
            bool check = true;
            Vector2 validBottomPosition = Vector2.zero;
            bool foundValidPosition = false;
            
            // First check if all positions are contained in the dictionary
            foreach (var pos in positionToOccupy)
            {
                if (!PlacementDatasInPosition.ContainsKey(pos)) 
                {
                    check = false;
                    break;
                }
                
                // Find a valid position that we can use for compatibility checks
                if (!foundValidPosition)
                {
                    validBottomPosition = pos;
                    foundValidPosition = true;
                }
            }

            if (check && foundValidPosition)
            {
                // Use the valid position we found for compatibility checks
                canPlace = positionToOccupy.Intersect(PlacementDatasInPosition[validBottomPosition].occupiedPositions).Count() == 
                           positionToOccupy.Count()
                           && PlacementDatasInPosition[validBottomPosition].IsCompatibleWith(originalData)
                           && PlacementDatasInPosition[validBottomPosition].HasFreePositions(positionToOccupy);

                placeOnTop = canPlace;
                
                // If we can place it, update the position to use for instantiation
                if (canPlace)
                {
                    position = validBottomPosition;
                }
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

            PlacementDatasInPosition[finalPos].instantiatedFurniture = furnitureObject;
        }
        else
        {
            // Si el objeto va encima de otro, lo guardamos en el PlacementData y damos doble score por combo
            int totalCombo = 0;
            
            TopFurnitureObject topObject = (TopFurnitureObject) furnitureObject;
            BottomFurnitureObject bottomObject = (BottomFurnitureObject) PlacementDatasInPosition[finalPos]
                .instantiatedFurniture;
            
            // Get the bottom furniture's original data
            FurnitureOriginalData bottomFurnitureData = bottomObject.Data.originalData;
            
            // Check for sprite changes regardless of combo status
            //CheckForSpriteChanges(topObject, bottomFurnitureData);
            topObject.CheckAndUpdateSprite(bottomFurnitureData);
            
            // Only calculate and award points if combo hasn't been done yet
            if(!topObject.ComboDone)
            {
                // Calculamos y sumamos los puntos por cada tile en el que se hace combo
                totalCombo += bottomObject.MakeCombo(positionToOccupy.ToArray(), topObject.Data.originalData);

                // Si hubo al menos un tile en el que se hizo combo, gastamos el combo tambien en el objeto de arriba
                if (totalCombo > 0)
                {
                    topObject.MakeCombo(bottomFurnitureData);
                    
                    PlayerController.instance.Inventory.UpdateMoney(totalCombo);
                    House.instance.UpdateScore(totalCombo);
                    
                    ComboPopUp.Create(popUpPrefab, totalCombo, finalPos, new Vector2(0f, 1.2f));
                }
            }
            
            // Updateamos la placement data con el objeto que pusimos encima
            PlacementDatasInPosition[finalPos].PlaceObjectOnTop(positionToOccupy, topObject);
        }
    }
    // New method to check for sprite changes
    private void CheckForSpriteChanges(TopFurnitureObject topObject, FurnitureOriginalData bottomFurnitureData)
    {
        if (topObject == null || bottomFurnitureData == null)
            return;
        
        // Check if this top furniture should change sprite when placed on specific bottom furniture
        if (topObject.Data != null && 
            topObject.Data.originalData != null && 
            topObject.Data.originalData.hasComboSprite)
        {
            // Get the sprite renderer
            SpriteRenderer spriteRenderer = topObject.GetComponentInChildren<SpriteRenderer>();
            if (spriteRenderer == null)
                return;
            
            // Check if this is the specific combo that triggers sprite change
            if (topObject.Data.originalData.comboTriggerFurniture == bottomFurnitureData)
            {
                // Change to combo sprite
                if (topObject.Data.originalData.sprites != null && 
                    topObject.Data.originalData.sprites.Length > 1)
                {
                    spriteRenderer.sprite = topObject.Data.originalData.sprites[1];
                }
            }
            else
            {
                // Reset to default sprite
                if (topObject.Data.originalData.sprites != null && 
                    topObject.Data.originalData.sprites.Length > 0)
                {
                    spriteRenderer.sprite = topObject.Data.originalData.sprites[0];
                }
            }
        }
    }
    public void RemoveDataInPositions(List<Vector2> positions)
    {
        foreach (var pos in positions)
        {
            PlacementDatasInPosition.Remove(pos);
        }
    }
    
    public void RemoveDataInPosition(Vector2 pos)
    {
        var data = PlacementDatasInPosition[pos];
        Destroy(data.instantiatedFurniture.gameObject);
        foreach (var tile in data.occupiedPositions)
        {
            PlacementDatasInPosition.Remove(tile);
        }
    }

    public void RemoveTopObjectInPositions(List<Vector2> positions)
    {
        foreach (var pos in positions)
        {
            //PlacementDatasInPosition[pos].furnitureOnTopData = null;
            //PlacementDatasInPosition[pos].instantiatedFurnitureOnTop = null;
        }
    }

    public void RemoveTopObjectInPosition(Vector2 pos)
    {
        PlacementDatasInPosition[pos].ClearTopObject(pos);
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