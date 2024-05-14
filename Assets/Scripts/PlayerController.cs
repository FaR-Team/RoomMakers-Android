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

    private void CheckInteract()
    {
        if(playerInput.Movement.Interact.WasPressedThisFrame() && !IsMoving)
        {
            interactor.Interact(inventory);
        }
    }

    public void CheckInFront()
    {
        var hit = Physics2D.Raycast(transform.position, transform.up, 1f, 1 << 10);

        if (hit.collider != null)
        {
            costText.text = House.instance.DoorPrice.ToString();
            costCanvas.SetActive(true);
            // House.instance.DoorPrice
        }
        else
        {
            costCanvas.SetActive(false);
        }
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