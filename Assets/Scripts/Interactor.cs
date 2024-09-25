using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Interactor : MonoBehaviour
{
    [SerializeField] private bool spanish;
    [SerializeField] private LayerMask doorLayer;
    [SerializeField] private TextMeshProUGUI text_name;
    public void Interact(Inventory playerInventory)
    {
        var gridPosition = GridManager.PositionToCellCenter(transform.position);
        // Si existe un PlacementData en el punto de interaccion, 
        if (House.instance.currentRoom.roomFurnitures.PlacementDatasInPosition.TryGetValue(gridPosition, out PlacementData placementData))
        {
            //Debug.Log("hay placement data en: " + (Vector2)gridPosition);
            if (playerInventory.furnitureInventory != null || playerInventory.furnitureInventoryWithData != null) return;
 

            if (placementData.furnitureOnTopData == null && placementData.furnitureData != null)
            {
                if (!spanish) text_name.text = placementData.furnitureData.originalData.Name;
                else text_name.text = placementData.furnitureData.originalData.es_Name;
                MainRoom.instance.availableTiles += placementData.furnitureData.originalData.size.x * placementData.furnitureData.originalData.size.y;
                playerInventory.furnitureInventoryWithData = placementData.furnitureData;
                playerInventory.EnablePackageUI(true);
                //PlayerController.instance.Inventory.UpdateMoney(-placementData.furnitureData.originalData.price);
                //House.instance.UpdateScore(-placementData.furnitureData.originalData.price);
                Destroy(placementData.instantiatedFurniture.gameObject);
                House.instance.currentRoom.roomFurnitures.RemoveDataInPositions(placementData.occupiedPositions);

            }
            else 
            {
                if (!spanish) text_name.text = placementData.furnitureOnTopData.originalData.Name;
                else text_name.text = placementData.furnitureOnTopData.originalData.es_Name;
                playerInventory.furnitureInventoryWithData = placementData.furnitureOnTopData;
                playerInventory.EnablePackageUI(true);
                //PlayerController.instance.Inventory.UpdateMoney(-placementData.furnitureOnTopData.originalData.priceCombo);
                //House.instance.UpdateScore(-placementData.furnitureOnTopData.originalData.priceCombo);
                Destroy(placementData.instantiatedFurnitureOnTop.gameObject);
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
