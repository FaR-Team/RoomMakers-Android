using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialHandler : MonoBehaviour
{
    [SerializeField] private Animator anim;
    [SerializeField] private GameObject tutorialObject;

    private bool animPlaying;
    public int tutorialStep;

    [SerializeField] private float reminderTimer = 0f;
    private const float reminderDelay = 30f;

    [SerializeField] private FurnitureOriginalData sofaData;
    [SerializeField] private FurnitureOriginalData tvData;
    [SerializeField] private FurnitureOriginalData tableData;

    private bool extraDialogueRequested;

    public bool onTutorial;
    public bool stepStarted;
    
    public static event Action OnTutorialLockStateUpdated;

    public static TutorialHandler instance;

    private void Awake()
    {
        if (instance == null) instance = this;
        else
        {
            Destroy(gameObject); // Destroy the GameObject, not just the component
        }

        tutorialStep = 0;
        OnTutorialLockStateUpdated?.Invoke(); // Initial state
    }

    void Start()
    {
        Inventory.OnFurniturePickUp += BeginStep;
    }

    private void OnDestroy()
    {
        Inventory.OnFurniturePickUp -= BeginStep;
        // Ensure all possible subscriptions are removed
        RoomFurnitures.OnPlaceFurniture -= CompletedStepPlacedFurniture;
        RoomFurnitures.OnPlaceOnTop -= CompletedStepPlacedFurniture;
        RoomFurnitures.OnComboDone -= CompletedStepCombo;
        RoomFurnitures.OnItemUse -= CompletedStepItemUse;

        if (instance == this)
        {
            instance = null;
        }
        Debug.Log("TutorialHandler.OnDestroy() called. Instance is now: " + (instance == null ? "null" : "not null"));
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
        }
        anim.SetInteger("TutorialStep", tutorialStep);
        anim.SetBool("Completed", false);
        

        switch (tutorialStep)
        {
            case 1:
                // No comprar
                RoomFurnitures.OnPlaceFurniture += CompletedStepPlacedFurniture;
                break;
            case 2:
                break;
            case 3:
                RoomFurnitures.OnPlaceFurniture -= CompletedStepPlacedFurniture;
                RoomFurnitures.OnPlaceOnTop += CompletedStepPlacedFurniture;
                break;
            case 4:
                onTutorial = true;
                RoomFurnitures.OnPlaceOnTop -= CompletedStepPlacedFurniture;
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
        extraDialogueRequested = tutorialStep is 1 or 4 or 6;
        StateManager.PauseGame();
        if(tutorialObject != null) tutorialObject.SetActive(true);
        anim.SetInteger("TutorialStep", tutorialStep);
        anim.SetBool("Completed", true);
        animPlaying = true;
        onTutorial = false;
        stepStarted = false;
        OnTutorialLockStateUpdated?.Invoke();
    }
    
    public void CompletedStepPlacedFurniture(FurnitureOriginalData furniture)
    {
        if((tutorialStep == 1 && furniture == sofaData) || 
           (tutorialStep == 2 && furniture == tvData) ||
           (tutorialStep == 3 && furniture == tvData)) CompleteStep();
    }

    void Update()
    {
        if (PlayerController.instance.playerInput.Movement.Start.WasPressedThisFrame())
        {
            SkipTutorial();
        }

        if (Input.anyKeyDown) CloseTutorialWindow();

        if (tutorialStep is 3 or 4 && !tutorialObject.activeSelf && stepStarted)
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
        anim.SetInteger("TutorialStep", tutorialStep);
        anim.SetBool("Reminder", true);
        animPlaying = true;
    }
    public void AnimationStopped()
    {
        animPlaying = false;
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
        
        tutorialObject.SetActive(false);
            anim.SetBool("Extra", false);
        anim.SetBool("Reminder", false);
        StateManager.StartGame();
    
        reminderTimer = 0f; 
    
        if(tutorialStep > 5 && !onTutorial)
        {
            OnTutorialLockStateUpdated?.Invoke();
            Destroy(gameObject);
        }
    }

    public static bool AreDoorsTutorialLocked()
    {
        if (instance == null) return false;
        return instance.tutorialStep < 6;
    }
    
    public void SkipTutorial()
    {
        Debug.Log($"Tutorial skip requested. Current step: {tutorialStep}, onTutorial: {onTutorial}, Time.timeScale before skip: {Time.timeScale}");

        // 1. Set tutorial state to "completed" first.
        // This ensures any listeners to OnTutorialLockStateUpdated see the correct final state.
        tutorialStep = 6; 
        onTutorial = false;
        stepStarted = false;
        animPlaying = false;
        extraDialogueRequested = false;

        // 2. Signal that tutorial lock conditions have changed.
        // Rooms might listen to this to unlock doors or populate furniture.
        // Listeners will see tutorialStep = 6 and onTutorial = false.
        OnTutorialLockStateUpdated?.Invoke(); 

        // 3. Clean up tutorial UI elements.
        if (tutorialObject != null)
        {
            if (tutorialObject.activeSelf)
            {
                tutorialObject.SetActive(false);
            }
            if (anim != null) // Check if anim component exists
            {
                anim.SetBool("Extra", false);
                anim.SetBool("Reminder", false);
                // anim.SetInteger("TutorialStep", 0); // Optionally reset animator step if it affects non-UI things
            }
        }
        
        // 4. Critical: Ensure game is unpaused and running.
        StateManager.StartGame(); 
        Debug.Log($"Time.timeScale after StateManager.StartGame() in SkipTutorial: {Time.timeScale}");

        // 5. The TutorialHandler is no longer needed.
        // OnDestroy will handle unsubscribing from events and nullifying 'instance'.
        Destroy(gameObject);
        Debug.Log("TutorialHandler.SkipTutorial() finished, object destroyed.");
    }
}