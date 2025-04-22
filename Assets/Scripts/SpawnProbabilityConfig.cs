using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SpawnProbabilityConfig", menuName = "Room Makers/Spawn Probability Config")]
public class SpawnProbabilityConfig : ScriptableObject
{
    [Serializable]
    public class TagProbability
    {
        public RoomTag tag;
        [Range(0f, 100f)]
        public float spawnProbability = 10f;
    }

    [Serializable]
    public class FurnitureSpecificProbability
    {
        public FurnitureOriginalData furniture;
        [Range(0f, 100f)]
        public float spawnProbability = 5f;
    }

    [Header("Tag-Based Probabilities")]
    [Tooltip("Default probability for tags not specified")]
    [Range(0f, 100f)]
    public float defaultTagProbability = 10f;
    public List<TagProbability> tagProbabilities = new List<TagProbability>();

    [Header("Specific Furniture Overrides")]
    [Tooltip("These values override tag-based probabilities for specific furniture")]
    public List<FurnitureSpecificProbability> furnitureSpecificProbabilities = new List<FurnitureSpecificProbability>();

    public float GetProbabilityForTag(RoomTag tag)
    {
        foreach (var tagProb in tagProbabilities)
        {
            if (tagProb.tag == tag)
                return tagProb.spawnProbability;
        }
        return defaultTagProbability;
    }

    public float GetProbabilityForFurniture(FurnitureOriginalData furniture)
    {
        // Check if this furniture has a specific override
        foreach (var furnitureProb in furnitureSpecificProbabilities)
        {
            if (furnitureProb.furniture == furniture)
                return furnitureProb.spawnProbability;
        }
        
        // Otherwise use the tag-based probability
        return GetProbabilityForTag(furniture.furnitureTag);
    }
}