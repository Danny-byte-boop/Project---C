using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Lets a character pick up coconuts and drop them in the basket during the trial.
/// The player presses an interact key (Enter, E, or the gamepad West button, matching
/// their control scheme) and the script checks what is within reach: So for Chuck 
/// it's the E key to pick up cocounuts and put in the basket, and for BARBARA it's the Enter key.
///  - carrying a coconut and a basket is near  -> deliver it and add to the score on the top
///  - otherwise, with room to carry maximum of one carry          -> pick up the nearest coconut
/// </summary>
[RequireComponent(typeof(PlayerMovement))]
public class PlayerCarry : MonoBehaviour
{
    [Header("Carrying")]
    //maximum of one coconut per each player to carry into the basket
    [SerializeField] int maxCarry = 1;                 

    [Header("Interaction Range")]
    //How far either BARBARA or Chuck can reach to put the coconut in the basket
    [SerializeField] float interactRadius = 1.2f;    
    //This shifts the reach circle so for example, up from the feet to the rest of the body.
    [SerializeField] Vector2 interactOffset = Vector2.zero;   
    //This is just the interact key.
    InputAction interactAction;                        
    //How many coconuts the players are currently carrying.
    public int Carried { get; private set; }           

    // This would be the middle of the reach circle: the character's position plus the offset
    Vector2 InteractCenter => (Vector2)transform.position + interactOffset;

    void Awake()
    {
        // Creates the interact key, and matches the character's control scheme
        interactAction = new InputAction("Interact", InputActionType.Button);

        switch (GetComponent<PlayerMovement>().Scheme)
        {
            case ControlScheme.Arrows:
                interactAction.AddBinding("<Keyboard>/enter");
                break;
            case ControlScheme.WASD:
                interactAction.AddBinding("<Keyboard>/e");
                break;
            case ControlScheme.Gamepad:
                interactAction.AddBinding("<Gamepad>/buttonWest");
                break;
        }
    }

    void OnEnable()
    {
        //This runs OnInteract when the key is pressed
        interactAction.Enable();
        interactAction.performed += OnInteract;    
    }

    void OnDisable()
    {
        interactAction.performed -= OnInteract;
        interactAction.Disable();
    }

    void OnDestroy()
    {
        interactAction?.Dispose();
    }

    // This runs each time the interact key is pressed
    void OnInteract(InputAction.CallbackContext ctx)
    {
        var trial = TrialManager.Instance;
        if (trial == null)
        {
            Debug.LogWarning("PlayerCarry: no TrialManager in the scene.");
            return;
        }
        //This only works while the trial is being played.
        if (!trial.IsRunning) return;  

        // This finds every collider inside the reach circle.
        Collider2D[] hits = Physics2D.OverlapCircleAll(InteractCenter, interactRadius);

        // This makes it so that delivering the coconut takes priority if the player is carrying something
        if (Carried > 0 && TryDeliver(hits, trial)) return;

        // Otherwise try to pick something up
        if (Carried < maxCarry) TryPickup(hits);
    }

    // If a basket is in reach, hand over everything carried and add it to the score. This allows for the 
    //score to go up by +1.
    bool TryDeliver(Collider2D[] hits, TrialManager trial)
    {
        foreach (var hit in hits)
        {
            if (hit.GetComponentInParent<Basket>() == null) continue;

            trial.AddScore(Carried);
            Carried = 0;
            return true;
        }
        return false;
    }

    // This picks up the nearest coconut within reach.
    void TryPickup(Collider2D[] hits)
    {
        Coconut closest = null;
        float bestDistance = float.MaxValue;
        Vector2 center = InteractCenter;

        foreach (var hit in hits)
        {
            var coconut = hit.GetComponentInParent<Coconut>();
            if (coconut == null || !coconut.gameObject.activeInHierarchy) continue;

            float distance = ((Vector2)coconut.transform.position - center).sqrMagnitude;
            if (distance < bestDistance)
            {
                bestDistance = distance;
                closest = coconut;
            }
        }

        if (closest == null) return;

        Carried++;
        //This hides the coconut once it's picked up, kind of like it's in a imaginary "inventory."
        closest.gameObject.SetActive(false);   
    }

    // Draws the reach circle (yellow) in the Scene view when the character is selected. This helps for Debugging.
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(InteractCenter, interactRadius);
    }
}