using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.UI;
using CandyCoded.HapticFeedback;


public enum VirtualDPadDirection { Both, Horizontal, Vertical }

public class VirtualDPad : OnScreenControl, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [SerializeField] private RectTransform centerArea = null;
    [SerializeField] private RectTransform handle = null;
    [SerializeField] private VirtualDPadDirection direction = VirtualDPadDirection.Both;
    [InputControl(layout = "Vector2")]
    [SerializeField] private string dPadControlPath;
    [SerializeField] private float movementRange = 10f;
    [SerializeField] private float moveThreshold = 0f;
    [SerializeField] private float uiMovementRange = 10f;
    [SerializeField] private bool forceIntValue = true;
    [SerializeField] private Image[] directionImages;

    private Vector2 lastDirection;

    private Vector3 startPos;

    protected override string controlPathInternal
    {
        get => dPadControlPath;
        set => dPadControlPath = value;
    }

    private void Awake()
    {
        if (centerArea == null)
            centerArea = GetComponent<RectTransform>();

        Vector2 center = new Vector2(0.5f, 0.5f);
        centerArea.pivot = center;
        handle.anchorMin = center;
        handle.anchorMax = center;
        handle.pivot = center;
        handle.anchoredPosition = Vector2.zero;
        lastDirection = Vector2.zero;
    }

    private void Start()
    {
        startPos = handle.anchoredPosition;

        foreach (var image in directionImages)
        {
            image.gameObject.SetActive(false);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData == null)
            throw new System.ArgumentNullException(nameof(eventData));

        OnDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData == null)
            throw new System.ArgumentNullException(nameof(eventData));

        RectTransformUtility.ScreenPointToLocalPointInRectangle(handle.parent.GetComponentInParent<RectTransform>(), eventData.position, eventData.pressEventCamera, out var position);
        Vector2 delta = position;

        if (direction == VirtualDPadDirection.Horizontal) delta.y = 0;
        else if (direction == VirtualDPadDirection.Vertical) delta.x = 0;

        Vector2 buttonDelta = Vector2.ClampMagnitude(delta, uiMovementRange);
        handle.anchoredPosition = startPos + (Vector3)buttonDelta;

        Vector2 newPos = SanitizePosition(delta);
        SendValueToControl(newPos);

        ToggleDirectionImages(newPos);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        handle.anchoredPosition = startPos;
        lastDirection = Vector2.zero;
        SendValueToControl(Vector2.zero);

        foreach (var image in directionImages)
        {
            image.gameObject.SetActive(false);
        }
    }

    private Vector2 SanitizePosition(Vector2 pos)
    {
        if (Mathf.Abs(pos.x) > Mathf.Abs(pos.y))
        {
            pos.y = 0;
        }
        else
        {
            pos.x = 0;
        }

        pos = Vector2.ClampMagnitude(pos, movementRange);

        if (pos.magnitude < moveThreshold)
        {
            return Vector2.zero;
        }

        pos = new Vector2(pos.x / movementRange, pos.y / movementRange);

        if (forceIntValue)
        {
            if (pos.x != 0) pos.x = Mathf.Sign(pos.x);
            if (pos.y != 0) pos.y = Mathf.Sign(pos.y);
        }

        return pos;
    }

    private void ToggleDirectionImages(Vector2 direction)
    {
        if (direction == lastDirection) return;
        lastDirection = direction;

        foreach (var image in directionImages)
        {
            image.gameObject.SetActive(false);
        }

        if (direction.x > 0.1f) directionImages[0].gameObject.SetActive(true);
        else if (direction.x < -0.1f) directionImages[1].gameObject.SetActive(true);
        else if (direction.y > 0.1f) directionImages[2].gameObject.SetActive(true);
        else if (direction.y < -0.1f) directionImages[3].gameObject.SetActive(true);

        if (direction != Vector2.zero)
            HapticFeedback.LightFeedback();
    }
}
