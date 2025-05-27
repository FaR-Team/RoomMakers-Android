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

    public static event Action<FurnitureOriginalData> OnFurniturePickUp; 

    public void UpdateMoney(int intMoney)
    {
        money += intMoney;
        if(money < 0) moneyText.text = "0";
        else moneyText.text = money.ToString();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Package")) return;

        if (HasItem()) return;
        
        TimerManager.StopTimer();

        furnitureInventory = Package._furnitureInPackage;
        Package.package.SetActive(false);
        EnablePackageUI(true);
        OnFurniturePickUp?.Invoke(furnitureInventory);
        
        UpdatePackageUI();
    }

    public void EnablePackageUI(bool enabled)
    {
        packageUI.SetActive(enabled);
        
        if (enabled)
        {
            UpdatePackageUI();
        }
    }

    private void UpdatePackageUI()
    {
        bool isSpanish = false;
        if (LocalizationManager.Instance != null)
        {
            isSpanish = LocalizationManager.Instance.IsSpanish;
        }

        if (furnitureInventory != null)
        {
            text_name.text = isSpanish ? furnitureInventory.es_Name.ToUpper() : furnitureInventory.Name.ToUpper();

            if (furnitureInventory is ItemData itemData && itemImage != null)
            {
                itemImage.sprite = itemData.ShopSprite;
            }
            else
            {
                itemImage.sprite = PackageSprite;
            }
        }

        if (furnitureInventoryWithData != null)
        {
            text_name.text = isSpanish ? furnitureInventoryWithData.originalData.es_Name.ToUpper() : furnitureInventoryWithData.originalData.Name.ToUpper();

            if (furnitureInventoryWithData.originalData is ItemData itemData && itemImage != null)
            {
                itemImage.sprite = itemData.ShopSprite;
            }
            else
            {
                itemImage.sprite = PackageSprite;
            }
        }
    }

    public bool HasItem()
    {
        return furnitureInventoryWithData != null || furnitureInventory != null;
    }
}
