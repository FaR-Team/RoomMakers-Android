using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.InputSystem;

public class FurniturePreview : MonoBehaviour
{
    public FurnitureOriginalData data;

    [SerializeField] private FurnitureData furnitureData;
    [SerializeField] private Inventory inventory;

    private Vector2Int originalSize;

    int rotation = 0;
    private Vector2Int position;

    private void Awake()
    {
        SetFurnitureData();
    }

    private void SetFurnitureData()
    {
        originalSize = data.size;
        furnitureData.size = data.size;
        furnitureData.prefab = data.prefab;
        furnitureData.compatibles = data.compatibles;
        furnitureData.originalData = data;
    }

    private void OnEnable()
    {
        SetFurnitureData();
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
            AudioManager.instance.PlaySfx(GlobalSfx.Click);
            inventory.furnitureInventory = null;
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
public struct FurnitureData
{
    public FurnitureOriginalData originalData;
    public Vector2Int size;
    public GameObject prefab;
    public FurnitureOriginalData[] compatibles;
    public Vector3Int VectorRotation;
}