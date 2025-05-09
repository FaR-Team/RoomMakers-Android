using System.Collections.Generic;
using UnityEngine;

public class ShopRoom : MonoBehaviour
{
    [Header("Shop Configuration")]
    [SerializeField] private Transform[] itemSpawnPoints;
    [SerializeField] private ShopItem[] shopItemPrefabs;
    
    [Header("Sledgehammer Pricing")]
    [SerializeField] private float sledgehammerPriceRatio = 0.5f;
    
    [Header("Restock Settings")]
    [SerializeField] private float restockCooldown = 300f;
    [SerializeField] private bool autoRestock = false;
    private float lastRestockTime;

    [Header("Restock Machine")]
    [SerializeField] private GameObject restockMachinePrefab;
    
    private Room roomComponent;
    private List<ShopItem> spawnedItems = new List<ShopItem>();
    private bool isInitialized = false;
    
    private void Awake()
    {
        roomComponent = GetComponent<Room>();
        
        if (itemSpawnPoints == null || itemSpawnPoints.Length < 2)
        {
            Debug.LogError("ShopRoom requires at least 2 spawn points for items");
        }
        
        if (shopItemPrefabs == null || shopItemPrefabs.Length == 0)
        {
            Debug.LogError("ShopRoom requires at least one shop item prefab");
        }
    }
    
    private void Start()
    {
        if (roomComponent != null)
        {

            
            if (roomComponent.roomTag == RoomTag.None)
            {
                roomComponent.SetRoomTag(RoomTag.Shop);
            }

            if (restockMachinePrefab == null)
            {
                Debug.LogError("Restock Machine prefab is not assigned in the inspector");
            }
            
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
    
    public void InitializeShopItems()
    {
        ClearShopItems();
        
        if (shopItemPrefabs == null || shopItemPrefabs.Length == 0)
        {
            Debug.LogWarning("No available item prefabs to spawn in shop");
            return;
        }
        
        int spawnCount = Mathf.Min(itemSpawnPoints.Length, 2);
        
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
                int prefabIndex = prefabIndices[i % prefabIndices.Count];
                SpawnShopItem(itemSpawnPoints[i], shopItemPrefabs[prefabIndex]);
            }
        }
    }
    
    private void SpawnShopItem(Transform spawnPoint, ShopItem itemPrefab)
    {
        ShopItem shopItemObj = Instantiate(itemPrefab, spawnPoint.position, Quaternion.identity, transform);
        
        if (shopItemObj != null)
        {
            ItemData itemData = shopItemObj.GetItemData();
            
            if (itemData != null)
            {
                int itemPrice = CalculateItemPrice(itemData);
                
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
        if (itemData.type == ItemType.Sledgehammer)
        {
            int nextRoomPrice = House.instance.DoorPrice;
            return Mathf.RoundToInt(nextRoomPrice * sledgehammerPriceRatio);
        }
        
        return itemData.buyPrice;
    }

    public void UpdatePrices()
    {
        foreach (ShopItem item in spawnedItems)
        {
            int newPrice = CalculateItemPrice(item.GetItemData());
            item.Initialize(item.GetItemData(), newPrice);
        }
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
    
    public bool RestockShop()
    {
        if (IsShopEmpty())
        {
            InitializeShopItems();
            lastRestockTime = Time.time;
            Debug.Log("Shop has been restocked!");
            return true;
        }
        else
        {
            Debug.LogWarning("Shop is not empty. Cannot restock.");
            return false;
        }
    }

    public bool IsShopEmpty()
    {
        spawnedItems.RemoveAll(item => item == null);
        
        return spawnedItems.Count == 0;
    }
    
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
}