using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class FurnitureObjectBase : MonoBehaviour
{
    public FurnitureOriginalData originalData;
    [SerializeField] protected FurnitureData furnitureData;

    public FurnitureData Data => furnitureData;
    
    private void Awake()
    {
        furnitureData = new FurnitureData();
    }

    public virtual void CopyFurnitureData(FurnitureData newData)
    {
        furnitureData.size = newData.size;
        furnitureData.prefab = newData.prefab;
        furnitureData.originalData = newData.originalData;
        furnitureData.VectorRotation = newData.VectorRotation;
        furnitureData.rotationStep = newData.rotationStep;
    }
}