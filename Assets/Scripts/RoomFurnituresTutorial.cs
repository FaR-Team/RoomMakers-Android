using System.Collections.Generic;
using UnityEngine;

public class RoomFurnituresTutorial : RoomFurnitures
{
    private bool firstCombo = false;
    protected override void GivePoints(FurnitureData data, List<Vector2> positionToOccupy, bool placeOnTop, Vector2 finalPos, FurnitureObjectBase furnitureObject)
    {
        // Get the room component
        Room currentRoom = GetComponent<Room>();
        
        // Check if this furniture has a tag and the room doesn't have one yet
        RoomTag furnitureTag = data.originalData.furnitureTag;
        if (furnitureTag != RoomTag.None && currentRoom != null)
        {
            // Try to set the room tag from furniture
            if (currentRoom.TrySetRoomTagFromFurniture(furnitureTag))
            {
                data.hasReceivedTagBonus = true;
            }
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
                PlayerController.instance.Inventory.UpdateMoney(tagBonus);
                House.instance.UpdateScore(tagBonus);
                ComboPopUp.Create(matchPrefab, tagBonus, finalPos, new Vector2(0f, 1.5f));
            }
        }
        
        if (!placeOnTop)
        {
            if (TryGetComponent(out MainRoom room))
            {
                MainRoom.instance.availableTiles -= data.originalData.size.x * data.originalData.size.y;
            }
            
            if (furnitureObject.originalData is not ItemData) PlacementDatasInPosition[finalPos].instantiatedFurniture = furnitureObject;
            //else PlacementDatasInPosition[finalPos].instantiatedBaseObject = furnitureObject;  // If just placed a kit
            
            //positionToOccupy.ForEach(pos => PlacementDatasInPosition[pos].instantiatedFurniture = furnitureObject);
        }
        else
        {
            // Si el objeto va encima de otro, lo guardamos en el PlacementData y damos doble score por combo
            // TODO: REWORK
            
            int totalCombo = 0;
            
            if (!firstCombo)
            {
                firstCombo = true;
                TutorialHandler.instance.CompletedStep();
            }
            
            TopFurnitureObject topObject = (TopFurnitureObject) furnitureObject;
            BottomFurnitureObject bottomObject = (BottomFurnitureObject) PlacementDatasInPosition[finalPos]
                .instantiatedFurniture;
            
            // Check for sprite changes regardless of combo status
            topObject.CheckAndUpdateSprite(bottomObject);
            
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
                    
                    ComboPopUp.Create(popUpPrefab, totalCombo, finalPos, new Vector2(0f, 1.2f));
                }
            }
            

            PlacementDatasInPosition[finalPos].PlaceObjectOnTop(positionToOccupy, topObject);
            /*
            PlacementDatasInPosition[finalPos].occupiedPositions.ForEach(pos =>
            {
                PlacementDatasInPosition[pos].instantiatedFurnitureOnTop = topObject;
                PlacementDatasInPosition[pos].furnitureOnTopData = topObject.Data;
            });*/
        }
    }
}