using UnityEngine;

[CreateAssetMenu(fileName = "New Room Type", menuName = "Room Generation/Room Type")]
public class RoomType : ScriptableObject
{
    [Header("Room Configuration")]
    public string roomTypeName;
    public RoomPosition position;
    public GameObject[] roomPrefabs;
    
    [Header("Difficulty Variants")]
    public GameObject[] easyVariants;
    public GameObject[] mediumVariants;
    public GameObject[] hardVariants;
    public GameObject[] extremeVariants;
    
    [Header("Special Properties")]
    public bool canSpawnShops = true;
    public float shopSpawnMultiplier = 1f;
    
    public GameObject[] GetPrefabsForDifficulty(DifficultyLevel difficulty)
    {
        GameObject[] variants = difficulty switch
        {
            DifficultyLevel.Easy => easyVariants?.Length > 0 ? easyVariants : null,
            DifficultyLevel.Medium => mediumVariants?.Length > 0 ? mediumVariants : null,
            DifficultyLevel.Hard => hardVariants?.Length > 0 ? hardVariants : null,
            DifficultyLevel.Extreme => extremeVariants?.Length > 0 ? extremeVariants : null,
            _ => null
        };
        
        return variants?.Length > 0 ? variants : roomPrefabs;
    }
    
    public GameObject GetRandomPrefab(DifficultyLevel difficulty = DifficultyLevel.Easy)
    {
        GameObject[] availablePrefabs = GetPrefabsForDifficulty(difficulty);
        
        if (availablePrefabs == null || availablePrefabs.Length == 0)
            return null;
            
        return availablePrefabs[Random.Range(0, availablePrefabs.Length)];
    }
}

public enum RoomPosition
{
    Center,
    Top,
    Bottom,
    Left,
    Right,
    TopLeftCorner,
    TopRightCorner,
    BottomLeftCorner,
    BottomRightCorner
}