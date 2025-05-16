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
    
    // Dictionary to track furniture spawn chances
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
        // Initial check in case tutorial is already over when this starts
        HandleTutorialStateUpdate(); 
    }

    private void HandleTutorialStateUpdate()
    {
        // This is called when TutorialHandler's lock state might change (e.g. step completed, tutorial skipped/ended)
        // AreDoorsTutorialLocked() will be false if tutorialStep >= 6 or TutorialHandler.instance is null
        if (!TutorialHandler.AreDoorsTutorialLocked()) // Checks if tutorial is over or skipped
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

        // Condition 1: Tutorial is active AND has specific items to dispense from its queue.
        // TutorialHandler.instance != null implies the tutorial system is running.
        // tutorialObjects.Count > 0 implies there's a specific item for the tutorial.
        if (TutorialHandler.instance != null && tutorialObjects.Count > 0)
        {
            Debug.Log($"[PackagesGenerator] Tutorial item available (tutorialObjects.Count: {tutorialObjects.Count}). Dispensing: {tutorialObjects[0].Name}");

            Package package = packageGO.GetComponent<Package>();
            package.furnitureInPackage = tutorialObjects[0];
            tutorialObjects.RemoveAt(0);

            // Activate the package GameObject so the player can interact with it to pick up the tutorial item.
            // Tutorial items might not use the timer. The tutorial flow will guide the player.
            packageGO.SetActive(true);
            // TimerManager.StartTimer(); // Decided against this for tutorial items; tutorial guides pickup.

            // TutorialHandler will proceed once the item is picked up (via Inventory.OnFurniturePickUp).
            return;
        }

        // Condition 2: No tutorial-specific items to dispense from the queue.
        // Now, decide whether to generate a REGULAR package or do nothing (if tutorial is active and blocking regular packages).
        // TutorialHandler.AreDoorsTutorialLocked() is true if tutorialStep < 6 and instance exists.
        // If TutorialHandler.instance is null (e.g., after skip), AreDoorsTutorialLocked() returns false.
        if (TutorialHandler.AreDoorsTutorialLocked())
        {
            // Tutorial is active and in a state that locks doors/controls packages,
            // AND there are no specific items in the tutorialObjects queue for it to dispense right now.
            // So, do nothing. Wait for tutorial progression or for tutorialObjects to be populated by other means if applicable.
            Debug.Log("[PackagesGenerator] Tutorial is active and controlling flow (doors locked), but no specific tutorial items in queue. Waiting.");
            return;
        }
        else // Tutorial is completed, skipped, or TutorialHandler instance is gone. Regular package generation is allowed.
        {
            // Safeguard: Clear any stale tutorial objects if the tutorial is no longer locking doors.
            // HandleTutorialStateUpdate (called by OnTutorialLockStateUpdated) should generally cover this,
            // but this is an extra check in case of timing issues or if the event wasn't caught.
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
                packageGO.SetActive(false); // Ensure package isn't left active if no data
                TimerManager.StopTimer();   // Stop timer if we started it
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
                packageGO.SetActive(false); // Deactivate package if critical logic fails
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
                return null; // No furniture available
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
                // Avoid deep recursion if SetPossibleFurnitures consistently fails
                if (possibleFurnitures.Count > 0 && possibleFurnitures[0] != null) {
                    // Try one more time after reset
                    return GetRandomFurniture(); // This could be risky if the underlying issue isn't fixed
                }
                return null; // Failed to get furniture
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
        if (possibleFurnitures.Count > 0) {
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
            if (f == null) continue; // Skip null entries in allFurnitures
            possibleFurnitures.Add(f);
        }

        if (possibleFurnitures.Count == 0)
        {
            Debug.LogError("[PackagesGenerator] SetPossibleFurnitures: No furnitures were added to possibleFurnitures. Check 'allFurnitures' configuration.");
        }
    }
}