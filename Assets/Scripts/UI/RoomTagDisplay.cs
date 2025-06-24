using UnityEngine;
using UnityEngine.UI;

public class RoomTagDisplay : MonoBehaviour
{
    [SerializeField] private SpriteRenderer tagDisplayImage;
    [SerializeField] private Sprite[] tagSprites;
    
    private Room currentRoom;
    
    private void Start()
    {
        // Initial update using House.instance.currentRoom
        UpdateRoomTagDisplay(House.instance.currentRoom);
        
        // Subscribe to room changes in House
        // Note: House doesn't currently have an event for room changes, so we'll need to poll in Update
    }
    
    private void Update()
    {
        // Check if the current room has changed
        if (currentRoom != House.instance.currentRoom) // TODO: Usar un evento cuando se cambia el current room en House, no update
        {
            UpdateRoomTagDisplay(House.instance.currentRoom);
        }
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events
        if (currentRoom != null)
        {
            currentRoom.OnRoomTagChanged -= OnRoomTagChanged;
        }
    }
    
    public void UpdateRoomTagDisplay(Room newRoom)
    {
        // Unsubscribe from previous room's events
        if (currentRoom != null)
        {
            currentRoom.OnRoomTagChanged -= OnRoomTagChanged;
        }
        
        // Update current room reference
        currentRoom = newRoom;
        
        // Subscribe to new room's events
        if (currentRoom != null)
        {
            currentRoom.OnRoomTagChanged += OnRoomTagChanged;
            SetTagSprite(currentRoom.roomTag);
        }
        else
        {
            SetTagSprite(RoomTag.None);
        }
    }
    
    private void OnRoomTagChanged(RoomTag newTag)
    {
        Debug.Log("" + newTag);
        SetTagSprite(newTag);
    }
    
    private void SetTagSprite(RoomTag tag)
    {
        // Convert enum to index (None = 0, Kitchen = 1, etc.)
        int spriteIndex = (int)tag;
        
        // Make sure we have a valid sprite
        if (spriteIndex >= 0 && spriteIndex < tagSprites.Length)
        {
            tagDisplayImage.sprite = tagSprites[spriteIndex];
            tagDisplayImage.enabled = true;
        }
        else if (tagSprites.Length > 0)
        {
            // Default to first sprite if tag is invalid
            tagDisplayImage.sprite = tagSprites[0];
            tagDisplayImage.enabled = true;
        }
        else
        {
            // No sprites available
            tagDisplayImage.enabled = false;
        }
    }

    public Sprite GetTagSprite(RoomTag roomTag)
    {
        int spriteIndex = (int)roomTag;
        
        if (spriteIndex >= 0 && spriteIndex < tagSprites.Length) return tagSprites[spriteIndex];
        return tagSprites.Length > 0 ? tagSprites[0] : null;
    }
}