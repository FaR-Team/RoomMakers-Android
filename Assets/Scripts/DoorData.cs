using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DoorData : MonoBehaviour
{
    public enum DoorType
    {
        Top,
        Bottom,
        Left,
        Right
    }

    public bool isUnlocked = false;

    public DoorType doorType;
    public Vector3 spawnPoint;
    private Room NextRoom;
    private Room thisRoom;
    private Transform roomPosition;
    private Vector3 roomCameraPosition;

    [SerializeField] private Sprite openDoor;
    
    [Header("Shake Settings")]
    [SerializeField] private float shakeDuration = 0.2f;
    [SerializeField] private float shakeMagnitude = 0.03f;

    [Header("Tutorial Settings")]
    [SerializeField] private GameObject lockedTutorialIndicator;

    void Start()
    {
        roomPosition = transform.parent;
        thisRoom = transform.parent.GetComponent<Room>();

        SetSpawnPoint();
        UpdateTutorialLockVisual(); 
    }

    private void OnEnable()
    {
        TutorialHandler.OnTutorialLockStateUpdated += UpdateTutorialLockVisual;
        UpdateTutorialLockVisual();
    }

    private void OnDisable()
    {
        TutorialHandler.OnTutorialLockStateUpdated -= UpdateTutorialLockVisual;
    }

    void SetSpawnPoint()
    {
        if (roomPosition == null)
        {
            roomPosition = transform.parent;
        }

        switch (doorType)
        {
            case DoorType.Top:
                spawnPoint = new Vector3(roomPosition.position.x, roomPosition.position.y + 9, roomPosition.position.z);
                break;
            case DoorType.Bottom:
                spawnPoint = new Vector3(roomPosition.position.x, roomPosition.position.y - 9, roomPosition.position.z);
                break;
            case DoorType.Left:
                spawnPoint = new Vector3(roomPosition.position.x - 10, roomPosition.position.y, roomPosition.position.z);
                break;
            case DoorType.Right:
                spawnPoint = new Vector3(roomPosition.position.x + 10, roomPosition.position.y, roomPosition.position.z);
                break;
            default:
                break;
        }
    }

    public void UnlockOtherRoomsDoor()
    {
        switch (doorType)
        {
            case DoorType.Top:
                NextRoom.doors.First(x => x.doorType == DoorType.Bottom)?.UnlockDoor();
                break;
            case DoorType.Bottom:
                NextRoom.doors.First(x => x.doorType == DoorType.Top)?.UnlockDoor();
                break;
            case DoorType.Left:
                NextRoom.doors.First(x => x.doorType == DoorType.Right)?.UnlockDoor();
                break;
            case DoorType.Right:
                NextRoom.doors.First(x => x.doorType == DoorType.Left)?.UnlockDoor();
                break;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            if (isUnlocked && House.instance.currentRoom != thisRoom)
            {
                House.instance.TransitionToRoom(thisRoom.cameraVector, thisRoom.paletteNum);
            }
        }
        else Debug.Log("NO FUNCIONA");
    }

    // Modified to support free doors from debug menu
    public void BuyNextRoom(bool isFree = false)
    {
        if (TutorialHandler.AreDoorsTutorialLocked())
        {
            AudioManager.instance.PlaySfx(GlobalSfx.Error);
            StartCoroutine(ShakeDoorCoroutine());
            return;
        }

        if (!isUnlocked && (isFree || PlayerController.instance.Inventory.money >= House.instance.DoorPrice))
        {
            // Only deduct money if it's not a free door
            if (!isFree)
            {
                PlayerController.instance.Inventory.UpdateMoney(-House.instance.DoorPrice);
            }

            NextRoom = House.instance.SpawnRoom(spawnPoint);
            AudioManager.instance.PlaySfx(GlobalSfx.Grab);
            UnlockDoor();
            UnlockOtherRoomsDoor();
        }
    }

    public void UnlockDoor()
    {
        isUnlocked = true;

        // Al desbloquear, hacemos el collider un trigger, y que la layer sea de puerta desbloqueada
        GetComponent<Collider2D>().isTrigger = true;
        GetComponent<SpriteRenderer>().sprite = openDoor;
        gameObject.layer = 7;
        UpdateTutorialLockVisual(); // Hide tutorial lock indicator as door is now unlocked
    }

    private IEnumerator ShakeDoorCoroutine()
    {
        Vector3 originalPosition = transform.localPosition;

        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            float xOffset = Mathf.Sin(elapsed * (Mathf.PI / shakeDuration) * 10f) * shakeMagnitude;
            transform.localPosition = originalPosition + new Vector3(xOffset, 0f, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPosition;
    }
    public void CheckNextDoor()
    {
        if (isUnlocked) return;

        Vector2 checkPos;
        switch (doorType)
        {
            case DoorType.Top:
                checkPos = transform.position + Vector3.up;
                break;
            case DoorType.Bottom:
                checkPos = transform.position - Vector3.up;
                break;
            case DoorType.Left:
                checkPos = transform.position - Vector3.right;
                break;
            case DoorType.Right:
                checkPos = transform.position + Vector3.right;
                break;
            default: checkPos = Vector2.zero;
                break;
        } 
        
        var door = Physics2D.OverlapCircle(checkPos, 0.2f, 1 << 10);
        // Si al instanciar esta puerta existe una al lado, desbloqueamos ambas
        if (door)
        {
            door.TryGetComponent(out DoorData doorData);

            if (doorData)
            {
                NextRoom = doorData.thisRoom;
                doorData.NextRoom = thisRoom;
                UnlockDoor();
                doorData.UnlockDoor();
            }
        }
    }

    private void UpdateTutorialLockVisual()
    {
        if (lockedTutorialIndicator != null)
        {
            bool shouldShowLock = TutorialHandler.AreDoorsTutorialLocked() && !isUnlocked;
            lockedTutorialIndicator.SetActive(shouldShowLock);
        }
    }
}