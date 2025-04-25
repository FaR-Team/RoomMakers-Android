using System.Collections.Generic;
using UnityEngine;

public class ShopRoom : MonoBehaviour
{
    [Header("Shop Configuration")]
    [SerializeField] private Transform[] itemSpawnPoints;
    [SerializeField] private ShopItem[] shopItemPrefabs;
    
    [Header("Sledgehammer Pricing")]
    [SerializeField] private float sledgehammerPriceRatio = 0.5f; // Half the room price
    
    [Header("Restock Settings")]
    [SerializeField] private float restockCooldown = 300f; // 5 minutes in seconds
    [SerializeField] private bool autoRestock = false;
    private float lastRestockTime;
    
    private Room roomComponent;
    private List<ShopItem> spawnedItems = new List<ShopItem>();
    private bool isInitialized = false;
    
    private void Awake()
    {
        roomComponent = GetComponent<Room>();
        
        // Validate spawn points
        if (itemSpawnPoints == null || itemSpawnPoints.Length < 2)
        {
            Debug.LogError("ShopRoom requires at least 2 spawn points for items");
        }
        
        // Validate shop item prefabs
        if (shopItemPrefabs == null || shopItemPrefabs.Length == 0)
        {
            Debug.LogError("ShopRoom requires at least one shop item prefab");
        }
    }
    
    private void Start()
    {
        if (roomComponent != null)
        {
            // Subscribe to room initialization
            //roomComponent.OnRoomTagChanged += HandleRoomTagChanged;
            
            // Set room tag to Shop if not already set
            if (roomComponent.roomTag == RoomTag.None)
            {
                roomComponent.SetRoomTag(RoomTag.Shop);
            }
            
            // Initialize shop items
            InitializeShopItems();
            isInitialized = true;
            lastRestockTime = Time.time;
        }
    }
    
    private void Update()
    {
        if (autoRestock && isInitialized && Time.time - lastRestockTime >= restockCooldown)
        {
            RestockShop();
        }
    }
    
    /*private void HandleRoomTagChanged(RoomTag newTag)
    {
        // If room tag changes to something other than Shop, we might want to clean up
        if (newTag != RoomTag.Shop)
        {
            ClearShopItems();
        }
    }*/
    
    public void InitializeShopItems()
    {
        // Clear any existing items first
        ClearShopItems();
        
        // Make sure we have items to spawn
        if (shopItemPrefabs == null || shopItemPrefabs.Length == 0)
        {
            Debug.LogWarning("No available item prefabs to spawn in shop");
            return;
        }
        
        // Spawn items at each spawn point (up to 2)
        int spawnCount = Mathf.Min(itemSpawnPoints.Length, 2);
        
        // Create a list of indices and shuffle it to select random unique prefabs
        List<int> prefabIndices = new List<int>();
        for (int i = 0; i < shopItemPrefabs.Length; i++)
        {
            prefabIndices.Add(i);
        }
        ShuffleList(prefabIndices);
        
        for (int i = 0; i < spawnCount; i++)
        {
            if (itemSpawnPoints[i] != null)
            {
                // Use modulo to ensure we don't go out of bounds if we have fewer prefabs than spawn points
                int prefabIndex = prefabIndices[i % prefabIndices.Count];
                SpawnShopItem(itemSpawnPoints[i], shopItemPrefabs[prefabIndex]);
            }
        }
    }
    
    private void SpawnShopItem(Transform spawnPoint, ShopItem itemPrefab)
    {
        // Instantiate the shop item
        ShopItem shopItemObj = Instantiate(itemPrefab, spawnPoint.position, Quaternion.identity, transform);
        //ShopItem shopItem = shopItemObj.GetComponent<ShopItem>();
        
        if (shopItemObj != null)
        {
            // Get the ItemData from the prefab
            ItemData itemData = shopItemObj.GetItemData();
            
            if (itemData != null)
            {
                // Calculate price based on item type
                int itemPrice = CalculateItemPrice(itemData);
                
                // Initialize the shop item with the selected item and price
                shopItemObj.Initialize(itemData, itemPrice);
                spawnedItems.Add(shopItemObj);
            }
            else
            {
                Debug.LogWarning("Shop item prefab does not have valid ItemData");
                Destroy(shopItemObj);
            }
        }
    }
    
    private int CalculateItemPrice(ItemData itemData)
    {
        // Special case for Sledgehammer - half the room price
        if (itemData.type == ItemType.Sledgehammer)
        {
            // Get the next room price (assuming we can access it from House instance)
            int nextRoomPrice = House.instance.DoorPrice;
            return Mathf.RoundToInt(nextRoomPrice * sledgehammerPriceRatio);
        }
        
        // For other items, use their predefined price
        return itemData.buyPrice;
    }
    
    private void ClearShopItems()
    {
        foreach (ShopItem item in spawnedItems)
        {
            if (item != null)
            {
                Destroy(item.gameObject);
            }
        }
        
        spawnedItems.Clear();
    }
    
    // Public method to restock the shop
    public void RestockShop()
    {
        InitializeShopItems();
        lastRestockTime = Time.time;
        Debug.Log("Shop has been restocked!");
    }
    
    // Helper method to shuffle a list
    private void ShuffleList<T>(List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
    
    /*private void OnDestroy()
    {
        // Unsubscribe from events
        if (roomComponent != null)
        {
            roomComponent.OnRoomTagChanged -= HandleRoomTagChanged;
        }
    }*/
}