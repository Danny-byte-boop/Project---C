using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class BarbaraMovement : MonoBehaviour
{
    [SerializeField] float moveSpeed = 5f;

    Rigidbody2D rb;
    Animator anim;
    Vector2 moveInput;
    InputSystem_Actions controls;
    InputAction move;
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        anim = GetComponent<Animator>();
        move = new InputAction("Move", InputActionType.Value);
        move.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");
    }

    void OnEnable()
    {
        controls = new InputSystem_Actions();
        controls.Player.Move.Enable();
        controls.Player.Jump.Enable();
        controls.Player.Move.performed += OnMove;
        controls.Player.Move.canceled += OnMove;
        controls.Player.Jump.started += OnJump;
    }

    void OnDisable()
    {
        controls.Player.Move.performed -= OnMove;
        controls.Player.Move.canceled -= OnMove;
        controls.Player.Jump.started -= OnJump;
        controls.Disable();
    }

    void OnDestroy() => controls?.Dispose();

    void OnMove(InputAction.CallbackContext ctx)
    {
        moveInput = Vector2.ClampMagnitude(ctx.ReadValue<Vector2>(), 1f);
    }

    void OnJump(InputAction.CallbackContext ctx)
    {
        anim.SetTrigger("Jump");  // triggers the Jump state once
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + moveInput * moveSpeed * Time.fixedDeltaTime);
        anim.SetFloat("Speed", moveInput.sqrMagnitude);  // NOT hardcoded!
    }
}
