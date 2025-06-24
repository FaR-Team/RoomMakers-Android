using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class FurnitureInfoMenu : MonoBehaviour
{
    [Header("UI References")]
    public GameObject menuPanel;
    public TextMeshProUGUI furnitureName;
    public TextMeshProUGUI furnitureDescription;
    public Image tagSprite;
    public Button startButton;
    
    private FurnitureOriginalData currentFurniture;
    private bool isMenuOpen = false;

    private void Start()
    {
        if (menuPanel != null)
            menuPanel.SetActive(false);
            
        if (startButton != null)
            startButton.onClick.AddListener(ToggleMenu);
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
    }

    public void ToggleMenu()
    {
        if (currentFurniture == null) return;

        isMenuOpen = !isMenuOpen;
        
        if (menuPanel != null)
        {
            menuPanel.SetActive(isMenuOpen);
        }

        if (isMenuOpen)
        {
            UpdateMenuDisplay();
        }
    }

    public void CloseMenu()
    {
        isMenuOpen = false;
        if (menuPanel != null)
        {
            menuPanel.SetActive(false);
        }
    }
}