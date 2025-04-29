using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

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

    private int availableSpaces;

    [Header("Shop Settings")]
    public GameObject[] shopRoomPrefabs;
    [Range(0f, 1f)]
    [SerializeField] private float shopSpawnProbability = 0.15f;
    [SerializeField] private bool allowShopsInCorners = false;
    
    [Header("Door Price Settings")]
    [Tooltip("Precio inicial")] public int baseDoorPrice = 100;
    [Tooltip("Controla qué tan rápido suben los precios")] public float priceGrowthFactor = 1.2f;
    [Tooltip("Cantidad a añadir para el crecimiento lineal")] public int priceAdditive = 200;
    [SerializeField] [Tooltip("Usar o no el precio exponencial")] private bool useExponentialGrowth = true;
    [SerializeField] [Tooltip("Cap opcional en los precios (0 significa ilimitado)")] private int maxDoorPrice = 0;
    
    private int doorPrice;
    private int roomsBuilt = 0;
    private int score = 0;

    public int roomHeight = 9;
    public int roomWidth = 10;
    public int DoorPrice => doorPrice;
    public int Score => score;

    [SerializeField] private float roomTransitionTime = .3f;
    private Camera _mainCam;

    [SerializeField] private GameObject uiContainer;

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
    }
    public Room SpawnRoom(Vector3 position)
    {
        if (!Habitaciones.TryGetValue(position, out Room room))
        {
            UpdateScore(doorPrice);
            roomsBuilt++;
            CalculateNextDoorPrice();
        
            int x = (int)position.x / roomWidth;
            int y = (int)position.y / roomHeight;
            WhichRoomToSpawnPos(x, y);
            Room _room = Instantiate(selectedPrefab, position, Quaternion.identity , houseParent.transform).GetComponent<Room>();
            _room.Init();
            Habitaciones.Add(position, _room);
            _room.cameraVector = new Vector3(position.x, position.y, -3);

            return _room;
        }
        else return GetRoom(position);
    }
    private void CalculateNextDoorPrice()
    {
        if (useExponentialGrowth)
        {
            // Fórmula de crecimiento exponencial: baseDoorPrice * (growthFactor ^ roomsBuilt)
            doorPrice = Mathf.RoundToInt(baseDoorPrice * Mathf.Pow(priceGrowthFactor, roomsBuilt));
        }
        else
        {
            // Crecimiento lineal
            doorPrice = baseDoorPrice + (priceAdditive * roomsBuilt);
        }

        // Round to nearest 5
        doorPrice = Mathf.RoundToInt(doorPrice / 5f) * 5;

        // Aplicar cap al precio si hay
        if (maxDoorPrice > 0 && doorPrice > maxDoorPrice)
        {
            doorPrice = maxDoorPrice;
            // Ensure the cap also ends in 0 or 5
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
        // Si ya estamos en la habitacion, no mover, pensar mejor forma de hacer que no se interactue con las
        // puertas de la habitacion en la que ya estamos
        if (_mainCam.transform.position == position) return;

        // CAMBIAR DESPUES, NO HARDCODEAR -12 SINO CAPAZ PASAR LA HABITACION POR PARAMETRO Y AHI SACAR LAS POSICIONES
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
        // Esconder UI temporalmente
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
        
        // Mostrar UI tras la transición
        uiContainer.SetActive(true);
        
        yield return null;
    }

    void RandomizeRoom(string type)
    {
        bool shouldSpawnShop = false;


        if (type == "center" && shopRoomPrefabs != null && shopRoomPrefabs.Length > 0)
        {
            shouldSpawnShop = Random.value < shopSpawnProbability;
        }
        
        if (shouldSpawnShop)
        {
            int shopIndex = Random.Range(0, shopRoomPrefabs.Length);
            selectedPrefab = shopRoomPrefabs[shopIndex];
            return;
        }
        if (type == "center")
        {
            int index = Random.Range(0, roomPrefabs.Length);
            selectedPrefab = roomPrefabs[index];
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
        else if (type == "Right")
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
    }
}