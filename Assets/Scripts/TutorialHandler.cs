using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialHandler : MonoBehaviour
{
    [SerializeField] private Animator anim;
    [SerializeField] private GameObject tutorialObject;

    private bool animPlaying;
    [SerializeField] private int tutorialStep;

    [SerializeField] private float reminderTimer = 0f;
    private const float reminderDelay = 30f;

    [SerializeField] private FurnitureOriginalData sofaData;
    [SerializeField] private FurnitureOriginalData tvData;
    [SerializeField] private FurnitureOriginalData tableData;

    private bool extraDialogueRequested;

    public bool onTutorial;
    public bool stepStarted;
    
    public static TutorialHandler instance;

    private void Awake()
    {
        if (instance == null) instance = this;
        else
        {
            Destroy(this);
        }

        tutorialStep = 0;
    }
    
    void Start()
    {
        Inventory.OnFurniturePickUp += BeginStep;
    }

    private void OnDestroy()
    {
        Inventory.OnFurniturePickUp -= BeginStep;
        RoomFurnitures.OnItemUse -= CompletedStepItemUse;
    }

    private void BeginStep(FurnitureOriginalData obj)
    {
        if (stepStarted) return;
        
        tutorialStep++;
        stepStarted = true;

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
        
    }
    
    public void CompletedStepPlacedFurniture(FurnitureOriginalData furniture)
    {
        if((tutorialStep == 1 && furniture == sofaData) || 
           (tutorialStep == 2 && furniture == tvData) ||
           (tutorialStep == 3 && furniture == tvData)) CompleteStep();
    }

    void Update()
    {
        if(Input.anyKeyDown) CloseTutorialWindow();

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
    
        reminderTimer = 0f; // Reset the timer when closing the window
    
        if(tutorialStep > 5 && !onTutorial)
        {
            Destroy(gameObject);
        }
    }
}