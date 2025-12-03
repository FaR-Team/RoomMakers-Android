using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class PackagesGenerator : MonoBehaviour
{
    [SerializeField] private FurnitureOriginalData[] allFurnitures;
    [SerializeField] private FurnitureOriginalData[] onePercent;
    private int quantityOfObjectSpawned = 0;

    [SerializeField] private GameObject packageGO;
    [SerializeField] private GameObject mainDoor;

    [SerializeField] private List<FurnitureOriginalData> possibleFurnitures;
    [SerializeField] private List<FurnitureOriginalData> deletedFurnitures;

    [SerializeField] private List<FurnitureOriginalData> tutorialObjects;

    [Header("Spawn Probability Configuration")]
    [SerializeField] private SpawnProbabilityConfig spawnConfig;
    [SerializeField] private bool useTagBasedProbabilities = true;

    private Dictionary<FurnitureOriginalData, float> furnitureSpawnChances = new Dictionary<FurnitureOriginalData, float>();

    private void OnEnable()
    {
        TutorialHandler.OnTutorialLockStateUpdated += HandleTutorialStateUpdate;
    }

    private void OnDisable()
    {
        TutorialHandler.OnTutorialLockStateUpdated -= HandleTutorialStateUpdate;
    }

    private void Start()
    {
        InvokeRepeating("GeneratePackage", 2f, 5f);
        SetPossibleFurnitures();

        if (useTagBasedProbabilities && spawnConfig != null)
        {
            CalculateFurnitureSpawnChances();
        }
        HandleTutorialStateUpdate();
    }

    private void HandleTutorialStateUpdate()
    {
        if (!TutorialHandler.AreDoorsTutorialLocked())
        {
            if (tutorialObjects.Count > 0)
            {
                Debug.Log("[PackagesGenerator] Tutorial ended or skipped. Clearing remaining tutorial objects.");
                tutorialObjects.Clear();
            }
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
        if (packageGO.activeInHierarchy)
        {
            // Debug.Log("[PackagesGenerator] Package already active, skipping generation.");
            return;
        }

        if (TutorialHandler.instance != null)
        {
            if (!TutorialHandler.instance.CanSpawnPackage() || tutorialObjects.Count == 0) return;
            Debug.Log($"[PackagesGenerator] Tutorial step not started and item available. Dispensing: {tutorialObjects[0].Name}");

            Package package = packageGO.GetComponent<Package>();
            package.furnitureInPackage = tutorialObjects[0];
            tutorialObjects.RemoveAt(0);

            packageGO.SetActive(true);

            return;
        }


        if (TutorialHandler.AreDoorsTutorialLocked()) //TODO: Creo que este if ya está al pedo, probar
        {

            Debug.Log("[PackagesGenerator] Tutorial is active and controlling flow (doors locked), but no specific tutorial items in queue. Waiting.");
            return;
        }
        else
        {

            if (tutorialObjects.Count > 0)
            {
                Debug.LogWarning("[PackagesGenerator] Tutorial not locking doors, but stale tutorial objects found. Clearing them now.");
                tutorialObjects.Clear();
            }

            Debug.Log("[PackagesGenerator] Generating regular package.");
            TimerManager.StartTimer();
            packageGO.SetActive(true);

            FurnitureOriginalData regularPackageData = GetRandomFurniture();
            if (regularPackageData == null)
            {
                Debug.LogWarning("[PackagesGenerator] GetRandomFurniture returned null. Skipping package generation for this cycle.");
                packageGO.SetActive(false);
                TimerManager.StopTimer();
                return;
            }

            packageGO.GetComponent<Package>().furnitureInPackage = regularPackageData;

            MainRoom mainRoom = gameObject.transform.parent.GetComponent<MainRoom>();
            if (mainRoom != null)
            {
                mainRoom.CheckIfLose(regularPackageData);
            }
            else
            {
                Debug.LogError("[PackagesGenerator] Parent MainRoom not found! Cannot perform CheckIfLose.");
                packageGO.SetActive(false);
                TimerManager.StopTimer();
            }
        }
    }
    private FurnitureOriginalData GetRandomFurniture()
    {
        if (Random.Range(0, 100f) <= 1f)
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
            if (possibleFurnitures.Count == 0)
            {
                Debug.LogError("[PackagesGenerator] SetPossibleFurnitures did not populate any furnitures. GetRandomFurniture cannot proceed.");
                return null;
            }
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
                if (possibleFurnitures.Count > 0 && possibleFurnitures[0] != null)
                {
                    // Try one more time after reset
                    return GetRandomFurniture();
                }
                return null;
            }
        }

        float randomValue = Random.Range(0f, totalWeight);
        float currentWeight = 0f;

        foreach (var kvp in availableFurniture)
        {
            currentWeight += kvp.Value;
            if (randomValue <= currentWeight)
            {
                FurnitureOriginalData selectedFurniture = kvp.Key;
                possibleFurnitures.Remove(selectedFurniture);
                deletedFurnitures.Add(selectedFurniture);
                quantityOfObjectSpawned++;
                return selectedFurniture;
            }
        }

        if (possibleFurnitures.Count > 0)
        {
            int index2 = GetRandomValueIn(possibleFurnitures);
            FurnitureOriginalData fallbackFurniture2 = possibleFurnitures[index2];
            possibleFurnitures.RemoveAt(index2);
            deletedFurnitures.Add(fallbackFurniture2);
            quantityOfObjectSpawned++;
            return fallbackFurniture2;
        }
        Debug.LogError("[PackagesGenerator] GetRandomFurnitureByProbability fallback failed: no possible furnitures.");
        return null;
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
            if (f == null) continue;
            possibleFurnitures.Add(f);
        }

        if (possibleFurnitures.Count == 0)
        {
            Debug.LogError("[PackagesGenerator] SetPossibleFurnitures: No furnitures were added to possibleFurnitures. Check 'allFurnitures' configuration.");
        }
    }
}