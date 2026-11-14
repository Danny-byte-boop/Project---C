using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class HelpingHand : MonoBehaviour
{
    [Header("Who she helps")]
    [SerializeField] PlayerMovement partner;      // drag Chuck here

    [Header("Boost")]
    [SerializeField] Key helpKey = Key.K;
    [SerializeField] float range = 1.8f;          // how close Chuck must be
    [SerializeField] float boostForce = 16f;      // upward launch speed
    [SerializeField] float cooldown = 1f;

    [Header("Optional visual")]
    [SerializeField] GameObject handVisual;       // a hand sprite child, shown briefly
    [SerializeField] float handShowTime = 0.3f;

    InputAction helpAction;
    Rigidbody2D partnerRb;
    float nextAllowedTime;

    void Awake()
    {
        helpAction = new InputAction("Help", InputActionType.Button);
        helpAction.AddBinding($"<Keyboard>/{helpKey}");
        helpAction.AddBinding("<Gamepad>/buttonNorth");

        if (partner != null) partnerRb = partner.GetComponent<Rigidbody2D>();
        if (handVisual != null) handVisual.SetActive(false);
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

    void OnHelp(InputAction.CallbackContext ctx)
    {
        if (partner == null || partnerRb == null) return;
        if (!partner.PlatformerMode) return;            // only in the platformer
        if (Time.time < nextAllowedTime) return;

        float distance = Vector2.Distance(transform.position, partner.transform.position);
        if (distance > range) return;                   // Chuck is too far away

        nextAllowedTime = Time.time + cooldown;
        partnerRb.linearVelocity = new Vector2(partnerRb.linearVelocity.x, boostForce);

        if (handVisual != null) StartCoroutine(ShowHand());
    }

    IEnumerator ShowHand()
    {
        handVisual.SetActive(true);
        yield return new WaitForSeconds(handShowTime);
        handVisual.SetActive(false);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}