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


    public bool onTutorial;
    
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
        
    }
    void Update()
    {
        if(Input.anyKeyDown) CloseTutorialWindow();

        if (tutorialStep == 3 && !tutorialObject.activeSelf)
        {
            reminderTimer += Time.deltaTime;
            if (reminderTimer >= reminderDelay)
            {
                ReminderStep();
                reminderTimer = 0f;
            }
        }
    }

    public void CompletedStep()
    {
        StateManager.currentGameState = GameState.Pause;
        tutorialStep++;
        tutorialObject.SetActive(true);
        anim.SetInteger("TutorialStep", tutorialStep);
        animPlaying = true;
    }

    public void ReminderStep()
    {
        if (StateManager.IsEditing()) 
	    {
		    reminderTimer = 0f;
		    return;
	    }
        
        StateManager.currentGameState = GameState.Pause;
        tutorialObject.SetActive(true);
        anim.SetInteger("TutorialStep", 10);
        animPlaying = true;
    }
    
    public void AnimationStopped()
    {
        animPlaying = false;
    }
    public void CloseTutorialWindow()
    {
        if(!tutorialObject.activeSelf || animPlaying) return;
        tutorialObject.SetActive(false);
        if(tutorialStep != 3) onTutorial = false;
    
        StateManager.currentGameState = GameState.Moving;

        if (tutorialStep == 4)
        {
            CompletedStep();
            return;
        }
    
        reminderTimer = 0f; // Reset the timer when closing the window
    
        if(tutorialStep > 4)
        {
            Destroy(gameObject);
        }
    }
}