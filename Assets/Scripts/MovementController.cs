using UnityEngine;
using UnityEngine.InputSystem;

public class MovementController : MonoBehaviour
{ 
    [SerializeField] protected Transform movePoint;
    [SerializeField] protected float moveSpeed = 5f;

    public LayerMask whatStopsMovement;
    public bool IsInEditingMode { get; private set; }
    public bool IsMoving => transform.position != movePoint.position;

    void Start()
    {
        movePoint.parent = null;
    }
    
    protected void MoveObject(Controls playerInput)
    {
        var Input = playerInput.Movement.Movement.ReadValue<Vector2>();

        if (Vector3.Distance(transform.position, movePoint.position) > .05f) return;


        if (Mathf.Abs(Input.x) == 1f)
        {
            Rotate(Vector2.right * Input.x);
            if (Physics2D.OverlapCircle(movePoint.position + new Vector3(Input.x, 0f, 0f), .1f, whatStopsMovement)) return;

            movePoint.position += new Vector3(Input.x, 0f, 0f);
        }
        else if (Mathf.Abs(Input.y) == 1f)
        {
            Rotate(Vector2.up * Input.y);
            if (Physics2D.OverlapCircle(movePoint.position + new Vector3(0f, Input.y, 0f), .1f, whatStopsMovement)) return;

            movePoint.position += new Vector3(0f, Input.y, 0f);
        }
    }

    protected virtual void Rotate(Vector2 dir)
    {
        transform.up = dir;
    }
}