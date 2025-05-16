using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RequiredBaseIndicator : MonoBehaviour
{
    public SpriteRenderer indicatorRenderer;
    [SerializeField] private float blinkRate = 0.5f;
    [SerializeField] private Color activeColor = Color.white;
    [SerializeField] private Color inactiveColor = new Color(1, 1, 1, 0.3f);
    
    private static bool isGloballyVisible = true;
    private static List<RequiredBaseIndicator> activeIndicators = new List<RequiredBaseIndicator>();
    private static Coroutine globalBlinkCoroutine;
    private static MonoBehaviour coroutineRunner;
    
    private void Awake()
    {
        if (indicatorRenderer == null)
        {
            indicatorRenderer = GetComponent<SpriteRenderer>();
        }
    }
    
    public void Initialize(Sprite kitSprite)
    {
        if (indicatorRenderer == null)
        {
            indicatorRenderer = GetComponent<SpriteRenderer>();
        }
            
        indicatorRenderer.sprite = kitSprite;
        
        RegisterIndicator();
    }
    
    private void RegisterIndicator()
    {
        if (!activeIndicators.Contains(this))
        {
            activeIndicators.Add(this);
        }
        
        UpdateVisibility();
        
        if (globalBlinkCoroutine == null && activeIndicators.Count > 0)
        {
            coroutineRunner = activeIndicators[0];
            globalBlinkCoroutine = coroutineRunner.StartCoroutine(GlobalBlinkRoutine());
        }
    }
    
    private void UnregisterIndicator()
    {
        activeIndicators.Remove(this);
        
        if (activeIndicators.Count == 0 && globalBlinkCoroutine != null && coroutineRunner != null)
        {
            coroutineRunner.StopCoroutine(globalBlinkCoroutine);
            globalBlinkCoroutine = null;
        }
        else if (coroutineRunner == this && globalBlinkCoroutine != null)
        {
            StopCoroutine(globalBlinkCoroutine);
            globalBlinkCoroutine = null;
            
            if (activeIndicators.Count > 0)
            {
                coroutineRunner = activeIndicators[0];
                globalBlinkCoroutine = coroutineRunner.StartCoroutine(GlobalBlinkRoutine());
            }
        }
    }
    
    private static IEnumerator GlobalBlinkRoutine()
    {
        while (activeIndicators.Count > 0)
        {
            isGloballyVisible = !isGloballyVisible;
            
            foreach (var indicator in activeIndicators)
            {
                if (indicator != null)
                {
                    indicator.UpdateVisibility();
                }
            }
            
            yield return new WaitForSeconds(0.5f);
        }
        
        globalBlinkCoroutine = null;
    }
    
    private void UpdateVisibility()
    {
        if (indicatorRenderer != null)
        {
            indicatorRenderer.color = isGloballyVisible ? activeColor : inactiveColor;
        }
    }
    
    private void OnDestroy()
    {
        UnregisterIndicator();
    }
    
    private void OnDisable()
    {
        UnregisterIndicator();
    }
    
    private void OnEnable()
    {
        RegisterIndicator();
    }
}