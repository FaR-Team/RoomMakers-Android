using System.Collections.Generic;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.InputSystem;

public class FurniturePreview : MonoBehaviour
{
    public FurnitureOriginalData data;

    [SerializeField] private FurnitureData furnitureData;
    [SerializeField] private Inventory inventory;
    [SerializeField] private FurniturePreviewMovement movement;
    
    [Header("Shake Effect")]
    [SerializeField] private float shakeDuration = 0.2f;
    [SerializeField] private float shakeAmount = 0.05f;
    [SerializeField] private float decreaseFactor = 1.0f;

    private Vector2Int originalSize;
    private bool firstTimePlacingFurniture = false; 

    int rotation = 0;
    private Vector2Int position;
    private Vector3 originalPosition;
    private bool isShaking = false;

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

    public void SetCurrentFurnitureData(FurnitureData newData)
    {
        furnitureData = newData; 
        data = furnitureData.originalData;
        originalSize = newData.originalData.size;
        rotation = furnitureData.rotationStep;
        firstTimePlacingFurniture = newData.firstTimePlaced; // To give points and score when placing furniture for the first time, and avoid adding this to every object's data

        if (furnitureData.originalData is ItemData {type: ItemType.Sledgehammer})
        {
            movement.RemoveDoorLayerToMask();
        }
        else
        {
            movement.AddDoorLayerToMask();
        }
    }

    private void OnEnable()
    {
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

        bool placeFurniture = false;

        
        
        if (furnitureData.originalData is ItemData itemData)
        {
            placeFurniture = House.instance.currentRoom.roomFurnitures.PlaceItem(new Vector2(transform.position.x, transform.position.y),
                itemData, furnitureData);
            
        }
        else
        {
            placeFurniture = House.instance.currentRoom.roomFurnitures.
                PlaceFurniture(new Vector2(transform.position.x, transform.position.y), furnitureData);
        }

        gameObject.SetActive(!placeFurniture);

        if (placeFurniture)
        {
            movement.AddDoorLayerToMask(); // Resetear layermask de movimiento de sledgehammer // TODO: Ver si es necesario aca o siempre va a funcionar con chequear en SetData arriba
            
            AudioManager.instance.PlaySfx(GlobalSfx.Click);
            inventory.SetItem(null);
            inventory.SetItemWithData(null);
            inventory.EnablePackageUI(false);
            if(StateManager.CurrentGameState != GameState.Pause) StateManager.SwitchEditMode();
        }
        else
        {
            AudioManager.instance.PlaySfx(GlobalSfx.Error);
            StartShake();
        }
    }

    private void StartShake()
    {
        if (!isShaking)
        {
            originalPosition = transform.position;
            StartCoroutine(ShakeCoroutine());
        }
    }

    private IEnumerator ShakeCoroutine()
    {
        isShaking = true;
        float elapsed = 0.0f;
        
        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-1f, 1f) * shakeAmount;
            float y = Random.Range(-1f, 1f) * shakeAmount;
            
            transform.position = new Vector3(originalPosition.x + x, originalPosition.y + y, originalPosition.z);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        transform.position = originalPosition;
        isShaking = false;
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
        furnitureData.SetRotationByStep(rotation);
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
        hasReceivedTagBonus = false;
        currentStackLevel = 0;
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
    public bool hasReceivedTagBonus = false;
    public bool firstTimePlaced;
    
    public int currentStackLevel = 0;

    public int instanceID;

    public void SetRotationByStep(int step)
    {
        rotationStep = Mathf.Clamp(step, 0, 3);
        if (step == 0) VectorRotation = new Vector3Int(0, 0, 0);
        else if (step == 1) VectorRotation = new Vector3Int(0, 0, 90);
        else if (step == 2) VectorRotation = new Vector3Int(0, 0, 180);
        else VectorRotation = new Vector3Int(0, 0, 270);

        switch (step)
        {
            case 0:
                VectorRotation = new Vector3Int(0, 0, 0);

                size = originalData.size;
                break;
            case 1:
                VectorRotation = new Vector3Int(0, 0, 90);

                size.x = -originalData.size.y;
                size.y = originalData.size.x;
                break;
            case 2:
                VectorRotation = new Vector3Int(0, 0, 180);

                size = -originalData.size;
                break;
            case 3:
                VectorRotation = new Vector3Int(0, 0, 270);

                size.x = originalData.size.y;
                size.y = -originalData.size.x;
                break;
            default:
                break;
        }
    }
}