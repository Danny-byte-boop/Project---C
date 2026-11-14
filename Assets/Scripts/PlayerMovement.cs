using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
// The control options a character can use. Chosen per character in the Inspector.
public enum ControlScheme { Arrows, WASD, Gamepad }
/// <summary>
/// Controls one character (Chuck or Barbara): movement, jumping and animation.
/// It works in two modes:
///  - Top-down (the coconut trial): moves in all four directions, and the jump is animation only.
///  - Platformer: left/right movement with gravity, a real jump, and optional extra jumps.
/// Each character picks its own keys with the Control Scheme setting, so the two
/// players never control each other.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class PlayerMovement : MonoBehaviour
{
    // These are the Inspector Settings
    // which keys this character uses
    [SerializeField] ControlScheme controlScheme = ControlScheme.Arrows;
    // This is the walking speed
    [SerializeField] float moveSpeed = 5f;                                  

    [Header("Controls")]
    //set a key here (e.g. L) to replace the scheme's default jump key. This is for BARBARA so that she doesn't have 
    //the same jump key as Chuck
    [SerializeField] Key jumpKeyOverride = Key.None;   
    

    [Header("Platformer")]
    // This is the upward speed of a jump
    [SerializeField] float jumpForce = 12f;
    // 0 = normal jump, 1 = double jump, 2 = triple jump, and BARBARA will have a double jump
    [SerializeField] int extraJumps = 0;
    // This is the gravity strength while in platformer mode
    [SerializeField] float platformerGravity = 3f;
    // This is a small empty object at the character's feet
    [SerializeField] Transform groundCheck;
    // This is the size of the "am I touching the ground?" circle
    [SerializeField] float groundCheckRadius = 0.2f;   
    //This determines which layers count as ground
    [SerializeField] LayerMask groundLayer;            
    [Header("Top-down jump (animation only)")]
    // how long the jump animation plays in the trial, so in this case it's suppose to be very brief
    [SerializeField] float topDownJumpTime = 0.5f;
    // This is information of other scripts that can be read

    // PlayerCarry reads this to pick matching key
    public ControlScheme Scheme => controlScheme;      
    //This is true once the platformer has started
    public bool PlatformerMode { get; private set; }   

    // These are all private working variables

    Rigidbody2D rb;
    Animator anim;
    //This is the direction the player is pushing
    Vector2 moveInput;
    // These are the movement keys
    InputAction moveAction;         
    //This is the jump key
    InputAction jumpAction;
    // These are the names of the parameters this character's Animator really has
    HashSet<string> animParams;
    //Are the characters standing on the ground?
    bool isGrounded = true;
    // is the trial-mode jump animation playing?
    bool topDownJumping;            
    //How many air jumps are still available
    int extraJumpsLeft;            

    // This is for the Setup

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        // There is no gravity in the top-down trial because you'd slide off
        rb.gravityScale = 0f;
        // This stops the character from tipping over
        rb.freezeRotation = true;       
        anim = GetComponent<Animator>();
        CacheAnimatorParameters();

        // This creates the characters own InputAction, so that BARBARA and Chuck don't share the same keys
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

        // If a custom jump key is set, throw away the default jump binding and use that key instead, this is for BARBARA
        if (jumpKeyOverride != Key.None)
        {
            jumpAction.Dispose();
            jumpAction = new InputAction("Jump", InputActionType.Button);
            jumpAction.AddBinding($"<Keyboard>/{jumpKeyOverride}");
        }

        
    }

    void Start()
    {
        SetBool("IsGrounded", true);
    }

    // This turns on the inputs when the script is enabled
    void OnEnable()
    {
        moveAction.Enable();
        jumpAction.Enable();
        // This calls OnJump the moment the key is pressed
        jumpAction.started += OnJump;   
    }

    // This turns it off when it is disabled (for example while dialogue is playing)
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
    // This is for the Platformer switch
    // This is called by TrialManager when the platformer level begins
    public void SetPlatformerMode(bool on)
    {
        PlatformerMode = on;
        // gravity only exists in the platformer, and not in the timed trial.
        rb.gravityScale = on ? platformerGravity : 0f;   
        rb.linearVelocity = Vector2.zero;
        extraJumpsLeft = extraJumps;
    }

    // ---------- Jumping ----------

    void OnJump(InputAction.CallbackContext ctx)
    {
       

        if (PlatformerMode)
        {
            //This is for the normal jump from the ground
            if (isGrounded)
            {
                
            }
            else if (extraJumpsLeft > 0)
            {
                // spend an air jump (double jump) for BARBARA and Chuck, Chuck gets one jump and BARBARA gets two
                extraJumpsLeft--;   
            }
            else
            {
                //You're in the air with no jumps left
                return;             
            }

            // This allows for the helping hand to launch Chuck upward but keep the current sideways speed
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            SetTrigger("Jump");
        }
        else if (!topDownJumping)
        {
            StartCoroutine(TopDownJump());
        }
    }

    // In the top-down trial the jump is just an animation, so this pretends to leave the
    // ground for a moment, then land again an illusion of Chuck and BARBARA's movement
    IEnumerator TopDownJump()
    {
        topDownJumping = true;
        SetTrigger("Jump");
        SetBool("IsGrounded", false);
        yield return new WaitForSeconds(topDownJumpTime);
        SetBool("IsGrounded", true);
        topDownJumping = false;
    }

    // This is the movement that runs on the physics timer

    void FixedUpdate()
    {
        // This reads the movement keys, capped so diagonals aren't faster
        moveInput = Vector2.ClampMagnitude(moveAction.ReadValue<Vector2>(), 1f);

        if (PlatformerMode)
        {
            // Are both players touching the ground? Only counts when not rising, so a jump doesn't instantly re-ground
            isGrounded = groundCheck != null
                && Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer)
                && rb.linearVelocity.y <= 0.01f;
            // Landing for both Chuck and BARBARA refills their air jumps
            if (isGrounded) extraJumpsLeft = extraJumps;   

            // Only control sideways speed, so gravity and jumps still work
            rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);
            SetBool("IsGrounded", isGrounded);
            SetFloat("Speed", Mathf.Abs(moveInput.x));
        }
        else
        {
            // Top-down: Both players can move freely in every direction
            rb.linearVelocity = moveInput * moveSpeed;
            SetFloat("Speed", moveInput.sqrMagnitude);
        }
    }
    // This remembers which parameters this character's Animator has
    void CacheAnimatorParameters()
    {
        animParams = new HashSet<string>();
        if (anim.runtimeAnimatorController == null) return;
        foreach (var p in anim.parameters) animParams.Add(p.name);
    }

    // These only set a parameter if the Animator has it, which avoids the warning spam
    void SetFloat(string n, float v) { if (animParams.Contains(n)) anim.SetFloat(n, v); }
    void SetBool(string n, bool v) { if (animParams.Contains(n)) anim.SetBool(n, v); }
    void SetTrigger(string n) { if (animParams.Contains(n)) anim.SetTrigger(n); }

    
    // Draws the ground-check circle (green) in the Scene view when the character is selected
    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}