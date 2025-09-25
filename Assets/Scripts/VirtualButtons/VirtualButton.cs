using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.OnScreen;
using CandyCoded.HapticFeedback;

public class VirtualButton : OnScreenControl, IPointerDownHandler, IPointerUpHandler
{
    [InputControl(layout = "Button")]
    [SerializeField] private string buttonControlPath;
    
    private bool pressed = false;
    public Sprite sprite;
    private Image image;

    protected override string controlPathInternal
    {
        get => buttonControlPath;
        set => buttonControlPath = value;
    }

    void Start()
    {
        image = this.gameObject.GetComponent<Image>();
    }

    void FixedUpdate()
    {
        if (pressed)
        {
            image.sprite = sprite;
            image.color = new Color(image.color.r, image.color.g, image.color.b, 1f);

        }
        else
        {
            this.gameObject.GetComponent<Image>().sprite = null;
            image.color = new Color(image.color.r, image.color.g, image.color.b, 0f);
        }
    }

    public void OnPointerDown(PointerEventData pointerEventData)
    {
        pressed = true;
        SendValueToControl(1.0f);
        HapticFeedback.LightFeedback();
    }

    public void OnPointerUp(PointerEventData pointerEventData)
    {
        pressed = false;
        SendValueToControl(0.0f);
    }
}
