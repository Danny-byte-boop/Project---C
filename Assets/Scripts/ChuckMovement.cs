
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class ChuckMovement : MonoBehaviour
{
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float jumpForce = 7f;
    [SerializeField] Transform groundCheck;
    [SerializeField] float groundCheckRadius = 0.2f;
    [SerializeField] LayerMask groundLayer;

    Rigidbody2D rb;
    Animator anim;
    Vector2 moveInput;
    InputSystem_Actions controls;
    InputAction move;
    bool isGrounded;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 2f;
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
        if (controls == null) return;

        controls.Player.Move.performed -= OnMove;
        controls.Player.Move.canceled -= OnMove;
        controls.Player.Jump.started -= OnJump;

        controls.Disable();
    }

    void OnDestroy()
    {
        controls?.Dispose();
    }

    void OnMove(InputAction.CallbackContext ctx)
    {
        moveInput = Vector2.ClampMagnitude(
            ctx.ReadValue<Vector2>(), 1f
        );
    }

    void OnJump(InputAction.CallbackContext ctx)
    {
        anim.SetTrigger("Jump");
    }

    void FixedUpdate()
    {
        rb.linearVelocity = moveInput * moveSpeed;

        if (moveInput == Vector2.zero)
        {
            rb.linearVelocity = Vector2.zero;
        }

        anim.SetFloat("Speed", moveInput.sqrMagnitude);
    }
}

