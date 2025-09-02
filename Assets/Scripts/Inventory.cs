using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public FurnitureOriginalData furnitureInventory;
    public FurnitureData furnitureInventoryWithData;
    public int money;
    public TextMeshProUGUI moneyText;
    public GameObject packageUI;
    [SerializeField] private GameObject startButtonUI;
    [SerializeField] private TextMeshProUGUI text_name;
    [SerializeField] private SpriteRenderer itemImage;
    public Sprite PackageSprite;

    [Header("Money Animation")] [SerializeField]
    private float increaseAnimationDuration = 1f;

    [SerializeField] private float decreaseAnimationDuration = 2f;
    [SerializeField] private AnimationCurve increaseAnimationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private AnimationCurve decreaseAnimationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private float shakeIntensity = 5f;
    [SerializeField] private float shakeDuration = 0.5f;
    
    [Header("Insufficient Funds Shake")]
    [SerializeField] private float insufficientFundsShakeDuration = 0.2f;
    [SerializeField] private float insufficientFundsShakeIntensity = 0.04f;

    private Coroutine moneyAnimationCoroutine;
    private Coroutine insufficientFundsShakeCoroutine;
    private int displayedMoney;
    private Vector3 originalMoneyTextPosition;

    public static event Action<FurnitureOriginalData> OnPackagePickUp;
    public static event Action<FurnitureOriginalData> OnInventoryChanged;

    private void Start()
    {
        displayedMoney = money;
        originalMoneyTextPosition = moneyText.transform.localPosition;
        UpdateMoneyDisplay(money);
        TutorialHandler.OnTutorialFinished += HandleTutorialFinished;
    }

    public void UpdateMoney(int intMoney)
    {
        int previousMoney = money;
        money += intMoney;

        if (money < 0)
        {
            money = 0;
        }

        if (intMoney > 0)
        {
            AnimateMoneyIncrease(previousMoney, money);
        }
        else if (intMoney < 0)
        {
            AnimateMoneyDecrease(previousMoney, money);
        }
        else
        {
            UpdateMoneyDisplay(money);
        }
    }

    private void AnimateMoneyIncrease(int fromAmount, int toAmount)
    {
        if (moneyAnimationCoroutine != null)
        {
            StopCoroutine(moneyAnimationCoroutine);
        }

        moneyAnimationCoroutine = StartCoroutine(AnimateMoneyIncreaseCoroutine(fromAmount, toAmount));
    }

    private void AnimateMoneyDecrease(int fromAmount, int toAmount)
    {
        if (moneyAnimationCoroutine != null)
        {
            StopCoroutine(moneyAnimationCoroutine);
        }

        moneyAnimationCoroutine = StartCoroutine(AnimateMoneyDecreaseCoroutine(fromAmount, toAmount));
    }

    private IEnumerator AnimateMoneyIncreaseCoroutine(int fromAmount, int toAmount)
    {
        float elapsedTime = 0f;

        while (elapsedTime < increaseAnimationDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / increaseAnimationDuration;
            float curveValue = increaseAnimationCurve.Evaluate(progress);

            int currentDisplayMoney = Mathf.RoundToInt(Mathf.Lerp(fromAmount, toAmount, curveValue));
            UpdateMoneyDisplay(currentDisplayMoney);

            yield return null;
        }

        UpdateMoneyDisplay(toAmount);
        displayedMoney = toAmount;
        moneyAnimationCoroutine = null;
    }

    private IEnumerator AnimateMoneyDecreaseCoroutine(int fromAmount, int toAmount)
    {
        float elapsedTime = 0f;

        while (elapsedTime < decreaseAnimationDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / decreaseAnimationDuration;
            float curveValue = decreaseAnimationCurve.Evaluate(progress);

            int currentDisplayMoney = Mathf.RoundToInt(Mathf.Lerp(fromAmount, toAmount, curveValue));
            UpdateMoneyDisplay(currentDisplayMoney);

            if (progress < shakeDuration / decreaseAnimationDuration)
            {
                float shakeProgress = (progress * decreaseAnimationDuration) / shakeDuration;
                Vector3 shakeOffset = new Vector3(
                    UnityEngine.Random.Range(-shakeIntensity, shakeIntensity) * (1 - shakeProgress),
                    UnityEngine.Random.Range(-shakeIntensity, shakeIntensity) * (1 - shakeProgress),
                    0
                );
                moneyText.transform.localPosition = originalMoneyTextPosition + shakeOffset;
            }
            else
            {
                moneyText.transform.localPosition = originalMoneyTextPosition;
            }

            yield return null;
        }

        UpdateMoneyDisplay(toAmount);
        moneyText.transform.localPosition = originalMoneyTextPosition;
        displayedMoney = toAmount;
        moneyAnimationCoroutine = null;
    }

    private void UpdateMoneyDisplay(int amount)
    {
        moneyText.text = amount.ToString();
    }

    public void ShakeMoneyForInsufficientFunds()
    {
        if (insufficientFundsShakeCoroutine != null)
        {
            StopCoroutine(insufficientFundsShakeCoroutine);
        }
        
        insufficientFundsShakeCoroutine = StartCoroutine(ShakeMoneyCoroutine());
    }

    private IEnumerator ShakeMoneyCoroutine()
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < insufficientFundsShakeDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / insufficientFundsShakeDuration;
            
            Vector3 shakeOffset = new Vector3(
                UnityEngine.Random.Range(-insufficientFundsShakeIntensity, insufficientFundsShakeIntensity) * (1 - progress),
                UnityEngine.Random.Range(-insufficientFundsShakeIntensity, insufficientFundsShakeIntensity) * (1 - progress),
                0
            );
            
            moneyText.transform.localPosition = originalMoneyTextPosition + shakeOffset;
            
            yield return null;
        }
        
        moneyText.transform.localPosition = originalMoneyTextPosition;
        insufficientFundsShakeCoroutine = null;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Package")) return;

        if (HasItem()) return;

        TimerManager.StopTimer();

        furnitureInventory = Package._furnitureInPackage;
        Package.package.SetActive(false);
        EnablePackageUI(true);
        OnPackagePickUp?.Invoke(furnitureInventory);
        OnInventoryChanged?.Invoke(furnitureInventory);

        UpdatePackageUI();
    }

    public void EnablePackageUI(bool enabled)
    {
        packageUI.SetActive(enabled);
        
        if (enabled)
        {
            UpdatePackageUI();
        }
    }

    private void UpdatePackageUI()
    {
        bool isSpanish = false;
        if (LocalizationManager.Instance != null)
        {
            isSpanish = LocalizationManager.Instance.IsSpanish;
        }

        if (furnitureInventory != null)
        {
            text_name.text = isSpanish ? furnitureInventory.es_Name.ToUpper() : furnitureInventory.Name.ToUpper();

            if (furnitureInventory is ItemData itemData && itemImage != null)
            {
                itemImage.sprite = itemData.ShopSprite;
            }
            else
            {
                itemImage.sprite = PackageSprite;
            }
        }

        if (furnitureInventoryWithData != null)
        {
            text_name.text = isSpanish
                ? furnitureInventoryWithData.originalData.es_Name.ToUpper()
                : furnitureInventoryWithData.originalData.Name.ToUpper();

            if (furnitureInventoryWithData.originalData is ItemData itemData && itemImage != null)
            {
                itemImage.sprite = itemData.ShopSprite;
            }
            else
            {
                itemImage.sprite = PackageSprite;
            }
        }
    }
    
    private void HandleTutorialFinished()
    {
        startButtonUI.SetActive(true);
    }

    public void SetItem(FurnitureOriginalData data)
    {
        if (furnitureInventory == data) return;
        furnitureInventory = data;
        OnInventoryChanged?.Invoke(data);
    }

    public void SetItemWithData(FurnitureData data)
    {
        if(furnitureInventoryWithData == data) return;
        furnitureInventoryWithData = data;
        OnInventoryChanged?.Invoke(furnitureInventoryWithData?.originalData);
    }

    public bool HasItem()
    {
        return furnitureInventoryWithData != null || furnitureInventory != null;
    }

    private void OnDestroy()
    {
        TutorialHandler.OnTutorialFinished -= HandleTutorialFinished;
    }
}
