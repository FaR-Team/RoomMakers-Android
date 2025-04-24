using System;
using UnityEngine;

public class FurniturePreviewMovement : MovementController
{
    private float moveCooldown = .2f;
    private float moveCooldownCounter;
    private Camera mainCam;
    private float moveRange = 5f;

    private void Awake()
    {
        mainCam = Camera.main;    
    }
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
                Vector3 nextPos = transform.position + new Vector3(Input.x, 0f, 0f);
                if (Mathf.Abs(mainCam.transform.position.x - nextPos.x) > moveRange ||
                    Physics2D.OverlapCircle(nextPos, .1f,
                        whatStopsMovement) || moveCooldownCounter > 0) return;

                transform.position += new Vector3(Input.x, 0f, 0f);;
                moveCooldownCounter = moveCooldown;
            }
            else if (Mathf.Abs(Input.y) == 1f)
            {
                Vector3 nextPos = transform.position + new Vector3(0f, Input.y, 0f);
                if (Mathf.Abs(mainCam.transform.position.y - nextPos.y) > moveRange ||
                    Physics2D.OverlapCircle(nextPos, .1f,
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
