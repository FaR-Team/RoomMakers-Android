using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemObjectBase : FurnitureObjectBase
{
    public ItemData ItemData => originalData as ItemData;

    public virtual void UseItem()
    {
        Debug.Log("Using Item");
    }
}
