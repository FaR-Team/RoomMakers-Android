using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialAnimationEvents : MonoBehaviour
{
    [SerializeField] private TutorialHandler tutorialHandler;

    public void StopAnim()
    {
        tutorialHandler.AnimationStopped();
    }
}
