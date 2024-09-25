using System;
using TMPro;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    // TODO: fix unnecessary double data

    [SerializeField] private bool spanish;
    public FurnitureOriginalData furnitureInventory;
    public FurnitureData furnitureInventoryWithData;
    public int money;
    public TextMeshProUGUI moneyText;
    public GameObject packageUI;
    [SerializeField] private TextMeshProUGUI text_name;
    


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
        if (!spanish) text_name.text = furnitureInventory.Name;
        else text_name.text = furnitureInventory.es_Name;
    }

    public void EnablePackageUI(bool enabled)
    {
        packageUI.SetActive(enabled);
        
    }
}
