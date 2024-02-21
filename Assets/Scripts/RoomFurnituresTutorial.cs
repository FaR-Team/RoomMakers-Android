using System.Collections.Generic;
using UnityEngine;

public class RoomFurnituresTutorial : RoomFurnitures
{
    private bool firstCombo = false;

    protected override void GivePoints(FurnitureOriginalData data, List<Vector2> positionToOccupy, bool placeOnTop, Vector2 finalPos, GameObject furniturePrefab)
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
            if (!firstCombo)
            {
                firstCombo = true;
                TutorialHandler.instance.CompletedStep();
            }
            PlayerController.instance.Inventory.UpdateMoney(data.price * 2);
            House.instance.UpdateScore(data.price * 2);

            PlacementDatasInPosition[finalPos].occupiedPositions.ForEach(pos =>
            {
                PlacementDatasInPosition[pos].instantiatedFurnitureOnTop = furniturePrefab;
                PlacementDatasInPosition[pos].furnitureOnTop = data;
            });
        }
    }
}