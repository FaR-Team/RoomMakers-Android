using System;
using UnityEngine;

public class FurniturePreviewMovement : MovementController
{
    private float moveCooldown = .2f;
    private float moveCooldownCounter;


    void Update()
    {
        if (StateManager.IsPaused()) return;
        if (StateManager.IsMoving()) return;

        //MoveObject();

        if (moveCooldownCounter >= 0)
        {
            moveCooldownCounter -= Time.deltaTime;
        }
        else
        {
            var Input = PlayerController.instance.playerInput.Movement.Movement.ReadValue<Vector2>();;

            if (Mathf.Abs(Input.x) == 1f)
            {
                if (Physics2D.OverlapCircle(transform.position + new Vector3(Input.x, 0f, 0f), .1f,
                        whatStopsMovement) || moveCooldownCounter > 0) return;

                transform.position += new Vector3(Input.x, 0f, 0f);;
                moveCooldownCounter = moveCooldown;
            }
            else if (Mathf.Abs(Input.y) == 1f)
            {
                if (Physics2D.OverlapCircle(transform.position + new Vector3(0f, Input.y, 0f), .1f,
                        whatStopsMovement) || moveCooldownCounter > 0) return;

                transform.position += new Vector3(0f, Input.y, 0f);
                moveCooldownCounter = moveCooldown;
            }
        }

        if (PlayerController.instance.playerInput.Movement.Rotate.WasPressedThisFrame())
        {
            GetComponent<FurniturePreview>().Rotate();
        }

        if (PlayerController.instance.playerInput.Movement.Interact.WasPressedThisFrame())
        {
            GetComponent<FurniturePreview>().PutFurniture();
        }
    }
}
