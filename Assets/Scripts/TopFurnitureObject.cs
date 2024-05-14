using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TopFurnitureObject : FurnitureObjectBase
{
    [SerializeField] private GameObject comboStar;
    private bool comboDone;

    public bool ComboDone => comboDone;
    
    private void OnEnable()
    {
        StateManager.OnStateChanged += EnableStar;
    }

    private void OnDisable()
    {
        StateManager.OnStateChanged -= EnableStar;
    }

    public override void CopyFurnitureData(FurnitureData newData)
    {
        base.CopyFurnitureData(newData);
        furnitureData.comboDone = newData.comboDone;
        comboDone = furnitureData.comboDone;
        comboStar.transform.eulerAngles = Vector3.zero;
        //Debug.Log("Copy Top Object Data, combo done: " + comboDone );
    }

    public void MakeCombo()
    {
        comboDone = true;
        furnitureData.comboDone = true;
    }
    
    public void EnableStar(GameState newState)
    {
        comboStar.SetActive(newState == GameState.Editing && comboDone);
    }
}
