using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.InputSystem;

public class FurniturePreview : MonoBehaviour
{
    public FurnitureOriginalData data;

    [SerializeField] private FurnitureData furnitureData;
    [SerializeField] private Inventory inventory;

    private Vector2Int originalSize;
    private bool firstTimePlacingFurniture = false; 

    int rotation = 0;
    private Vector2Int position;

    private void Awake()
    {
        //SetFurnitureData();
        //furnitureData = new FurnitureData();
    }

    private void SetFurnitureData()
    {
        originalSize = data.size;
        furnitureData.size = data.size;
        furnitureData.prefab = data.prefab;
        furnitureData.originalData = data;
    }

    public void SetCurrentFurnitureData(FurnitureData newData, bool firstTimePlacing)
    {
        furnitureData = newData; 
        data = furnitureData.originalData;
        originalSize = newData.originalData.size;
        rotation = furnitureData.rotationStep;
        firstTimePlacingFurniture = firstTimePlacing; // To give points and score when placing furniture for the first time, and avoid adding this to every object's data
    }

    private void OnEnable()
    {
        //SetFurnitureData();
        CheckRotation();

        var playerPos = GameObject.FindGameObjectWithTag("Player").transform.position;

        if(Mathf.Abs(playerPos.x - Mathf.Floor(playerPos.x)) != 0.5f)
        {
            playerPos.x = Mathf.Floor(playerPos.x) + 0.5f;
        }
        if (Mathf.Abs(playerPos.y - Mathf.Floor(playerPos.y)) != 0f)
        {
            playerPos.y = Mathf.Floor(playerPos.y);
        }

        transform.position = playerPos;
    }

    public void PutFurniture()
    {
        Vector3Int cellPos = GridManager._grid.WorldToCell(transform.position);
        position.x = cellPos.x;
        position.y = cellPos.y;

        bool placeFurniture = House.instance.currentRoom.roomFurnitures.
               PlaceFurniture(new Vector2(transform.position.x, transform.position.y), furnitureData);

        gameObject.SetActive(!placeFurniture);

        if (placeFurniture)
        {
            if (firstTimePlacingFurniture)
            {
                PlayerController.instance.Inventory.UpdateMoney(furnitureData.originalData.price);
                House.instance.UpdateScore(furnitureData.originalData.price);
                firstTimePlacingFurniture = false;
            }
            
            AudioManager.instance.PlaySfx(GlobalSfx.Click);
            inventory.furnitureInventory = null;
            inventory.furnitureInventoryWithData = null;
            inventory.packageUI.SetActive(false);
            if(StateManager.currentGameState != GameState.Pause) StateManager.SwitchEditMode();
        }
        else
        {
            AudioManager.instance.PlaySfx(GlobalSfx.Error);
        }
    }

    public void Rotate()
    {
        rotation++;

        if (rotation >= 4)
            rotation = 0;

        furnitureData.rotationStep = rotation;
        CheckRotation();
    }

    void CheckRotation()
    {
        switch (rotation)
        {
            case 0:
                furnitureData.VectorRotation = new Vector3Int(0, 0, 0);

                furnitureData.size = originalSize;
                break;
            case 1:
                furnitureData.VectorRotation = new Vector3Int(0, 0, 90);

                furnitureData.size.x = -originalSize.y;
                furnitureData.size.y = originalSize.x;
                break;
            case 2:
                furnitureData.VectorRotation = new Vector3Int(0, 0, 180);

                furnitureData.size = -originalSize;
                break;
            case 3:
                furnitureData.VectorRotation = new Vector3Int(0, 0, 270);

                furnitureData.size.x = originalSize.y;
                furnitureData.size.y = -originalSize.x;
                break;
            default:
                break;
        }

        transform.rotation = Quaternion.Euler(furnitureData.VectorRotation);
    }
}
public class FurnitureData
{
    public FurnitureData(FurnitureOriginalData originalData)
    {
        this.originalData = originalData;
        size = originalData.size;
        prefab = originalData.prefab;
        VectorRotation = Vector3Int.zero;
        rotationStep = 0;
        comboDone = false;
        localTileCombos = new();
    }

    public FurnitureData()
    {
        // Create empty data
    }
        
    public FurnitureOriginalData originalData;
    public Vector2Int size;
    public GameObject prefab;
    public Vector3Int VectorRotation;
    public int rotationStep;
    public bool comboDone;
    public HashSet<Vector2Int> localTileCombos;
}