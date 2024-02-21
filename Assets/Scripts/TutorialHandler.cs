using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialHandler : MonoBehaviour
{
    [SerializeField] private Animator anim;
    [SerializeField] private GameObject tutorialObject;

    private bool animPlaying;
    private int tutorialStep;

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
        if(Input.GetMouseButtonDown(0)) CloseTutorialWindow();
    }

    public void CompletedStep()
    {
        StateManager.currentGameState = GameState.Pause;
        tutorialStep++;
        tutorialObject.SetActive(true);
        anim.SetInteger("TutorialStep", tutorialStep);
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
        StateManager.currentGameState = GameState.Moving;
        
        if(tutorialStep != 3) onTutorial = false;
        if(tutorialStep >= 4) Destroy(gameObject);
    }
}
