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
    private void Start()
    {
        InvokeRepeating("GeneratePackage", 2f, 5f);
        SetPossibleFurnitures();
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

        int index_possibles = GetRandomValueIn(possibleFurnitures);

        FurnitureOriginalData furniture = possibleFurnitures[index_possibles];
        possibleFurnitures.RemoveAt(index_possibles);
        deletedFurnitures.Add(furniture);

        quantityOfObjectSpawned++;

        return furniture;
    }
    

    private int GetRandomValueIn(List<FurnitureOriginalData> list)
    {
        return Random.Range(0, list.Count);
    }
    private void SetPossibleFurnitures()
    {
        foreach (var f in allFurnitures)
        {
            possibleFurnitures.Add(f);
        }
    }
}