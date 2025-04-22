using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TagSelectionUI : MonoBehaviour
{
    public static TagSelectionUI instance;
    
    [SerializeField] private GameObject tagSelectionPanel;
    [SerializeField] private Transform tagButtonsContainer;
    [SerializeField] private Button tagButtonPrefab;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;
    
    private Room targetRoom;
    private RoomTag selectedTag = RoomTag.None;
    private Action onConfirmCallback;
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        //tagSelectionPanel.SetActive(false);
        
        //confirmButton.onClick.AddListener(ConfirmTagSelection);
        //cancelButton.onClick.AddListener(CancelTagSelection);
    }
    
    public static void ShowTagSelection(Room room, Action callback)
    {
        if (instance == null) return;
        
        // Debug version - show a simple debug menu instead of the UI
        instance.ShowDebugTagSelection(room, callback);
    }
    
    private void ShowDebugTagSelection(Room room, Action callback)
    {
        targetRoom = room;
        onConfirmCallback = callback;
        selectedTag = room.roomTag;
        
        // Create a debug menu with buttons for each tag
        Debug.Log("=== DEBUG TAG SELECTION ===");
        Debug.Log($"Current room tag: {targetRoom.roomTag}");
        Debug.Log("Press number keys to select a tag:");
        
        int index = 1;
        foreach (RoomTag tag in Enum.GetValues(typeof(RoomTag)))
        {
            if (tag == RoomTag.None) continue;
            Debug.Log($"{index}. {tag}");
            index++;
        }
        
        // Start listening for key presses
        StartCoroutine(WaitForTagSelection());
    }
    
    private System.Collections.IEnumerator WaitForTagSelection()
    {
        bool selectionMade = false;
        
        while (!selectionMade)
        {
            // Check for number key presses
            for (int i = 1; i <= Enum.GetValues(typeof(RoomTag)).Length - 1; i++) // -1 to skip None
            {
                if (Input.GetKeyDown(KeyCode.Alpha0 + i) || Input.GetKeyDown(KeyCode.Keypad0 + i))
                {
                    // Get the tag at this index
                    RoomTag[] tags = (RoomTag[])Enum.GetValues(typeof(RoomTag));
                    if (i < tags.Length)
                    {
                        // Skip None (index 0)
                        selectedTag = tags[i];
                        Debug.Log($"Selected tag: {selectedTag}");
                        
                        // Apply the tag immediately
                        if (targetRoom != null && selectedTag != RoomTag.None)
                        {
                            targetRoom.SetRoomTag(selectedTag);
                            Debug.Log($"Room tagged as {selectedTag}!");
                            
                            // Show a notification about the room tag change
                            Vector3 roomCenter = targetRoom.transform.position;
                        }
                        
                        // Call the callback
                        onConfirmCallback?.Invoke();
                        selectionMade = true;
                        break;
                    }
                }
            }
            
            // Check for cancel
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Debug.Log("Tag selection cancelled");
                selectionMade = true;
                onConfirmCallback?.Invoke();
            }
            
            yield return null;
        }
        
        Time.timeScale = 1; // Resume game
    }
    
    // Para cuando hagamos la UI
    public void SelectTag(RoomTag tag, Button button)
    {
        selectedTag = tag;
        
        // Reset all button colors
        foreach (Transform child in tagButtonsContainer)
        {
            child.GetComponent<Image>().color = Color.white;
        }
        
        // Highlight selected button
        button.GetComponent<Image>().color = new Color(0.8f, 0.8f, 1f);
    }
    
    private void ConfirmTagSelection()
    {
        if (targetRoom != null && selectedTag != RoomTag.None)
        {
            targetRoom.SetRoomTag(selectedTag);
            
            // Show a notification about the room tag change
            Vector3 roomCenter = targetRoom.transform.position;
            ComboPopUp.Create(FindObjectOfType<RoomFurnitures>().GetComponent<ComboPopUp>(), 
                0, new Vector2(roomCenter.x, roomCenter.y), Vector2.up);
        }
        
        ClosePanel();
        
        // Call the callback
        onConfirmCallback?.Invoke();
    }
    
    private void CancelTagSelection()
    {
        ClosePanel();
    }
    
    private void ClosePanel()
    {
        tagSelectionPanel.SetActive(false);
        Time.timeScale = 1; // Resume game
    }
}
