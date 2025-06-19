using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class ConfirmationScreen : MonoBehaviour
{
    public Action onConfirmAction;
    public Action onCancelAction;

    private void OnEnable()
    {
        PlayerController.instance.playerInput.Movement.Interact.performed += Confirm;
        PlayerController.instance.playerInput.Movement.Rotate.performed += Cancel;
    }
    
    private void OnDisable()
    {
        PlayerController.instance.playerInput.Movement.Interact.performed -= Confirm;
        PlayerController.instance.playerInput.Movement.Rotate.performed -= Cancel;
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }
    public void Initialize(Action onConfirm, Action onCancel)
    {
        onConfirmAction = onConfirm;
        onCancelAction = onCancel;
    }

    void Confirm(InputAction.CallbackContext ctx)
    {
        onConfirmAction?.Invoke();
        gameObject.SetActive(false);
    }
    
    void Cancel(InputAction.CallbackContext ctx)
    {
        onCancelAction?.Invoke();
        gameObject.SetActive(false);
    }
}
