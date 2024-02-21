using UnityEngine;

public class Room : MonoBehaviour
{
    public DoorData[] doors;
    public Vector3 cameraVector;
    public int paletteNum;
    public RoomFurnitures roomFurnitures;

    protected virtual void Start()
    {
        roomFurnitures = GetComponent<RoomFurnitures>();
    }
    public void Init()
    {
        paletteNum = Random.Range(0, ColourChanger.instance.colorPalettes.Length);
        cameraVector = new Vector3(transform.position.x, transform.position.y, -3);
        UpdateAdjacentRoomsDoors();
    }

    public void UpdateAdjacentRoomsDoors()
    {
        for (int i = 0; i < doors.Length; i++)
        {
            doors[i].CheckNextDoor();
        }
    }
}
