using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Goes on the end platform (alongside a trigger BoxCollider2D placed just above its surface).
/// When both players have been standing inside the trigger for the Hold Time,
/// they freeze and the End Credits scene loads.
/// </summary>
public class EndZone : MonoBehaviour
{
    //This is how many players must be inside the EndZone, so both players have to be in the EndZone in order to bring 
    //up the End Credits Scene
    [SerializeField] int playersRequired = 2;          
    //This is how long both players, BARBARA and Chuck, must stay in the zone
    [SerializeField] float holdTime = 0.5f;              
    //This just has a string for the End Credits Scene
    [SerializeField] string creditsSceneName = "End Credits";   
    //There is a short pause before the scene changes
    [SerializeField] float loadDelay = 0.5f;              

    // The players that are currently inside the EndZone
    readonly HashSet<PlayerMovement> inside = new HashSet<PlayerMovement>();
    // This keeps track of how long players have been inside
    float timer;          
    //This is true once the credits have been started
    bool triggered;       

    // A player's CapsuleCollider2D entered the zone
    void OnTriggerEnter2D(Collider2D other)
    {
        var p = other.GetComponentInParent<PlayerMovement>();
        if (p != null) inside.Add(p);
    }

    // A player's CapsuleCollider2D left the zone
    void OnTriggerExit2D(Collider2D other)
    {
        var p = other.GetComponentInParent<PlayerMovement>();
        if (p != null) inside.Remove(p);
    }

    void Update()
    {
        if (triggered) return;

        if (inside.Count >= playersRequired)
        {
            //All players are here in the EndZone, it counts up, and then starts the credits once they have stayed long enough
            timer += Time.deltaTime;
            if (timer >= holdTime) StartCredits();
        }
        else
        {
            //One player left the EndZone so it starts over
            timer = 0f;    
        }
    }

    // This freezes both players, then loads the credits after a short pause
    void StartCredits()
    {
        triggered = true;

        foreach (var p in FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None))
        {
            var rb = p.GetComponent<Rigidbody2D>();
            //This stops any sliding
            if (rb != null) rb.linearVelocity = Vector2.zero;   
            //No more input is allowed
            p.enabled = false;                                   
        }

        Invoke(nameof(LoadCredits), loadDelay);
    }
    //LoadCredits() loads the End Credits and you beat the game, CONGRATS!
    void LoadCredits()
    {
        SceneManager.LoadScene(creditsSceneName);
    }
}