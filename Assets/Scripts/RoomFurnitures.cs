using System.Collections;
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
    [SerializeField] protected ComboPopUp matchPrefab;

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
        // Get the room component
        Room currentRoom = GetComponent<Room>();
    
        // Handle labeler item
        if (data.originalData.isLabeler)
        {
            // Show tag selection UI
            TagSelectionUI.ShowTagSelection(currentRoom, () => {
                // This callback will be called after tag selection
                // Destroy the labeler item after use
                Destroy(furnitureObject.gameObject);
                RemoveDataInPositions(positionToOccupy);
            });
            return;
        }
    
        // Check if this furniture has a tag and the room doesn't have one yet
        RoomTag furnitureTag = data.originalData.furnitureTag;
        if (furnitureTag != RoomTag.None && currentRoom != null)
        {
            // Try to set the room tag from furniture
            currentRoom.TrySetRoomTagFromFurniture(furnitureTag);
        }
    
        // Calculate bonus points for tag matching - only if not already received
        int tagBonus = 0;
        if (currentRoom != null && 
            furnitureTag != RoomTag.None && 
            furnitureTag == currentRoom.roomTag && 
            !data.hasReceivedTagBonus) // Check using the data field
        {
            tagBonus = data.originalData.tagMatchBonusPoints;
            data.hasReceivedTagBonus = true; // Mark as received in the data
            
            // Show bonus points popup only if we're actually awarding points and not placing on top
            // (if placing on top, we'll handle it differently below)
            if (tagBonus > 0 && !placeOnTop)
            {
                ComboPopUp.Create(matchPrefab, tagBonus, finalPos, new Vector2(0f, 1.5f));
            }
        }
    
        if (!placeOnTop)
        {
            if (TryGetComponent(out MainRoom room))
            {
                MainRoom.instance.availableTiles -= data.originalData.size.x * data.originalData.size.y;
            }

            PlacementDatasInPosition[finalPos].instantiatedFurniture = furnitureObject;
            
            // Add tag bonus points
            if (tagBonus > 0)
            {
                PlayerController.instance.Inventory.UpdateMoney(tagBonus);
                House.instance.UpdateScore(tagBonus);
            }
        }
        else
        {
            // Si el objeto va encima de otro, lo guardamos en el PlacementData y damos doble score por combo
            int totalCombo = 0;
        
            TopFurnitureObject topObject = (TopFurnitureObject) furnitureObject;
            BottomFurnitureObject bottomObject = (BottomFurnitureObject) PlacementDatasInPosition[finalPos]
                .instantiatedFurniture;
        
            // Check for sprite changes regardless of combo status
            topObject.CheckAndUpdateSprite(bottomObject);
        
            // Only calculate and award points if combo hasn't been done yet
            if(!topObject.ComboDone)
            {
                // Calculamos y sumamos los puntos por cada tile en el que se hace combo
                totalCombo += bottomObject.MakeCombo(positionToOccupy.ToArray());

                // Si hubo al menos un tile en el que se hizo combo, gastamos el combo tambien en el objeto de arriba
                if (totalCombo > 0)
                {
                    topObject.MakeCombo();
                    
                    // Add tag bonus to combo points
                    int comboPoints = totalCombo;
                    if (tagBonus > 0)
                    {
                        comboPoints += tagBonus;
                    }
                    
                    PlayerController.instance.Inventory.UpdateMoney(comboPoints);
                    House.instance.UpdateScore(comboPoints);
                    
                    // Show combo popup immediately
                    ComboPopUp.Create(popUpPrefab, comboPoints, finalPos, new Vector2(0f, 1.2f));
                    
                    // If there's also a tag bonus, show it after a delay
                    if (tagBonus > 0)
                    {
                        StartCoroutine(ShowMatchPopupDelayed(tagBonus, finalPos));
                    }
                }
                else if (tagBonus > 0)
                {
                    // If there's no combo but there is a tag bonus, add it separately
                    PlayerController.instance.Inventory.UpdateMoney(tagBonus);
                    House.instance.UpdateScore(tagBonus);
                
                    ComboPopUp.Create(matchPrefab, tagBonus, finalPos, new Vector2(0f, 1.2f));
                }
            }
        
            // Updateamos la placement data con el objeto que pusimos encima
            PlacementDatasInPosition[finalPos].PlaceObjectOnTop(positionToOccupy, topObject);
        }
    }
    
    private IEnumerator ShowMatchPopupDelayed(int tagBonus, Vector2 position)
    {
        // Wait for half a second
        yield return new WaitForSeconds(0.7f);
        
        // Show the match popup
        ComboPopUp.Create(matchPrefab, tagBonus, position, new Vector2(0f, 1f)); // Slightly higher position
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