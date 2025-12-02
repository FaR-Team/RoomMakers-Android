using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class House : MonoBehaviour
{
    public static House instance;

    [Header("Room Generation")]
    [SerializeField] private RoomGenerator roomGenerator;

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

    [Header("Difficulty Settings")]
    [SerializeField] private bool useDifficultySystem = false;
    [SerializeField] private int roomsPerDifficultyTier = 5;
    [SerializeField] private float difficultyMultiplier = 1.2f;
    [SerializeField] private int maxDifficultyTier = 10;
    [SerializeField] private bool showDifficultyDebug = false;

    [Header("Door Price Settings")]
    [Tooltip("Precio inicial")] public int baseDoorPrice = 100;
    [Tooltip("Controla qué tan rápido suben los precios")] public float priceGrowthFactor = 1.2f;
    [Tooltip("Cantidad a añadir para el crecimiento lineal")] public int priceAdditive = 200;
    [SerializeField][Tooltip("Usar o no el precio exponencial")] private bool useExponentialGrowth = true;
    [SerializeField][Tooltip("Cap opcional en los precios (0 significa ilimitado)")] private int maxDoorPrice = 0;

    private int doorPrice;
    private int roomsBuilt = 0;
    private int score = 0;
    private int currentDifficultyTier = 0;

    public int roomHeight = 9;
    public int roomWidth = 10;
    public int DoorPrice => doorPrice;
    public int Score => score;
    public int CurrentDifficultyTier => currentDifficultyTier;

    public bool classicMode = false;

    [SerializeField] private float roomTransitionTime = .3f;
    private Camera _mainCam;

    [SerializeField] private GameObject uiContainer;

    [Header("Restock Settings")]
    [Tooltip("Base price for restocking shops")] public int baseRestockPrice = 100;
    [SerializeField][Tooltip("Cap optional for restock prices (0 means unlimited)")] private int maxRestockPrice = 0;
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

        if (roomGenerator == null)
        {
            roomGenerator = GetComponent<RoomGenerator>();
            if (roomGenerator == null)
            {
                roomGenerator = gameObject.AddComponent<RoomGenerator>();
            }
        }
    }

    public Room SpawnRoom(Vector3 position)
    {
        if (!Habitaciones.TryGetValue(position, out Room room))
        {
            UpdateScore(doorPrice);
            roomsBuilt++;
            CalculateNextDoorPrice();
            UpdateDifficultyTier();

            GameObject selectedPrefab = roomGenerator.GenerateRoom(position, currentDifficultyTier);

            if (selectedPrefab == null)
            {
                Debug.LogError($"Failed to generate room at position {position}");
                return null;
            }

            Room _room = Instantiate(selectedPrefab, position, Quaternion.identity, houseParent.transform).GetComponent<Room>();
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

        if (score > PlayerPrefs.GetInt("HighScore", 0) && PlayGamesManager.Instance != null)
        {
            PlayGamesManager.Instance.TrySubmitHighScore(score);
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

        if (RoomTagNotification.instance != null && currentRoom != null && !classicMode)
        {
            RoomTagNotification.instance.ShowRoomTag(currentRoom.roomTag);
        }

        yield return null;
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
        if (roomGenerator != null)
        {
            roomGenerator.SetDifficultySystem(enabled);
        }

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

    public void SetShopSpawnProbability(float probability)
    {
        if (roomGenerator != null)
        {
            roomGenerator.SetShopProbability(probability);
        }
    }

    public void SetAllowShopsInCorners(bool allow)
    {
        if (roomGenerator != null)
        {
            roomGenerator.SetAllowShopsInCorners(allow);
        }
    }

    public string GetDifficultyInfo()
    {
        if (!useDifficultySystem) return "Difficulty: Disabled";

        string baseInfo = $"Tier: {currentDifficultyTier}/{maxDifficultyTier}";

        if (roomGenerator != null)
        {
            baseInfo += " | " + roomGenerator.GetGenerationInfo();
        }

        return baseInfo;
    }

    public RoomGenerator GetRoomGenerator()
    {
        return roomGenerator;
    }

}

public enum DifficultyLevel
{
    Easy,
    Medium,
    Hard,
    Extreme
}

