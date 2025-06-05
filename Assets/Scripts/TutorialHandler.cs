using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialHandler : MonoBehaviour
{
    [SerializeField] private Animator anim;
    [SerializeField] private GameObject tutorialObject;
    [SerializeField] private GameObject bButtonPromptObject; // Prompt for B button interaction
    [SerializeField] private GameObject skipPromptObject; // Prompt for tutorial skip
    
    [SerializeField] TutorialStepData[] stepsData;

    private bool animPlaying;
    public int tutorialStep;

    [SerializeField] private float reminderTimer = 0f;
    private const float reminderDelay = 30f;

    [SerializeField] private FurnitureOriginalData sofaData;
    [SerializeField] private FurnitureOriginalData tvData;
    [SerializeField] private FurnitureOriginalData tableData;

    private const string TutorialCompletedKey = "TutorialCompleted";

    private bool extraDialogueRequested;
    private bool _canSkip;

    public bool onTutorial;
    public bool stepStarted;
    public bool dialogueBoxOpen;

    private GameObject _currentPreviewAnimation;
    public static event Action OnTutorialLockStateUpdated;

    public static TutorialHandler instance;

    private void Awake()
    {
        if (instance == null) instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        tutorialStep = 0;
        OnTutorialLockStateUpdated?.Invoke();

        if (bButtonPromptObject != null)
        {
            bButtonPromptObject.SetActive(false);
        }

        _canSkip = PlayerPrefs.GetInt(TutorialCompletedKey, 0) != 0;
    }

    void Start()
    {
        Inventory.OnFurniturePickUp += BeginStep;
    }

    private void OnDestroy()
    {
        Inventory.OnFurniturePickUp -= BeginStep;
        RoomFurnitures.OnPlaceFurniture -= CompletedStepPlacedFurniture;
        RoomFurnitures.OnPlaceOnTopEvent -= CompletedStepPlacedFurniture;
        RoomFurnitures.OnComboDone -= CompletedStepCombo;
        RoomFurnitures.OnItemUse -= CompletedStepItemUse;

        if (bButtonPromptObject != null && bButtonPromptObject.activeSelf)
        {
            bButtonPromptObject.SetActive(false);
        }

        if (instance == this)
        {
            instance = null;
        }
        Debug.Log("TutorialHandler.OnDestroy() called. Instance is now: " + (instance == null ? "null" : "not null"));
    }
    
    private void UpdateBButtonPromptState()
    {
        if (bButtonPromptObject == null) return;

        bool shouldBeActive = tutorialObject != null && tutorialObject.activeSelf && !animPlaying;
        bButtonPromptObject.SetActive(shouldBeActive);
    }

    private void BeginStep(FurnitureOriginalData obj)
    {
        if (stepStarted) return;
        
        tutorialStep++;
        stepStarted = true;
        OnTutorialLockStateUpdated?.Invoke();

        if (tutorialObject != null)
        {
            StateManager.PauseGame();
            tutorialObject.SetActive(true);
            dialogueBoxOpen = true;
            
            skipPromptObject.SetActive(_canSkip && tutorialStep == 1);
            if(stepsData[tutorialStep - 1].animOnBeginStep) _currentPreviewAnimation = stepsData[tutorialStep - 1].animOnBeginStep;
            _currentPreviewAnimation?.SetActive(true);
        }
        anim.SetInteger("TutorialStep", tutorialStep);
        anim.SetBool("Completed", false);
        animPlaying = true;
        UpdateBButtonPromptState();

        switch (tutorialStep)
        {
            case 1:
                RoomFurnitures.OnPlaceFurniture += CompletedStepPlacedFurniture;
                break;
            case 2:
                break;
            case 3:
                RoomFurnitures.OnPlaceFurniture -= CompletedStepPlacedFurniture;
                RoomFurnitures.OnPlaceOnTopEvent += CompletedStepPlacedFurniture;
                break;
            case 4:
                onTutorial = true;
                RoomFurnitures.OnPlaceOnTopEvent -= CompletedStepPlacedFurniture;
                RoomFurnitures.OnComboDone += CompletedStepCombo;
                break;
            case 5:
                onTutorial = true;
                RoomFurnitures.OnComboDone -= CompletedStepCombo;
                RoomFurnitures.OnItemUse += CompletedStepItemUse;
                break;
            case 6:
                onTutorial = true;
                break;
            default:
                break;
        }
    }

    private void CompletedStepItemUse(ItemType obj)
    {
        if ((tutorialStep == 5 && obj == ItemType.Tagger) || (tutorialStep == 6 && obj == ItemType.Sledgehammer))
        {
            CompleteStep();
            Debug.Log("Completed step from placement");
        }
    }

    private void CompletedStepCombo(int obj)
    {
        Debug.Log("Completed step from combo");
        CompleteStep();
    }

    public void CompleteStep()
    {
        if (!stepStarted) return;

        if (stepsData[tutorialStep - 1].hasCompletedAnim)
        {
            extraDialogueRequested = stepsData[tutorialStep - 1].hasExtraDialogue;
            StateManager.PauseGame();

            if (tutorialObject != null)
            {
                tutorialObject.SetActive(true);
                dialogueBoxOpen = true;
            }
            
            anim.SetInteger("TutorialStep", tutorialStep);
            anim.SetBool("Completed", true);
            animPlaying = true;
            UpdateBButtonPromptState();
        }

        onTutorial = false;
        stepStarted = false;

        if (tutorialStep == 6)
        {
            tutorialStep = 7;
        }
        OnTutorialLockStateUpdated?.Invoke();
    }
    
    public void CompletedStepPlacedFurniture(FurnitureOriginalData furniture)
    {
        if((tutorialStep == 1 && furniture == sofaData) || 
           (tutorialStep == 2 && furniture == tableData) ||
           (tutorialStep == 3 && furniture == tvData)) CompleteStep();
    }

    void Update()
    {
        if (_canSkip && PlayerController.instance.playerInput.Movement.Start.WasPressedThisFrame())
        {
            SkipTutorial();
        }

        if (PlayerController.instance != null && PlayerController.instance.playerInput != null && PlayerController.instance.playerInput.Movement.Rotate.WasPressedThisFrame()) CloseTutorialWindow();

        if (stepStarted && stepsData[tutorialStep - 1].hasReminder && !tutorialObject.activeSelf)
        {
            reminderTimer += Time.deltaTime;
            if (reminderTimer >= reminderDelay)
            {
                ReminderStep();
                reminderTimer = 0f;
            }
        }
    }

    public void ReminderStep()
    {
        if (StateManager.IsEditing()) 
        {
            PlayerController.instance.ForceSwitchEditingMode();
        }
        StateManager.PauseGame();
        tutorialObject.SetActive(true);
        dialogueBoxOpen = true;
        anim.SetInteger("TutorialStep", tutorialStep);
        anim.SetBool("Reminder", true);
        animPlaying = true;
        UpdateBButtonPromptState();
    }
    public void AnimationStopped()
    {
        animPlaying = false;
        UpdateBButtonPromptState();
    }
    public void CloseTutorialWindow()
    {
        if(!tutorialObject.activeSelf || animPlaying) return;
        
        if (extraDialogueRequested)
        {
            anim.SetBool("Extra", true);
            extraDialogueRequested = false;
            animPlaying = true;
            return;
        }

        dialogueBoxOpen = false;
        tutorialObject.SetActive(false);
        skipPromptObject.SetActive(false);
        _currentPreviewAnimation?.SetActive(false);
        _currentPreviewAnimation = null;
        anim.SetBool("Extra", false);
        anim.SetBool("Reminder", false);
        StateManager.StartGame();
        UpdateBButtonPromptState();

        reminderTimer = 0f; 

        if(tutorialStep > 5 && !onTutorial)
        {
            OnTutorialLockStateUpdated?.Invoke();
            FinishTutorial();
        }

    }

    public static bool AreDoorsTutorialLocked()
    {
        if (instance == null) return false;
        return instance.tutorialStep <= 6;
    }
    
    public void SkipTutorial()
    {
        Debug.Log($"Tutorial skip requested. Current step: {tutorialStep}, onTutorial: {onTutorial}, Time.timeScale before skip: {Time.timeScale}");

        _currentPreviewAnimation?.SetActive(false);
        tutorialStep = 7; 
        onTutorial = false;
        stepStarted = false;
        animPlaying = false;
        extraDialogueRequested = false;
        _currentPreviewAnimation = null;

        OnTutorialLockStateUpdated?.Invoke(); 

        if (tutorialObject != null)
        {
            if (tutorialObject.activeSelf)
            {
                tutorialObject.SetActive(false);
            }
            if (anim != null)
            {
                anim.SetBool("Extra", false);
                anim.SetBool("Reminder", false);
            }
        }
        
        StateManager.StartGame(); 
        Debug.Log($"Time.timeScale after StateManager.StartGame() in SkipTutorial: {Time.timeScale}");
        
        UpdateBButtonPromptState();

        FinishTutorial();
        Debug.Log("TutorialHandler.SkipTutorial() finished, object destroyed.");
    }

    void FinishTutorial()
    {
        Destroy(gameObject);
        PlayerPrefs.SetInt("TutorialCompleted", 1);
    }

    public bool CanSpawnPackage()
    {
        return !stepStarted && !dialogueBoxOpen;
    }
}

[System.Serializable]
public struct TutorialStepData
{
    public string stepName;
    public bool hasCompletedAnim;
    public bool hasReminder;
    public bool hasExtraDialogue;
    public GameObject animOnBeginStep;
}