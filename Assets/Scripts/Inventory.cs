using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
    // TODO: fix unnecessary double data

    public FurnitureOriginalData furnitureInventory;
    public FurnitureData furnitureInventoryWithData;
    public int money;
    public TextMeshProUGUI moneyText;
    public GameObject packageUI;
    [SerializeField] private TextMeshProUGUI text_name;
    [SerializeField] private SpriteRenderer itemImage;
    public Sprite PackageSprite;

    public void UpdateMoney(int intMoney)
    {
        money += intMoney;
        if(money < 0) moneyText.text = "0";
        else moneyText.text = money.ToString();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Package")) return;

        if (furnitureInventory != null || furnitureInventoryWithData != null) return;
        
        if(TutorialHandler.instance) TutorialHandler.instance.CompletedStep();
        TimerManager.StopTimer();

        furnitureInventory = Package._furnitureInPackage;
        Package.package.SetActive(false);
        EnablePackageUI(true);
        
        // Update UI with package information
        UpdatePackageUI();
    }

    public void EnablePackageUI(bool enabled)
    {
        packageUI.SetActive(enabled);
        
        // If enabling the UI, update it with current item information
        if (enabled)
        {
            UpdatePackageUI();
        }
    }
    
    // New method to update the package UI with the current item's information
    private void UpdatePackageUI()
    {
        // Get language preference safely
        bool isSpanish = false;
        if (LocalizationManager.Instance != null)
        {
            isSpanish = LocalizationManager.Instance.IsSpanish;
        }
        
        // Set the text based on language and item type
        if (furnitureInventory != null)
        {
            // Set the name text
            text_name.text = isSpanish ? furnitureInventory.es_Name : furnitureInventory.Name;
            
            if (furnitureInventory is ItemData itemData && itemImage != null)
            {
                // Set the sprite from ItemData's ShopSprite
                itemImage.sprite = itemData.ShopSprite;
            }
            else
            {
                itemImage.sprite = PackageSprite;
            }
        }
    }
}
