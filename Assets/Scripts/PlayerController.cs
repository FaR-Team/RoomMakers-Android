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
    private ILookable currentLookTarget;
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
        if (!playerInput.Movement.SwitchMode.WasPressedThisFrame() || (IsMoving && StateManager.CurrentGameState == GameState.Moving) || StateManager.CurrentGameState == GameState.Pause) return;

        FurnitureData furnitureData = null;
        
        if (inventory.furnitureInventoryWithData != null)
        {
            furnitureData = inventory.furnitureInventoryWithData;
        }
        else
        {
            if (inventory.furnitureInventory)
            {
                furnitureData = new FurnitureData(inventory.furnitureInventory);
            }
        }

        if (furnitureData == null) return;
        
        StateManager.SwitchEditMode();
        bool isNowEditing = StateManager.IsEditing();

        if (EditingManager.Instance != null)
        {
            if (isNowEditing)
            {
                EditingManager.Instance.SetCurrentlyHeldItem(furnitureData.originalData);
            }
            else
            {
                EditingManager.Instance.ClearCurrentlyHeldItem();
            }
        }

        foreach (var furniturePreview in furniturePreviews)
        {
            if (furniturePreview == furniturePreviews[(int)furnitureData.originalData.typeOfSize])
            {
                if (isNowEditing)
                {
                    furniturePreview.SetCurrentFurnitureData(furnitureData);
                    furniturePreview.gameObject.SetActive(true);
                }
                else
                {
                    furniturePreview.gameObject.SetActive(false);
                }
            }
            else
            {
                furniturePreview.gameObject.SetActive(false);
            }
        }
    }

    public void ForceSwitchEditingMode()
    {
        FurnitureData furnitureData = null;
        
        if (inventory.furnitureInventoryWithData != null)
        {
            furnitureData = inventory.furnitureInventoryWithData;
        }
        else
        {
            if (inventory.furnitureInventory)
            {
                furnitureData = new FurnitureData(inventory.furnitureInventory);
            }
        }

        if (furnitureData == null) return;
        
        StateManager.SwitchEditMode();
        bool isNowEditing = StateManager.IsEditing();

        if (EditingManager.Instance != null)
        {
            if (isNowEditing)
            {
                EditingManager.Instance.SetCurrentlyHeldItem(furnitureData.originalData);
            }
            else
            {
                EditingManager.Instance.ClearCurrentlyHeldItem();
            }
        }

        foreach (var furniturePreview in furniturePreviews)
        {
            if (furniturePreview == furniturePreviews[(int)furnitureData.originalData.typeOfSize])
            {
                if (isNowEditing)
                {
                    furniturePreview.SetCurrentFurnitureData(furnitureData);
                    furniturePreview.gameObject.SetActive(true);
                }
                else
                {
                    furniturePreview.gameObject.SetActive(false);
                }
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
        var hit = Physics2D.Raycast(transform.position, transform.up, 1f, 1 << 10 | 1 << 13 | 1 << 16); // TODO: Layermask para que sea mas claro
        if (currentLookTarget != null) ClearLookable();
        
        if (hit.collider != null)
        {
            Vector3 costPosition = GetCostTextPosition();
            costCanvas.transform.position = costPosition;

            if (hit.collider.TryGetComponent(out DoorData doorData))
            {
                costText.text = House.instance.DoorPrice.ToString();
                costCanvas.SetActive(true);
            }
            else if (hit.collider.TryGetComponent(out ShopItem shopItem))
            {
                ItemData itemData = shopItem.GetItemData();
                if (itemData != null)
                {
                    costText.text = shopItem.GetPrice().ToString();
                    costCanvas.SetActive(true);
                }
            }
            else if (hit.collider.TryGetComponent(out ILookable lookable)) // TODO: crear los demas Lookable para door y shop
            {
                costCanvas.SetActive(false); // Turn off player's canvas
                currentLookTarget = lookable;
                lookable.StartLook(transform);
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

    void ClearLookable()
    {
        currentLookTarget.EndLook();
        currentLookTarget = null;
    }
}