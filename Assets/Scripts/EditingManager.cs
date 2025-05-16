using System;
using UnityEngine;

public class EditingManager : MonoBehaviour
{
    public static EditingManager Instance { get; private set; }

    public FurnitureOriginalData CurrentlyHeldFurnitureOriginalData { get; private set; }
    public event Action<FurnitureOriginalData> OnHeldItemChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetCurrentlyHeldItem(FurnitureOriginalData itemData)
    {
        CurrentlyHeldFurnitureOriginalData = itemData;
        OnHeldItemChanged?.Invoke(CurrentlyHeldFurnitureOriginalData);
    }

    public void ClearCurrentlyHeldItem()
    {
        CurrentlyHeldFurnitureOriginalData = null;
        OnHeldItemChanged?.Invoke(null);
    }
}