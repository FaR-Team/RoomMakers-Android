using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using Utils;

public class Interactor : MonoBehaviour
{
    [FormerlySerializedAs("doorLayer")] [SerializeField] private LayerMask interactableLayer;
    [SerializeField] private TextMeshProUGUI text_name;

    public void Interact(Inventory playerInventory)
    {
        var gridPosition = GridManager.PositionToCellCenter(transform.position);
        if (House.instance.currentRoom.roomFurnitures.PlacementDatasInPosition.TryGetValue(gridPosition, out PlacementData placementData))
        {
            if (playerInventory.HasItem()) return;
 
            if (placementData.stackedItems != null && placementData.stackedItems.Count > 0)
            {
                FurnitureObjectBase topStackedItem = placementData.PickUpTopStackedItem();
            
                if (topStackedItem != null)
                {        
                    topStackedItem.Data.currentStackLevel = 0;
                
                    playerInventory.SetItemWithData(topStackedItem.Data);
                    playerInventory.EnablePackageUI(true);
                
                    Destroy(topStackedItem.gameObject);
                
                    AudioManager.instance.PlaySfx(GlobalSfx.Grab);
                    return;
                }
            }

            FurnitureData topFurnitureData = placementData.GetTopFurnitureData(gridPosition);
            
            if (topFurnitureData == null && placementData.furnitureData != null)
            {
                if (placementData.topPlacementDatas.Count == 0)
                {
                    MainRoom.instance.availableTiles += placementData.furnitureData.originalData.size.x *
                                                        placementData.furnitureData.originalData.size.y;
                    playerInventory.SetItemWithData(placementData.furnitureData);
                    playerInventory.EnablePackageUI(true);
                    House.instance.currentRoom.roomFurnitures.RemoveDataInPosition(gridPosition);
                }
                else
                {
                    topFurnitureData = placementData.GetAndClearFirstObject();
                    
                    playerInventory.SetItemWithData(topFurnitureData);
                    playerInventory.EnablePackageUI(true);
                }
            }
            else 
            {
                playerInventory.SetItemWithData(topFurnitureData);
                playerInventory.EnablePackageUI(true);
                House.instance.currentRoom.roomFurnitures.RemoveTopObjectInPosition(gridPosition);
            }

            
            AudioManager.instance.PlaySfx(GlobalSfx.Grab);

            return;
        }
        
        if (House.instance.currentRoom.roomFurnitures.KitsInPosition.TryGetValue(gridPosition,
                     out KitObject kit) && !playerInventory.HasItem())
        {
            playerInventory.SetItemWithData(kit.Data); ;
            playerInventory.EnablePackageUI(true);
            House.instance.currentRoom.roomFurnitures.RemoveKitInPosition(gridPosition);
            
            return;
        }

        var interactable = Physics2D.OverlapCircle(transform.position, 0.2f, interactableLayer);

        if (!interactable) return;
        
        if(interactable.TryGetComponent(out DoorData doorData)){
            // Desbloquear habitacion si hay guita
            doorData.BuyNextRoom();
            PlayerController.instance.CheckInFront();
        }
        else if(interactable.TryGetComponent(out ShopItem shopItemData))
        {
            // Comprar si hay guita
            shopItemData.TryPurchase();
            PlayerController.instance.CheckInFront();
        }
        else if (interactable.TryGetComponent(out RestockMachine machine))
        {
            machine.TryRestock();
            PlayerController.instance.CheckInFront();
        }
    }
}
