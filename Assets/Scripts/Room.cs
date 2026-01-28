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
        if (ColourChanger.instance.palettesDatabase != null && ColourChanger.instance.palettesDatabase.palettes != null)
        {
            paletteNum = Random.Range(1, ColourChanger.instance.palettesDatabase.palettes.Count);
        }
        else
        {
            paletteNum = 0; // Fallback
        }
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
    
    public delegate void RoomTagChangedHandler(RoomTag newTag);
    public event RoomTagChangedHandler OnRoomTagChanged;
    
    public virtual void SetRoomTag(RoomTag newTag)
    {
        if (roomTag != newTag)
        {
            roomTag = newTag;
            OnRoomTagChanged?.Invoke(roomTag);
        }
    }
    
    public virtual bool TrySetRoomTagFromFurniture(RoomTag newTag)
    {
        if (roomTag == RoomTag.None && newTag != RoomTag.None)
        {
            roomTag = newTag;
            OnRoomTagChanged?.Invoke(roomTag);
            return true;
        }
        return false;
    }}
