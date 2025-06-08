using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;
using Random = UnityEngine.Random;

public class House : MonoBehaviour
{
    public static House instance;

    public GameObject[] roomPrefabs;
    public GameObject[] roomTopPrefabs;
    public GameObject[] roomBottomPrefabs;
    public GameObject[] roomLeftPrefabs;
    public GameObject[] roomRightPrefabs;
    public GameObject[] roomTopLeftCornerPrefabs;
    public GameObject[] roomTopRightCornerPrefabs;
    public GameObject[] roomBottomLeftCornerPrefabs;
    public GameObject[] roomBottomRightCornerPrefabs;
    
    public GameObject selectedPrefab;
    [SerializeField] private GameObject houseParent;
    public Dictionary<Vector3, Room> Habitaciones = new Dictionary<Vector3, Room>();

    public Room currentRoom;

    public GameObject comboStarSprite;
    [Header("Box Sprites")] 
    public Sprite[] one_one_sprites;
    public Sprite[] two_two_sprites;
    public Sprite[] two_one_sprites;
    public Sprite[] three_one_sprites;

    private int availableSpaces;

    [Header("Shop Settings")]
    public GameObject[] shopRoomPrefabs;
    [Range(0f, 1f)]
    [SerializeField] private float shopSpawnProbability = 0.15f;
    [SerializeField] private bool allowShopsInCorners = false;
    
    [Header("Difficulty Settings")]
    [SerializeField] private bool useDifficultySystem = false;
    [SerializeField] private int roomsPerDifficultyTier = 5;
    [SerializeField] private float difficultyMultiplier = 1.2f;
    [SerializeField] private int maxDifficultyTier = 10;
    [SerializeField] private bool showDifficultyDebug = false;
    
    [Header("Difficulty Room Variants")]
    [SerializeField] private GameObject[] easyRoomPrefabs;
    [SerializeField] private GameObject[] mediumRoomPrefabs;
    [SerializeField] private GameObject[] hardRoomPrefabs;
    [SerializeField] private GameObject[] extremeRoomPrefabs;
    
    [Header("Door Price Settings")]
    [Tooltip("Precio inicial")] public int baseDoorPrice = 100;
    [Tooltip("Controla qué tan rápido suben los precios")] public float priceGrowthFactor = 1.2f;
    [Tooltip("Cantidad a añadir para el crecimiento lineal")] public int priceAdditive = 200;
    [SerializeField] [Tooltip("Usar o no el precio exponencial")] private bool useExponentialGrowth = true;
    [SerializeField] [Tooltip("Cap opcional en los precios (0 significa ilimitado)")] private int maxDoorPrice = 0;
    
    private int doorPrice;
    private int roomsBuilt = 0;
    private int score = 0;
    private int currentDifficultyTier = 0;

    public int roomHeight = 9;
    public int roomWidth = 10;
    public int DoorPrice => doorPrice;
    public int Score => score;
    public int CurrentDifficultyTier => currentDifficultyTier;

    [SerializeField] private float roomTransitionTime = .3f;
    private Camera _mainCam;

    [SerializeField] private GameObject uiContainer;

    [Header("Restock Settings")]
    [Tooltip("Base price for restocking shops")] public int baseRestockPrice = 100;
    [SerializeField] [Tooltip("Cap optional for restock prices (0 means unlimited)")] private int maxRestockPrice = 0;
    private int restockPrice;
    private int timesRestocked = 0;
    public int RestockPrice => restockPrice;

    [Header("Furniture Indicators")]
    public GameObject requiredBaseIndicatorPrefab;

    void Awake()
    {
        if (instance == null || instance != this)
        {
            instance = this;
        }

        _mainCam = Camera.main;
        doorPrice = baseDoorPrice;
        score = 0;
        roomsBuilt = 0;
        restockPrice = baseRestockPrice;
        timesRestocked = 0;
        currentDifficultyTier = 0;
    }
    
    public Room SpawnRoom(Vector3 position)
    {
        if (!Habitaciones.TryGetValue(position, out Room room))
        {
            UpdateScore(doorPrice);
            roomsBuilt++;
            CalculateNextDoorPrice();
            UpdateDifficultyTier();
        
            int x = (int)position.x / roomWidth;
            int y = (int)position.y / roomHeight;
            WhichRoomToSpawnPos(x, y);
            Room _room = Instantiate(selectedPrefab, position, Quaternion.identity , houseParent.transform).GetComponent<Room>();
            _room.Init();
            Habitaciones.Add(position, _room);
            _room.cameraVector = new Vector3(position.x, position.y, -3);

            if (showDifficultyDebug)
            {
                Debug.Log($"Spawned room at tier {currentDifficultyTier}, total rooms: {roomsBuilt}");
            }

            return _room;
        }
        else return GetRoom(position);
    }
    
    private void UpdateDifficultyTier()
    {
        if (!useDifficultySystem) return;
        
        int newTier = Mathf.Min(roomsBuilt / roomsPerDifficultyTier, maxDifficultyTier);
        
        if (newTier > currentDifficultyTier)
        {
            currentDifficultyTier = newTier;
            if (showDifficultyDebug)
            {
                Debug.Log($"Difficulty increased to tier {currentDifficultyTier}!");
            }
        }
    }
    
    private DifficultyLevel GetCurrentDifficultyLevel()
    {
        if (!useDifficultySystem) return DifficultyLevel.Easy;
        
        float tierProgress = (float)currentDifficultyTier / maxDifficultyTier;
        
        if (tierProgress <= 0.25f) return DifficultyLevel.Easy;
        if (tierProgress <= 0.5f) return DifficultyLevel.Medium;
        if (tierProgress <= 0.75f) return DifficultyLevel.Hard;
        return DifficultyLevel.Extreme;
    }
    
    private GameObject[] GetRoomPrefabsByDifficulty(DifficultyLevel difficulty)
    {
        return difficulty switch
        {
            DifficultyLevel.Easy => easyRoomPrefabs?.Length > 0 ? easyRoomPrefabs : roomPrefabs,
            DifficultyLevel.Medium => mediumRoomPrefabs?.Length > 0 ? mediumRoomPrefabs : roomPrefabs,
            DifficultyLevel.Hard => hardRoomPrefabs?.Length > 0 ? hardRoomPrefabs : roomPrefabs,
            DifficultyLevel.Extreme => extremeRoomPrefabs?.Length > 0 ? extremeRoomPrefabs : roomPrefabs,
            _ => roomPrefabs
        };
    }
    
    private float GetDifficultyShopProbability()
    {
        if (!useDifficultySystem) return shopSpawnProbability;
        
        float difficultyBonus = currentDifficultyTier * 0.05f;
        return shopSpawnProbability - difficultyBonus;
    }

    private void CalculateNextDoorPrice()
    {
        float difficultyPriceMultiplier = 1f;
        
        if (useDifficultySystem)
        {
            difficultyPriceMultiplier = Mathf.Pow(difficultyMultiplier, currentDifficultyTier);
        }
        
        if (useExponentialGrowth)
        {
            doorPrice = Mathf.RoundToInt(baseDoorPrice * Mathf.Pow(priceGrowthFactor, roomsBuilt) * difficultyPriceMultiplier);
        }
        else
        {
            doorPrice = Mathf.RoundToInt((baseDoorPrice + (priceAdditive * roomsBuilt)) * difficultyPriceMultiplier);
        }

        doorPrice = Mathf.RoundToInt(doorPrice / 5f) * 5;

        if (maxDoorPrice > 0 && doorPrice > maxDoorPrice)
        {
            doorPrice = maxDoorPrice;
            doorPrice = Mathf.RoundToInt(doorPrice / 5f) * 5;
        }
    }

    public void UpdateScore(int scoreToAdd)
    {
        score += scoreToAdd;
    }

    public void WhichRoomToSpawnPos(int x, int y)
    {
        if (x == -7 && y == 0)
        {
            RandomizeRoom("TopLeftCorner");
        }
        else if (x == 8 && y == 0)
        {
            RandomizeRoom("TopRightCorner");
        }
        else if (x == -7 && y == -15)
        {
            RandomizeRoom("BottomLeftCorner");
        }
        else if (x == 8 && y == -15)
        {
            RandomizeRoom("BottomRightCorner");
        }
        else if (x == -7)
        {
            RandomizeRoom("left");
        }
        else if (x == 8)
        {
            RandomizeRoom("right");
        }
        else if (y == 0)
        {
            RandomizeRoom("Top");
        }
        else if (y == -15)
        {
            RandomizeRoom("Bottom");
        }
        else
        {
            RandomizeRoom("center");
        }
    }

    public Room GetRoom(Vector3 position)
    {
        if (Habitaciones.TryGetValue(position, out Room room))
        {
            room.cameraVector = new Vector3(position.x, position.y, -3);
            return room;
        }
        else return null;
    }

    public void TransitionToRoom(Vector3 position, int color)
    {
        if (_mainCam.transform.position == position) return;

        Vector3 roomPos = new Vector3(position.x, position.y, -12f);
        currentRoom = Habitaciones[roomPos];

        if (currentRoom.TryGetComponent<ShopRoom>(out ShopRoom shopRoom))
        {
            shopRoom.UpdatePrices();
        }
        
        StopAllCoroutines();
        StartCoroutine(MoveCamNextRoom(position, color));
    }

    IEnumerator MoveCamNextRoom(Vector3 position, int color)
    {
        uiContainer.SetActive(false);
        
        Vector3 initialCameraPos = _mainCam.transform.position;
        position.z = initialCameraPos.z;

        float elapsedTime = 0;
        float startTime = Time.unscaledTime;

        while (elapsedTime < roomTransitionTime)
        {
            elapsedTime = Time.unscaledTime - startTime;
            float normalizedTime = Mathf.Clamp01(elapsedTime / roomTransitionTime);
            
            float t = Mathf.SmoothStep(0f, 1f, normalizedTime);
            _mainCam.transform.position = Vector3.Lerp(initialCameraPos, position, t);
            
            yield return null;
        }
        
        _mainCam.transform.position = position;
        ColourChanger.instance.ChangeColour(color);
        
        uiContainer.SetActive(true);
        
        yield return null;
    }

    void RandomizeRoom(string type)
    {
        bool shouldSpawnShop = false;
        DifficultyLevel currentDifficulty = GetCurrentDifficultyLevel();

        if (type == "center" && shopRoomPrefabs != null && shopRoomPrefabs.Length > 0)
        {
            shouldSpawnShop = Random.value < GetDifficultyShopProbability();
        }
        
        if (shouldSpawnShop)
        {
            int shopIndex = Random.Range(0, shopRoomPrefabs.Length);
            selectedPrefab = shopRoomPrefabs[shopIndex];
            return;
        }
        
        GameObject[] roomsToUse = GetRoomPrefabsByDifficulty(currentDifficulty);
        
        if (type == "center")
        {
            int index = Random.Range(0, roomsToUse.Length);
            selectedPrefab = roomsToUse[index];
        }
        else if (type == "Top")
        {
            int index = Random.Range(0, roomTopPrefabs.Length);
            selectedPrefab = roomTopPrefabs[index];
        }
        else if (type == "Bottom")
        {
            int index = Random.Range(0, roomBottomPrefabs.Length);
            selectedPrefab = roomBottomPrefabs[index];
        }
        else if (type == "left")
        {
            int index = Random.Range(0, roomLeftPrefabs.Length);
            selectedPrefab = roomLeftPrefabs[index];
        }
        else if (type == "right")
        {
            int index = Random.Range(0, roomRightPrefabs.Length);
            selectedPrefab = roomRightPrefabs[index];
        }
        else if (type == "TopLeftCorner")
        {
            int index = Random.Range(0, roomTopLeftCornerPrefabs.Length);
            selectedPrefab = roomTopLeftCornerPrefabs[index];
        }
        else if (type == "TopRightCorner")
        {
            int index = Random.Range(0, roomTopRightCornerPrefabs.Length);
            selectedPrefab = roomTopRightCornerPrefabs[index];
        }
        else if (type == "BottomLeftCorner")
        {
            int index = Random.Range(0, roomBottomLeftCornerPrefabs.Length);
            selectedPrefab = roomBottomLeftCornerPrefabs[index];
        }
        else if (type == "BottomRightCorner")
        {
            int index = Random.Range(0, roomBottomRightCornerPrefabs.Length);
            selectedPrefab = roomBottomRightCornerPrefabs[index];
        }
        
        if (showDifficultyDebug && type == "center")
        {
            Debug.Log($"Selected {currentDifficulty} difficulty room from {roomsToUse.Length} options");
        }
    }

    public void IncreaseRestockPrice()
    {
        timesRestocked++;
        restockPrice += Mathf.RoundToInt(baseRestockPrice * Mathf.Pow(priceGrowthFactor, timesRestocked));
        
        restockPrice = Mathf.RoundToInt(restockPrice / 5f) * 5;
        
        if (maxRestockPrice > 0 && restockPrice > maxRestockPrice)
        {
            restockPrice = maxRestockPrice;
                        restockPrice = Mathf.RoundToInt(restockPrice / 5f) * 5;
        }
    }

    public Sprite[] GetSpritesBySize(TypeOfSize size)
    {
        return size switch
        {
            TypeOfSize.one_one => one_one_sprites,
            TypeOfSize.two_two => two_two_sprites,
            TypeOfSize.two_one => two_one_sprites,
            TypeOfSize.three_one => three_one_sprites,
            _ => null
        };
    }
    
    public void SetDifficultySystem(bool enabled)
    {
        useDifficultySystem = enabled;
        if (showDifficultyDebug)
        {
            Debug.Log($"Difficulty system {(enabled ? "enabled" : "disabled")}");
        }
    }
    
    public void SetDifficultyTier(int tier)
    {
        currentDifficultyTier = Mathf.Clamp(tier, 0, maxDifficultyTier);
        if (showDifficultyDebug)
        {
            Debug.Log($"Difficulty tier manually set to {currentDifficultyTier}");
        }
    }
    
    public void ResetDifficulty()
    {
        currentDifficultyTier = 0;
        if (showDifficultyDebug)
        {
            Debug.Log("Difficulty reset to tier 0");
        }
    }
    
    public string GetDifficultyInfo()
    {
        if (!useDifficultySystem) return "Difficulty: Disabled";
        
        DifficultyLevel level = GetCurrentDifficultyLevel();
        float shopProb = GetDifficultyShopProbability();
        
        return $"Tier: {currentDifficultyTier}/{maxDifficultyTier} | Level: {level} | Shop Prob: {shopProb:P0}";
    }
}

public enum DifficultyLevel
{
    Easy,
    Medium,
    Hard,
    Extreme
}
