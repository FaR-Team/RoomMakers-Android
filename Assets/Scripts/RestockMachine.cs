using UnityEngine;
using TMPro;
using System.Collections;

public class RestockMachine : MonoBehaviour
{
    [SerializeField] private float vibrationDuration = 0.5f;
    [SerializeField] private float vibrationAmount = 0.05f;
    [SerializeField] private int vibrationCount = 5;
    
    private ShopRoom parentShop;
    private Vector3 originalPosition;
    private bool isVibrating = false;
    
    private void Awake()
    {
        parentShop = GetComponentInParent<ShopRoom>();
        if (parentShop == null)
        {
            Debug.LogError("RestockMachine must be a child of a ShopRoom");
        }
        
    }

    public void TryRestock()
    {
        int currentMoney = PlayerController.instance.Inventory.money;
        int restockPrice = House.instance.RestockPrice;
        
        if (currentMoney >= restockPrice)
        {   
            if (parentShop.RestockShop())
            {
                PlayerController.instance.Inventory.UpdateMoney(-restockPrice);
                House.instance.IncreaseRestockPrice();
                
                AudioManager.instance.PlaySfx(GlobalSfx.Restock);
            }
            else
            {
                AudioManager.instance.PlaySfx(GlobalSfx.Error);
                StartVibration();
            }                     
        }
        else
        {
            //TODO: ver cómo se ve que vibre como los muebles
            
            AudioManager.instance.PlaySfx(GlobalSfx.Error);
            StartVibration();
        }
    }

    private void StartVibration()
    {
        if (!isVibrating)
        {
            StartCoroutine(VibrateCoroutine());
        }
    }

    private IEnumerator VibrateCoroutine()
    {
        isVibrating = true;
        
        Vector3 startPosition = transform.localPosition;
        
        for (int i = 0; i < vibrationCount; i++)
        {
            // Vibrate right
            transform.localPosition = startPosition + new Vector3(vibrationAmount, 0, 0);
            yield return new WaitForSeconds(vibrationDuration / (vibrationCount * 2));
            
            // Vibrate left
            transform.localPosition = startPosition - new Vector3(vibrationAmount, 0, 0);
            yield return new WaitForSeconds(vibrationDuration / (vibrationCount * 2));
        }
        
        // Reset position
        transform.localPosition = startPosition;
        
        isVibrating = false;
    }
}