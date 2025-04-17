using UnityEngine;

public class Room : MonoBehaviour
{
    public DoorData[] doors;
    public Vector3 cameraVector;
    public int paletteNum;
    public RoomFurnitures roomFurnitures;
    
    [Header("Tagging System")]
    public RoomTag roomTag = RoomTag.None;
    public bool hasTagBeenSet = false;

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
    
    public void SetRoomTag(RoomTag newTag)
    {
        roomTag = newTag;
        hasTagBeenSet = true;
        
        // TODO: UI para esto, ya sea iconitos, o lo que verga haya
        Debug.Log($"Room tag set to: {newTag}");
    }
    
    public bool TrySetRoomTagFromFurniture(RoomTag furnitureTag)
    {
        // Only set the room tag if it hasn't been set before
        if (!hasTagBeenSet && furnitureTag != RoomTag.None)
        {
            SetRoomTag(furnitureTag);
            return true;
        }
        return false;
    }
}
