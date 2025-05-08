using System;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;

public class PlayerController : MovementController
{
    public static PlayerController instance;
    [SerializeField] private Animator anim;
    [SerializeField] private FurniturePreview[] furniturePreviews;
    [SerializeField] private Inventory inventory;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private GameObject costCanvas;
    private bool reachedPosition;
    public Inventory Inventory => inventory;
    [SerializeField] private Interactor interactor;

    public Controls playerInput;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }

        playerInput = new Controls();
        playerInput.Enable();
    }

    private void OnEnable()
    {
        StateManager.OnStateChanged += HandleStateChanged;
    }
    
    private void OnDisable()
    {
        StateManager.OnStateChanged -= HandleStateChanged;
    }

    private void HandleStateChanged(GameState newState)
    {
        if (newState is GameState.Editing or GameState.Pause)
        {
            anim.SetBool("IsWalking", false);
        }
    }

    void Update()
    {
        SwitchEditingMode();

        if (StateManager.IsPaused()) return;
        if (StateManager.IsEditing()) return;
        if (StateManager.IsGameOver()) return;

        Animate();
        MoveObject(playerInput);
        CheckInteract();
    }

    private void SwitchEditingMode()
    {
        if (!playerInput.Movement.SwitchMode.WasPressedThisFrame() || (IsMoving && StateManager.currentGameState == GameState.Moving) || StateManager.currentGameState == GameState.Pause) return;

         // TODO: stop calling this everytime Space is pressed, do this only when turning edit mode on and object isn't updated
        
        // Create new furniture data if placing object for the first time
        FurnitureData furnitureData = null;
        bool firstTimePlacing = false;
        
        if (inventory.furnitureInventoryWithData != null)
        {
            furnitureData = inventory.furnitureInventoryWithData;
        }
        else
        {
            if (inventory.furnitureInventory)
            {
                firstTimePlacing = true; // If there's no data in the inventory object, we're placing it for the first time, so we tell the furniturePreview to reward points when placed
                furnitureData = new FurnitureData(inventory.furnitureInventory);
            }
        }

        // Return if no data in the inventory
        if (furnitureData == null) return;
        
        StateManager.SwitchEditMode();

        //Debug.Log($"Game State: {StateManager.currentGameState}");


        foreach (var furniturePreview in furniturePreviews)
        {
            if (furniturePreview == furniturePreviews[(int)furnitureData.originalData.typeOfSize])
            {
                furniturePreview.SetCurrentFurnitureData(furnitureData, firstTimePlacing);
                furniturePreview.gameObject.SetActive(!furniturePreview.gameObject.activeInHierarchy);
            }
            else
            {
                furniturePreview.gameObject.SetActive(false);
            }
        }
    }

    public void ForceSwitchEditingMode()
    {
        // Create new furniture data if placing object for the first time
        FurnitureData furnitureData = null;
        bool firstTimePlacing = false;
        
        if (inventory.furnitureInventoryWithData != null)
        {
            furnitureData = inventory.furnitureInventoryWithData;
        }
        else
        {
            if (inventory.furnitureInventory)
            {
                firstTimePlacing = true; // If there's no data in the inventory object, we're placing it for the first time, so we tell the furniturePreview to reward points when placed
                furnitureData = new FurnitureData(inventory.furnitureInventory);
            }
        }

        // Return if no data in the inventory
        if (furnitureData == null) return;
        
        StateManager.SwitchEditMode();

        //Debug.Log($"Game State: {StateManager.currentGameState}");


        foreach (var furniturePreview in furniturePreviews)
        {
            if (furniturePreview == furniturePreviews[(int)furnitureData.originalData.typeOfSize])
            {
                furniturePreview.SetCurrentFurnitureData(furnitureData, firstTimePlacing);
                furniturePreview.gameObject.SetActive(!furniturePreview.gameObject.activeInHierarchy);
            }
            else
            {
                furniturePreview.gameObject.SetActive(false);
            }
        }
    }
    private void CheckInteract()
    {
        if(playerInput.Movement.Interact.WasPressedThisFrame() && !IsMoving)
        {
            interactor.Interact(inventory);
        }
    }

    public void CheckInFront()
    {

        var hit = Physics2D.Raycast(transform.position, transform.up, 1f, 1 << 10 | 1 << 13 | 1 << 16);

        if (hit.collider != null)
        {
            Vector3 costPosition = GetCostTextPosition();
            costCanvas.transform.position = costPosition;

            Color backgroundColor = SampleBackgroundColorAt(costPosition);
            costText.color = GetInvertedColor(backgroundColor);

            // Check if it's a door
            if (hit.collider.TryGetComponent(out DoorData doorData))
            {
                costText.text = House.instance.DoorPrice.ToString();
                costCanvas.SetActive(true);
            }
            // Check if it's a shop item
            else if (hit.collider.TryGetComponent(out ShopItem shopItem))
            {
                ItemData itemData = shopItem.GetItemData();
                if (itemData != null)
                {
                    costText.text = shopItem.GetPrice().ToString();
                    costCanvas.SetActive(true);
                }
            }
            else if (hit.collider.TryGetComponent(out RestockMachine machine))
            {
                costText.text = House.instance.RestockPrice.ToString();
                costCanvas.SetActive(true);
            }
        }
        else
        {
            costCanvas.SetActive(false);
        }
    }

    private Vector3 GetCostTextPosition()
    {
        Vector3 offset = Vector3.zero;
        
        if (transform.up == Vector3.up)
            offset = new Vector3(0, -1, 0);
        else if (transform.up == Vector3.down)
            offset = new Vector3(0, 1, 0);
        else if (transform.up == Vector3.left)
            offset = new Vector3(0.6f, 1, 0);
        else if (transform.up == Vector3.right)
            offset = new Vector3(-1, 1, 0);

        return transform.position + offset;
    }

    private Color GetInvertedColor(Color backgroundColor)
    {
        return new Color(1 - backgroundColor.r, 1 - backgroundColor.g, 1 - backgroundColor.b);
    }

    private Color SampleBackgroundColorAt(Vector3 position)
    {
        RenderTexture tempRT = RenderTexture.GetTemporary(1, 1, 0);
        Camera mainCamera = Camera.main;
        
        RenderTexture prevRT = mainCamera.targetTexture;
        
        mainCamera.targetTexture = tempRT;
        
        int prevCullingMask = mainCamera.cullingMask;
        
        mainCamera.cullingMask = prevCullingMask & ~(1 << LayerMask.NameToLayer("UI"));
        
        mainCamera.Render();
        
        Texture2D texture = new Texture2D(1, 1, TextureFormat.RGB24, false);
        
        RenderTexture prevActive = RenderTexture.active;
        
        RenderTexture.active = tempRT;
        
        texture.ReadPixels(new Rect(0, 0, 1, 1), 0, 0);
        texture.Apply();
        
        Color backgroundColor = texture.GetPixel(0, 0);
        
        RenderTexture.active = prevActive;
        mainCamera.targetTexture = prevRT;
        mainCamera.cullingMask = prevCullingMask;
        
        RenderTexture.ReleaseTemporary(tempRT);
    
        Destroy(texture);
        
        return backgroundColor;
    }

    private void Animate()
    {
        if (transform.position != movePoint.position)
        {
            transform.position = Vector3.MoveTowards(transform.position, movePoint.position, moveSpeed * Time.deltaTime);
            anim.SetBool("IsWalking", true);
            reachedPosition = false;
        }
        else
        {
            if (!reachedPosition)
            {
                CheckInFront();
                reachedPosition = true;
            }
            anim.SetBool("IsWalking", false);
            
        }
    }

    protected override void Rotate(Vector2 dir)
    {
        base.Rotate(dir);
        CheckInFront();
    }
}