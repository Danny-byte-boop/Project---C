using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Optional co-op ability. Goes on Barbara. While in the platformer, pressing the help key
/// near Chuck launches him upward, higher than his normal jump, so he can reach ledges
/// he couldn't reach alone.
/// </summary>
public class HelpingHand : MonoBehaviour
{
    [Header("Who she helps")]
    //You drag Chuck here
    [SerializeField] PlayerMovement partner;      

    [Header("Boost")]
    // the keyboard key that gives the helping hand boost
    [SerializeField] Key helpKey = Key.K;
    // how close Chuck must be to BARBARA
    [SerializeField] float range = 1.8f;          
    //This makes an upward launch speed where the key being pressed by BARBARA gives Chuck a boost
    [SerializeField] float boostForce = 16f;      
    //This is so it can't be spammed so you have to wait seconds before the ability can be used again
    [SerializeField] float cooldown = 1f;         
    //This is what the helping key is
    InputAction helpAction;
    // the partner's physics body, so we can launch Chuck
    Rigidbody2D partnerRb;
    //This is the earliest time the boost can be used again
    float nextAllowedTime;        

    void Awake()
    {
        // The help key, plus the gamepad North button
        helpAction = new InputAction("Help", InputActionType.Button);
        helpAction.AddBinding($"<Keyboard>/{helpKey}");
        helpAction.AddBinding("<Gamepad>/buttonNorth");

        if (partner != null) partnerRb = partner.GetComponent<Rigidbody2D>();
       
    }

    void OnEnable()
    {
        helpAction.Enable();
        helpAction.performed += OnHelp;
    }

    void OnDisable()
    {
        helpAction.performed -= OnHelp;
        helpAction.Disable();
    }

    void OnDestroy()
    {
        helpAction?.Dispose();
    }

    // This runs when the help key is pressed
    void OnHelp(InputAction.CallbackContext ctx)
    {
        if (partner == null || partnerRb == null) return;
        //This only works within the platformer
        if (!partner.PlatformerMode) return;
        // this means it is still cooling down
        if (Time.time < nextAllowedTime) return;         

        // Chuck must be close enough to BARBARA
        float distance = Vector2.Distance(transform.position, partner.transform.position);
        if (distance > range) return;

        nextAllowedTime = Time.time + cooldown;

        // Launches Chuck upward, keeping their sideways speed
        partnerRb.linearVelocity = new Vector2(partnerRb.linearVelocity.x, boostForce);
    }

    

    // Draws the help range (cyan) in the Scene view when selected
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}