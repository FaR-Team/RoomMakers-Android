using UnityEngine;

public class Package : MonoBehaviour
{
    public static Package instance;

    public FurnitureOriginalData furnitureInPackage;

    public static GameObject package => instance.gameObject;
    public static FurnitureOriginalData _furnitureInPackage => instance.furnitureInPackage;

    private void Awake()
    {
        instance = this;
    }
}