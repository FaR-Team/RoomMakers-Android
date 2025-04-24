using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Interactor : MonoBehaviour
{
    [SerializeField] private bool spanish;
    [SerializeField] private LayerMask doorLayer;
    [SerializeField] private LayerMask shopLayer;
    [SerializeField] private TextMeshProUGUI text_name;
    public void Interact(Inventory playerInventory)
    {
        var gridPosition = GridManager.PositionToCellCenter(transform.position);
        // Si existe un PlacementData en el punto de interaccion, 
        if (House.instance.currentRoom.roomFurnitures.PlacementDatasInPosition.TryGetValue(gridPosition, out PlacementData placementData))
        {
            //Debug.Log("hay placement data en: " + (Vector2)gridPosition);
            if (playerInventory.furnitureInventory != null || playerInventory.furnitureInventoryWithData != null) return;
 
            FurnitureData topFurnitureData = placementData.GetTopFurnitureData(gridPosition);
            
            if (topFurnitureData == null && placementData.furnitureData != null)
            {
                if (placementData.topPlacementDatas.Count == 0)
                {
                    if (!spanish) text_name.text = placementData.furnitureData.originalData.Name;
                    else text_name.text = placementData.furnitureData.originalData.es_Name;
                    MainRoom.instance.availableTiles += placementData.furnitureData.originalData.size.x *
                                                        placementData.furnitureData.originalData.size.y;
                    playerInventory.furnitureInventoryWithData = placementData.furnitureData;
                    playerInventory.EnablePackageUI(true);
                    House.instance.currentRoom.roomFurnitures.RemoveDataInPosition(gridPosition);
                }
                else
                {
                    topFurnitureData = placementData.GetAndClearFirstObject();
                    
                    if (!spanish) text_name.text = topFurnitureData.originalData.Name;
                    else text_name.text = topFurnitureData.originalData.es_Name;
                    playerInventory.furnitureInventoryWithData = topFurnitureData;
                    playerInventory.EnablePackageUI(true);
                }
            }
            else 
            {
                if (!spanish) text_name.text = topFurnitureData.originalData.Name;
                else text_name.text = topFurnitureData.originalData.es_Name;
                playerInventory.furnitureInventoryWithData = topFurnitureData;
                playerInventory.EnablePackageUI(true);
                House.instance.currentRoom.roomFurnitures.RemoveTopObjectInPosition(gridPosition);
            }

            
            AudioManager.instance.PlaySfx(GlobalSfx.Grab);

            return;
        }

        var door = Physics2D.OverlapCircle(transform.position, 0.2f, doorLayer);

        if (door)
        {
            // Desbloquear habitacion si hay guita
            door.TryGetComponent(out DoorData doorData);

            if (doorData)
            {
                doorData.BuyNextRoom();
                PlayerController.instance.CheckInFront();
            }
            return;
        }

        var item = Physics2D.OverlapCircle(transform.position, 0.2f, shopLayer);

        if (item)
        {
            // Comprar si hay guita
            item.TryGetComponent(out ShopItem shopItemData);
            Debug.Log(shopItemData.name);

            if (shopItemData)
            {
                shopItemData.TryPurchase();
                PlayerController.instance.CheckInFront();
            }
        }
    }
}
