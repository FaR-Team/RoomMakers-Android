using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class TagSelectionUI : MonoBehaviour
{
    public static TagSelectionUI instance;
    
    [SerializeField] private Transform selectedImage;
    [SerializeField] private TagButton[] tagButtons;
    private TagButton selectedButton;
    
    private Room targetRoom;
    private RoomTag selectedTag = RoomTag.None;
    private Action onConfirmCallback;

    private Controls playerInput;
    
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
        
        playerInput = PlayerController.instance.playerInput;
        gameObject.SetActive(false);
        
        //tagSelectionPanel.SetActive(false);
        
        //confirmButton.onClick.AddListener(ConfirmTagSelection);
        //cancelButton.onClick.AddListener(CancelTagSelection);
    }

    private void OnEnable()
    {
        playerInput.Movement.Movement.performed += Navigate;
    }

    private void OnDisable()
    {
        playerInput.Movement.Movement.performed -= Navigate;
    }

    private void Navigate(InputAction.CallbackContext obj)
    {
        if (!obj.performed) return;

        var dir = obj.ReadValue<Vector2>();
        selectedButton.SelectNext(dir);
    }

    public void ShowTagSelection(Room room, Action callback)
    {
        if (instance == null) return;
        gameObject.SetActive(true);
        targetRoom = room;
        onConfirmCallback = callback;
        StateManager.currentGameState = GameState.Pause; // TODO: Mejorar

        if (room.roomTag == RoomTag.None)
        {
            selectedTag = tagButtons[0].Tag;
        }
        else selectedTag = room.roomTag;
        
        
        var selectedTagButton = tagButtons.FirstOrDefault(b => b.Tag == selectedTag);
        if (selectedTagButton != null)
        {
            selectedButton = selectedTagButton;
            selectedTagButton.Select();
            //selectedImage.transform.position = selectedTagButton.transform.position;
        }
        else selectedImage.transform.position = tagButtons[0].transform.position;
        //ShowDebugTagSelection(room, callback);
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
    public void SelectTag(RoomTag tag, TagButton button)
    {
        selectedTag = tag;
        selectedImage.position = button.transform.position;
        selectedButton = button;
    }
    
    private void ConfirmTagSelection()
    {
        if (targetRoom != null && selectedTag != RoomTag.None)
        {
            targetRoom.SetRoomTag(selectedTag);
            
            // Show a notification about the room tag change
            /*Vector3 roomCenter = targetRoom.transform.position;
            ComboPopUp.Create(FindObjectOfType<RoomFurnitures>().GetComponent<ComboPopUp>(), 
                0, new Vector2(roomCenter.x, roomCenter.y), Vector2.up);*/
        }
        
        ClosePanel();
        
        // Call the callback
        onConfirmCallback?.Invoke();
    }

    private void Update()
    {
        if (playerInput.Movement.Interact.WasPressedThisFrame())
        {
            ConfirmTagSelection();
        }

        if (playerInput.Movement.Rotate.WasPressedThisFrame())
        {
            CancelTagSelection();
        }
    }

    private void CancelTagSelection()
    {
        ClosePanel();
    }
    
    private void ClosePanel()
    {
        gameObject.SetActive(false);
        StateManager.StartGame();
        Time.timeScale = 1; // Resume game
    }
}
