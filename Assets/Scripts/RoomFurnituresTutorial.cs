using System.Collections.Generic;
using UnityEngine;

public class RoomFurnituresTutorial : RoomFurnitures
{
    private bool firstCombo = false;
    protected override void GivePoints(FurnitureData data, List<Vector2> positionToOccupy, bool placeOnTop, Vector2 finalPos, FurnitureObjectBase furnitureObject)
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
            

            PlacementDatasInPosition[finalPos].occupiedPositions.ForEach(pos =>
            {
                PlacementDatasInPosition[pos].instantiatedFurnitureOnTop = topObject;
                PlacementDatasInPosition[pos].furnitureOnTopData = topObject.Data;
            });
        }
    }
}