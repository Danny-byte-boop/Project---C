using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public enum ControlScheme { Arrows, WASD, Gamepad }

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] ControlScheme controlScheme = ControlScheme.Arrows;
    [SerializeField] float moveSpeed = 5f;

    [Header("Controls")]
    [SerializeField] Key jumpKeyOverride = Key.None;   // set to L for Barbara
    [SerializeField] bool debugLogs = true;

    [Header("Platformer")]
    [SerializeField] float jumpForce = 12f;
    [SerializeField] float platformerGravity = 3f;
    [SerializeField] Transform groundCheck;
    [SerializeField] float groundCheckRadius = 0.2f;
    [SerializeField] LayerMask groundLayer;

    [Header("Top-down jump (animation only)")]
    [SerializeField] float topDownJumpTime = 0.5f;

    public ControlScheme Scheme => controlScheme;
    public bool PlatformerMode { get; private set; }
    [SerializeField] int extraJumps = 0;   // 0 = normal jump, 1 = double jump
    Rigidbody2D rb;
    Animator anim;
    Vector2 moveInput;
    InputAction moveAction;
    InputAction jumpAction;
    HashSet<string> animParams;
    bool isGrounded = true;
    int extraJumpsLeft;
    bool topDownJumping;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        anim = GetComponent<Animator>();
        CacheAnimatorParameters();

        moveAction = new InputAction("Move", InputActionType.Value);
        jumpAction = new InputAction("Jump", InputActionType.Button);

        switch (controlScheme)
        {
            case ControlScheme.Arrows:
                moveAction.AddCompositeBinding("2DVector")
                    .With("Up", "<Keyboard>/upArrow")
                    .With("Down", "<Keyboard>/downArrow")
                    .With("Left", "<Keyboard>/leftArrow")
                    .With("Right", "<Keyboard>/rightArrow");
                jumpAction.AddBinding("<Keyboard>/rightShift");
                break;

            case ControlScheme.WASD:
                moveAction.AddCompositeBinding("2DVector")
                    .With("Up", "<Keyboard>/w")
                    .With("Down", "<Keyboard>/s")
                    .With("Left", "<Keyboard>/a")
                    .With("Right", "<Keyboard>/d");
                jumpAction.AddBinding("<Keyboard>/space");
                break;

            case ControlScheme.Gamepad:
                moveAction.AddBinding("<Gamepad>/leftStick");
                jumpAction.AddBinding("<Gamepad>/buttonSouth");
                break;
        }

        if (jumpKeyOverride != Key.None)
        {
            jumpAction.Dispose();
            jumpAction = new InputAction("Jump", InputActionType.Button);
            jumpAction.AddBinding($"<Keyboard>/{jumpKeyOverride}");
        }

        Log($"{name} ready. Scheme={controlScheme}, jump binding={jumpAction.bindings[0].path}");
    }

    void Start()
    {
        SetBool("IsGrounded", true);
    }

    void OnEnable()
    {
        moveAction.Enable();
        jumpAction.Enable();
        jumpAction.started += OnJump;
    }

    void OnDisable()
    {
        jumpAction.started -= OnJump;
        moveAction.Disable();
        jumpAction.Disable();
    }

    void OnDestroy()
    {
        moveAction?.Dispose();
        jumpAction?.Dispose();
    }

    public void SetPlatformerMode(bool on)
    {
        PlatformerMode = on;
        rb.gravityScale = on ? platformerGravity : 0f;
        rb.linearVelocity = Vector2.zero;
        extraJumpsLeft = extraJumps;
    }

    void OnJump(InputAction.CallbackContext ctx)
    {
        Log($"{name} jump pressed. Platformer={PlatformerMode}, grounded={isGrounded}, extraJumpsLeft={extraJumpsLeft}");

        if (PlatformerMode)
        {
            if (isGrounded)
            {
                // normal jump
            }
            else if (extraJumpsLeft > 0)
            {
                extraJumpsLeft--;   // spend the air jump
            }
            else
            {
                return;             // no jumps left
            }

            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            SetTrigger("Jump");
        }
        else if (!topDownJumping)
        {
            StartCoroutine(TopDownJump());
        }
    }

    // Top-down: the jump is only the animation
    IEnumerator TopDownJump()
    {
        topDownJumping = true;
        SetTrigger("Jump");
        SetBool("IsGrounded", false);
        yield return new WaitForSeconds(topDownJumpTime);
        SetBool("IsGrounded", true);
        topDownJumping = false;
    }

    void FixedUpdate()
    {
        moveInput = Vector2.ClampMagnitude(moveAction.ReadValue<Vector2>(), 1f);

        if (PlatformerMode)
        {
            isGrounded = groundCheck != null
                && Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer)
                && rb.linearVelocity.y <= 0.01f;
            if (isGrounded) extraJumpsLeft = extraJumps;

            rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);
            SetBool("IsGrounded", isGrounded);
            SetFloat("Speed", Mathf.Abs(moveInput.x));
        }
        else
        {
            rb.linearVelocity = moveInput * moveSpeed;
            SetFloat("Speed", moveInput.sqrMagnitude);
        }
    }

    // Animator helpers: skip parameters the controller doesn't have
    void CacheAnimatorParameters()
    {
        animParams = new HashSet<string>();
        if (anim.runtimeAnimatorController == null) return;
        foreach (var p in anim.parameters) animParams.Add(p.name);
    }

    void SetFloat(string n, float v) { if (animParams.Contains(n)) anim.SetFloat(n, v); }
    void SetBool(string n, bool v) { if (animParams.Contains(n)) anim.SetBool(n, v); }
    void SetTrigger(string n) { if (animParams.Contains(n)) anim.SetTrigger(n); }

    void Log(string msg)
    {
        if (debugLogs) Debug.Log(msg);
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}