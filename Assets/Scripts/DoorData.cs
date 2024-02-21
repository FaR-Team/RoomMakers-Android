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

    void Start()
    {
        
        roomPosition = transform.parent;
        thisRoom = transform.parent.GetComponent<Room>();

        SetSpawnPoint();
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
                //NextRoom.doors[1].UnlockDoor();
                break;
            case DoorType.Bottom:
                NextRoom.doors.First(x => x.doorType == DoorType.Top)?.UnlockDoor();
               // NextRoom.doors[0].UnlockDoor();
                break;
            case DoorType.Left:
                NextRoom.doors.First(x => x.doorType == DoorType.Right)?.UnlockDoor();
                //NextRoom.doors[3].UnlockDoor();
                break;
            case DoorType.Right:
                NextRoom.doors.First(x => x.doorType == DoorType.Left)?.UnlockDoor();
                //NextRoom.doors[2].UnlockDoor();
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

    public void BuyNextRoom()
    {
        if (!isUnlocked && PlayerController.instance.Inventory.money >= House.instance.DoorPrice)
        {
            PlayerController.instance.Inventory.UpdateMoney(-House.instance.DoorPrice);
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
        //Debug.Log("Check pos: " + checkPos);
        
        var door = Physics2D.OverlapCircle(checkPos, 0.2f, 1 << 10);
        // Si al instanciar esta puerta existe una al lado, desbloqueamos ambas
        if (door)
        {
            //Debug.Log("Hay puerta en " + door.transform.position);
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
}