using UnityEngine;

public class RoomGenerator : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private RoomGenerationConfig config;
    
    [Header("Difficulty Settings")]
    [SerializeField] private bool useDifficultySystem = false;
    [SerializeField] private DifficultyLevel currentDifficultyLevel = DifficultyLevel.Easy;
    
    [Header("Shop Settings")]
    [SerializeField] private float baseShopProbability = 0.15f;
    [SerializeField] private bool allowShopsInCorners = false;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = false;
    
    public RoomGenerationConfig Config => config;
    public DifficultyLevel CurrentDifficultyLevel => currentDifficultyLevel;
    
    public GameObject GenerateRoom(Vector3 worldPosition, int difficultyTier = 0)
    {
        if (config == null)
        {
            Debug.LogError("RoomGenerator: No configuration assigned!");
            return null;
        }
        
        Vector2Int gridCoords = config.WorldToGridCoords(worldPosition);
        RoomPosition roomPosition = config.GetRoomPosition(gridCoords);
        
        UpdateDifficultyLevel(difficultyTier);
        
        if (ShouldSpawnShop(roomPosition))
        {
            return GenerateShopRoom();
        }
        
        return GenerateNormalRoom(roomPosition);
    }
    
    private bool ShouldSpawnShop(RoomPosition position)
    {
        if (config.shopRooms == null) return false;
        if (position != RoomPosition.Center && !allowShopsInCorners) return false;
        
        RoomType roomType = config.GetRoomType(position);
        if (roomType == null || !roomType.canSpawnShops) return false;
        
        float shopProbability = CalculateShopProbability(roomType);
        return Random.value < shopProbability;
    }
    
    private float CalculateShopProbability(RoomType roomType)
    {
        float probability = baseShopProbability * roomType.shopSpawnMultiplier;
        
        if (useDifficultySystem)
        {
            float difficultyBonus = (int)currentDifficultyLevel * 0.05f;
            probability += difficultyBonus;
        }
        
        return Mathf.Clamp01(probability);
    }
    
    private GameObject GenerateShopRoom()
    {
        GameObject shopPrefab = config.shopRooms.GetRandomPrefab(currentDifficultyLevel);
        
        if (showDebugInfo)
        {
            Debug.Log($"Generated shop room with difficulty: {currentDifficultyLevel}");
        }
        
        return shopPrefab;
    }
    
    private GameObject GenerateNormalRoom(RoomPosition position)
    {
        RoomType roomType = config.GetRoomType(position);
        
        if (roomType == null)
        {
            Debug.LogWarning($"No room type configured for position: {position}");
            return null;
        }
        
        GameObject roomPrefab = roomType.GetRandomPrefab(useDifficultySystem ? currentDifficultyLevel : DifficultyLevel.Easy);
        
        if (showDebugInfo)
        {
            Debug.Log($"Generated {position} room with difficulty: {currentDifficultyLevel}");
        }
        
        return roomPrefab;
    }
    
    private void UpdateDifficultyLevel(int difficultyTier)
    {
        if (!useDifficultySystem) return;
        
        currentDifficultyLevel = difficultyTier switch
        {
            <= 2 => DifficultyLevel.Easy,
            <= 5 => DifficultyLevel.Medium,
            <= 8 => DifficultyLevel.Hard,
            _ => DifficultyLevel.Extreme
        };
    }
    
    public void SetDifficultySystem(bool enabled)
    {
        useDifficultySystem = enabled;
    }
    
    public void SetShopProbability(float probability)
    {
        baseShopProbability = Mathf.Clamp01(probability);
    }
    
    public void SetAllowShopsInCorners(bool allow)
    {
        allowShopsInCorners = allow;
    }
    
    public string GetGenerationInfo()
    {
        if (!useDifficultySystem) return "Difficulty: Disabled";
        
        return $"Difficulty: {currentDifficultyLevel} | Shop Prob: {baseShopProbability:P0}";
    }
}