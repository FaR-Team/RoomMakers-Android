using UnityEngine;
using TMPro;

public class ShopItem : MonoBehaviour
{
    [SerializeField] private ItemData itemData;
    [SerializeField] private int price;
    
    
    public void Initialize(ItemData item, int itemPrice)
    {
        itemData = item;
        price = itemPrice;
    }
    
    public void TryPurchase()
    {
        if (itemData == null) return;
        
        if (PlayerController.instance.Inventory.money >= price)
        {
            // Add item to inventory
            if (PlayerController.instance.Inventory.furnitureInventory == null && PlayerController.instance.Inventory.furnitureInventoryWithData == null)
            {
                // Deduct money
                PlayerController.instance.Inventory.UpdateMoney(-price);
                
                PlayerController.instance.Inventory.furnitureInventory = itemData;
                
                // Show the package UI with the purchased item
                PlayerController.instance.Inventory.EnablePackageUI(true);
                
                // Remove the shop item
                Destroy(gameObject);
            }
            else
            {
                // Player already has an item, can't buy
                Debug.Log("Inventory full, can't purchase item");
            }
        }
        else
        {
            // Not enough money
            Debug.Log("Not enough money to purchase item");
        }
    }

    public ItemData GetItemData()
    {
        return itemData;
    }

    public int GetPrice()
    {
        return price;
    }
}