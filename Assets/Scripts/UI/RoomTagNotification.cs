using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RoomTagNotification : MonoBehaviour
{
    public static RoomTagNotification instance;
    
    [SerializeField] private Canvas notificationCanvas;
    [SerializeField] private TextMeshProUGUI tagText;
    [SerializeField] private CanvasGroup canvasGroup;
    
    [Header("Animation Settings")]
    [SerializeField] private float displayDuration = 1.5f;
    [SerializeField] private float fadeInDuration = 0.3f;
    [SerializeField] private float fadeOutDuration = 0.5f;
    
    private Coroutine currentNotificationCoroutine;
    
    private Dictionary<RoomTag, string> englishTranslations = new Dictionary<RoomTag, string>
    {
        { RoomTag.Bedroom, "Bedroom" },
        { RoomTag.Kitchen, "Kitchen" },
        { RoomTag.Bathroom, "Bathroom" },
        { RoomTag.LivingRoom, "Living Room" },
        { RoomTag.DiningRoom, "Dining Room" },
        { RoomTag.Office, "Office" },
        { RoomTag.Lab, "Lab" },
        { RoomTag.Gym, "Gym" },
        { RoomTag.Shop, "Shop" }
    };
    
    private Dictionary<RoomTag, string> spanishTranslations = new Dictionary<RoomTag, string>
    {
        { RoomTag.Bedroom, "Dormitorio" },
        { RoomTag.Kitchen, "Cocina" },
        { RoomTag.Bathroom, "Baño" },
        { RoomTag.LivingRoom, "Sala de Estar" },
        { RoomTag.DiningRoom, "Comedor" },
        { RoomTag.Office, "Oficina" },
        { RoomTag.Lab, "Laboratorio" },
        { RoomTag.Gym, "Gimnasio" },
        { RoomTag.Shop, "Tienda" }
    };
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        if (notificationCanvas != null)
            notificationCanvas.enabled = false;
    }
    
    public void ShowRoomTag(RoomTag tag)
    {
        if (currentNotificationCoroutine != null)
        {
            StopCoroutine(currentNotificationCoroutine);
            currentNotificationCoroutine = null;
        }
        
        if (tag == RoomTag.None)
        {
            notificationCanvas.enabled = false;
            return;
        }
        
        currentNotificationCoroutine = StartCoroutine(DisplayNotification(tag));
    }
    
    private IEnumerator DisplayNotification(RoomTag tag)
    {
        SetupNotification(tag);
        notificationCanvas.enabled = true;
        
        yield return StartCoroutine(FadeIn());
        yield return new WaitForSeconds(displayDuration);
        yield return StartCoroutine(FadeOut());
        
        notificationCanvas.enabled = false;
        currentNotificationCoroutine = null;
    }
    
    private void SetupNotification(RoomTag tag)
    {
        if (tagText != null)
        {
            string localizedText = GetLocalizedRoomTagText(tag);
            tagText.text = localizedText;
        }
    }
    
    private string GetLocalizedRoomTagText(RoomTag tag)
    {
        if (LocalizationManager.Instance != null && LocalizationManager.Instance.IsSpanish)
        {
            return spanishTranslations.ContainsKey(tag) ? spanishTranslations[tag] : tag.ToString();
        }
        else
        {
            return englishTranslations.ContainsKey(tag) ? englishTranslations[tag] : tag.ToString();
        }
    }
    
    private IEnumerator FadeIn()
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float alpha = Mathf.Clamp01(elapsedTime / fadeInDuration);
            canvasGroup.alpha = alpha;
            yield return null;
        }
        
        canvasGroup.alpha = 1;
    }
    
    private IEnumerator FadeOut()
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeOutDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float alpha = Mathf.Clamp01(1f - (elapsedTime / fadeOutDuration));
            canvasGroup.alpha = alpha;
            yield return null;
        }
        
        canvasGroup.alpha = 0;
    }
}
