using System;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class FurnitureInfoMenu : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] GameObject menuPanel;
    [SerializeField] TextMeshProUGUI furnitureName;
    [SerializeField] TextMeshProUGUI furnitureDescription;
    [SerializeField] Image tagSprite;
    [SerializeField] private FurnitureInfoSprite[] infoSprites;
    [SerializeField] RoomTagDisplay tagDisplay;
    
    private FurnitureOriginalData currentFurniture;

    private void Start()
    {
        menuPanel.SetActive(false);
    }

    private void OnEnable()
    {
        PlayerController.instance.playerInput.Movement.Start.performed += ToggleMenu;
        Inventory.OnInventoryChanged += SetFurnitureData;
        //UpdateMenuDisplay();
    }

    private void OnDisable()
    {
        PlayerController.instance.playerInput.Movement.Start.performed -= ToggleMenu;
        Inventory.OnInventoryChanged -= SetFurnitureData;
    }

    public void SetFurnitureData(FurnitureOriginalData furniture)
    {
        currentFurniture = furniture;
        UpdateMenuDisplay();
    }

    private void UpdateMenuDisplay()
    {
        if (currentFurniture == null) return;

        bool isSpanish = false;
        if (LocalizationManager.Instance != null)
        {
            isSpanish = LocalizationManager.Instance.IsSpanish;
        }

        if (furnitureName != null)
        {
            furnitureName.text = isSpanish ? currentFurniture.es_Name.ToUpper() : currentFurniture.Name.ToUpper();
        }

        if (furnitureDescription != null)
        {
            furnitureDescription.text = isSpanish ? currentFurniture.es_Description.ToUpper() : currentFurniture.Description.ToUpper();
        }

        infoSprites[(int)currentFurniture.typeOfSize]?.UpdateSprites(currentFurniture.sprites);
        tagSprite.sprite = tagDisplay.GetTagSprite(currentFurniture.furnitureTag);
    }

    public void ToggleMenu(InputAction.CallbackContext ctx)
    {
        if (currentFurniture == null || TutorialHandler.instance != null) return;
        
        if(menuPanel.activeSelf) CloseMenu();
        else OpenMenu();
    }

    void OpenMenu()
    {
        StateManager.PauseGame();
        menuPanel.SetActive(true);
        infoSprites[(int)currentFurniture.typeOfSize]?.gameObject.SetActive(true);
    }

    void CloseMenu()
    {
        StateManager.Resume();
        menuPanel.SetActive(false);
        for(int i = 0; i < infoSprites.Length; i++) infoSprites[i].gameObject.SetActive(false);
    }
}