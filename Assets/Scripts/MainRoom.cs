using System.Linq;
using UnityEngine;

public class MainRoom : Room
{

    private int tiles = 54;
    public int availableTiles;

    public static MainRoom instance;


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    protected override void Start()
    {
        base.Start();
        availableTiles = tiles;
        InitMainRoom();
    }
    public void InitMainRoom()
    {
        House.instance.Habitaciones.Add(transform.position, this);
        House.instance.currentRoom = this;
        paletteNum = 0;
        cameraVector = new Vector3(transform.position.x, transform.position.y, -3);
    }

    public void CheckIfLose(FurnitureOriginalData inventoryFurnitureData)
    {
        if (inventoryFurnitureData.size.x * inventoryFurnitureData.size.y < availableTiles) return;
        
        foreach (PlacementData placementData in roomFurnitures.PlacementDatasInPosition.Values)
        {
            if (placementData.furniture.compatibles.Contains(inventoryFurnitureData) && placementData.furnitureOnTop != null)
            {
                Debug.Log("DEBERIA PERDER");
                return;
            }
            
        }
        LoseManager.Instance.Lose();
    }
}