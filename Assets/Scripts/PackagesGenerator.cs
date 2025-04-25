using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class PackagesGenerator : MonoBehaviour
{
    [SerializeField] private FurnitureOriginalData[] allFurnitures;
    [SerializeField] private FurnitureOriginalData wife;
    [SerializeField] private FurnitureOriginalData[] onePercent;
    private bool isWife;
    private int quantityOfObjectSpawned = 0;

    [SerializeField] private GameObject packageGO;
    [SerializeField] private GameObject mainDoor;

    [SerializeField] private List<FurnitureOriginalData> possibleFurnitures;
    [SerializeField] private List<FurnitureOriginalData> deletedFurnitures;

    [SerializeField] private List<FurnitureOriginalData> tutorialObjects;

    [Header("Spawn Probability Configuration")]
    [SerializeField] private SpawnProbabilityConfig spawnConfig;
    [SerializeField] private bool useTagBasedProbabilities = true;
    
    // Dictionary to track furniture spawn chances
    private Dictionary<FurnitureOriginalData, float> furnitureSpawnChances = new Dictionary<FurnitureOriginalData, float>();
    private void Start()
    {
        InvokeRepeating("GeneratePackage", 2f, 5f);
        SetPossibleFurnitures();

        if (useTagBasedProbabilities && spawnConfig != null)
        {
            CalculateFurnitureSpawnChances();
        }
    }

    private void CalculateFurnitureSpawnChances()
    {
        furnitureSpawnChances.Clear();
        
        foreach (var furniture in allFurnitures)
        {
            if (furniture != null)
            {
                float probability = spawnConfig.GetProbabilityForFurniture(furniture);
                furnitureSpawnChances[furniture] = probability;
            }
        }
    }

    private void GeneratePackage()
    {
        if (packageGO.activeInHierarchy || TutorialHandler.instance.onTutorial) return;
        
        TimerManager.StartTimer();
        packageGO.SetActive(true);

        FurnitureOriginalData packageData;
        if (tutorialObjects.Count > 0)
        {
            TutorialHandler.instance.onTutorial = true;
            packageData = packageGO.GetComponent<Package>().furnitureInPackage = tutorialObjects[0];
            tutorialObjects.RemoveAt(0);
            return;
        }

        packageData = packageGO.GetComponent<Package>().furnitureInPackage = GetRandomFurniture();
        gameObject.transform.parent.GetComponent<MainRoom>().CheckIfLose(packageData);
        
    }

    private FurnitureOriginalData GetRandomFurniture()
    {

        if(quantityOfObjectSpawned >= 25 && Random.Range(0, 1f) <= 0.1f && !isWife)
        {
            isWife = true;
            Debug.Log("Sweetieee. I'm Home <3");
            return wife;
        }

        if(Random.Range(0, 100f) <= 1f)
        {
            return onePercent[Random.Range(0, onePercent.Length)];
        }

        if (deletedFurnitures.Count != 0 && Random.Range(0, 1f) <= 0.5f)
        {
            int index_deleted = GetRandomValueIn(deletedFurnitures);
            FurnitureOriginalData value = deletedFurnitures[index_deleted];
            deletedFurnitures.RemoveAt(index_deleted);
            possibleFurnitures.Add(value);
        }

        if (possibleFurnitures.Count == 0)
        {
            deletedFurnitures.Clear();
            SetPossibleFurnitures();
        }

        if (useTagBasedProbabilities && spawnConfig != null)
        {
            return GetRandomFurnitureByProbability();
        }
        else
        {
            // Original method
            int index_possibles = GetRandomValueIn(possibleFurnitures);
            FurnitureOriginalData furniture = possibleFurnitures[index_possibles];
            possibleFurnitures.RemoveAt(index_possibles);
            deletedFurnitures.Add(furniture);
            quantityOfObjectSpawned++;
            return furniture;
        }
    }

    private FurnitureOriginalData GetRandomFurnitureByProbability()
    {
        // Calculate total probability weight
        float totalWeight = 0f;
        Dictionary<FurnitureOriginalData, float> availableFurniture = new Dictionary<FurnitureOriginalData, float>();
        
        foreach (var furniture in possibleFurnitures)
        {
            if (furniture == null) continue;

            float weight = furnitureSpawnChances.ContainsKey(furniture) ? 
                furnitureSpawnChances[furniture] : spawnConfig.defaultTagProbability;
            availableFurniture[furniture] = weight;
            totalWeight += weight;
        }
        
        if (totalWeight <= 0f || availableFurniture.Count == 0)
        {
            if (possibleFurnitures.Count > 0 && possibleFurnitures[0] != null)
            {
                int index = GetRandomValueIn(possibleFurnitures);
                FurnitureOriginalData fallbackFurniture = possibleFurnitures[index];
                possibleFurnitures.RemoveAt(index);
                deletedFurnitures.Add(fallbackFurniture);
                quantityOfObjectSpawned++;
                return fallbackFurniture;
            }
            else
            {
                deletedFurnitures.Clear();
                SetPossibleFurnitures();
                return GetRandomFurniture();
            }
        }
        
        // Select a random value within the total weight
        float randomValue = Random.Range(0f, totalWeight);
        float currentWeight = 0f;
        
        // Find the furniture that corresponds to the random value
        foreach (var kvp in availableFurniture)
        {
            currentWeight += kvp.Value;
            if (randomValue <= currentWeight)
            {
                // Found our furniture
                FurnitureOriginalData selectedFurniture = kvp.Key;
                possibleFurnitures.Remove(selectedFurniture);
                deletedFurnitures.Add(selectedFurniture);
                quantityOfObjectSpawned++;
                return selectedFurniture;
            }
        }
        
        // Fallback (should not reach here)
        int index2 = GetRandomValueIn(possibleFurnitures);
        FurnitureOriginalData fallbackFurniture2 = possibleFurnitures[index2];
        possibleFurnitures.RemoveAt(index2);
        deletedFurnitures.Add(fallbackFurniture2);
        quantityOfObjectSpawned++;
        return fallbackFurniture2;
    }

    private int GetRandomValueIn(List<FurnitureOriginalData> list)
    {
        return Random.Range(0, list.Count);
    }
    private void SetPossibleFurnitures()
    {
        possibleFurnitures.Clear();

        foreach (var f in allFurnitures)
        {
            if (f == null) return;
            possibleFurnitures.Add(f);
        }

        if (possibleFurnitures.Count == 0)
        {
            Debug.LogError("Fua loco");
        }
    }
}