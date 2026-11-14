using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerCarry : MonoBehaviour
{
    [Header("Carrying")]
    [SerializeField] int maxCarry = 1;

    [Header("Interaction Range")]
    [SerializeField] float interactRadius = 1.2f;
    [SerializeField] Vector2 interactOffset = Vector2.zero;

    InputAction interactAction;

    public int Carried { get; private set; }

    Vector2 InteractCenter => (Vector2)transform.position + interactOffset;

    void Awake()
    {
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

    void OnInteract(InputAction.CallbackContext ctx)
    {
        var trial = TrialManager.Instance;
        if (trial == null)
        {
            Debug.LogWarning("PlayerCarry: no TrialManager in the scene.");
            return;
        }
        if (!trial.IsRunning) return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(InteractCenter, interactRadius);

        if (Carried > 0 && TryDeliver(hits, trial)) return;
        if (Carried < maxCarry) TryPickup(hits);
    }

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
        closest.gameObject.SetActive(false);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(InteractCenter, interactRadius);
    }
}