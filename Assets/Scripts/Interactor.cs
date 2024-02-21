using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Interactor : MonoBehaviour
{
    [SerializeField] private LayerMask doorLayer;
    [SerializeField] private TextMeshProUGUI text_name;
    public void Interact(Inventory playerInventory)
    {
        var gridPosition = GridManager.PositionToCellCenter(transform.position);
        // Si existe un PlacementData en el punto de interaccion, 
        if (House.instance.currentRoom.roomFurnitures.PlacementDatasInPosition.TryGetValue(gridPosition, out PlacementData placementData))
        {
            //Debug.Log("hay placement data en: " + (Vector2)gridPosition);
            if (playerInventory.furnitureInventory != null) return;
 

            if (placementData.furnitureOnTop == null)
            {
                text_name.text = placementData.furniture.Name;
                MainRoom.instance.availableTiles += placementData.furniture.size.x * placementData.furniture.size.y;
                playerInventory.furnitureInventory = placementData.furniture;
                playerInventory.EnablePackageUI(true);
                PlayerController.instance.Inventory.UpdateMoney(-placementData.furniture.price);
                House.instance.UpdateScore(-placementData.furniture.price);
                Destroy(placementData.instantiatedFurniture);
                House.instance.currentRoom.roomFurnitures.RemoveDataInPositions(placementData.occupiedPositions);

            }
            else
            {
                text_name.text = placementData.furnitureOnTop.Name;
                playerInventory.furnitureInventory = placementData.furnitureOnTop;
                playerInventory.EnablePackageUI(true);
                PlayerController.instance.Inventory.UpdateMoney(-placementData.furnitureOnTop.priceCombo);
                House.instance.UpdateScore(-placementData.furnitureOnTop.priceCombo);
                Destroy(placementData.instantiatedFurnitureOnTop);
                placementData.instantiatedFurnitureOnTop = null;
                House.instance.currentRoom.roomFurnitures.RemoveTopObjectInPositions(placementData.occupiedPositions);
            }

            
            AudioManager.instance.PlaySfx(GlobalSfx.Grab);

            return;
        }

        var door = Physics2D.OverlapCircle(transform.position, 0.2f, doorLayer);

        if (!door) return;
        
        // Desbloquear habitacion si hay guita
        door.TryGetComponent(out DoorData doorData);

        if (doorData)
        {
            doorData.BuyNextRoom();
            PlayerController.instance.CheckInFront();
        }
    }
}
