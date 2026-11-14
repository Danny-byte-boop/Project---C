using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Runs the whole game flow:
///  1. Intro dialogue, then the timed coconut trial begins.
///  2. If the team collects enough coconuts in time, the platformer section starts
///     (with its own dialogue). If time runs out, the death screen appears.
///  3. In the platformer, falling below the Fall Limit shows the death screen, and if one player dies, Chuck or BARBARA
///  you both have to restart the level.
/// Other scripts reach it through TrialManager.Instance.
/// </summary>
public class TrialManager : MonoBehaviour
{
    // A shared reference so other scripts (Coconut pickups, PlayerCarry) can find the manager
    public static TrialManager Instance { get; private set; }

    [Header("Trial")]
    //This is the number of seconds allowed for the timed trial, obviously changeable within the Trial Manager Script
    [SerializeField] float timeLimit = 60f;    
    //The # of coconuts that are needed to win
    [SerializeField] int targetScore = 10;        

    [Header("UI")]
    //This shows the # of seconds left in the timed trial
    [SerializeField] TMP_Text timerText;          
    //This shows the score / target mainly this is so that if they fail the timed trial, it shows how many coconuts they 
    //got out of the targeted goal.
    [SerializeField] TMP_Text scoreText;          
    //This shows the "You did it!" when you complete the timed trial, transitioning onto the platform scene.
    [SerializeField] TMP_Text resultText;         

    [Header("Dialogue")]
    //This dialogue plays before the trial starts.
    [SerializeField] DialogueLine[] introLines;        
    //This dialogue plays when the platformer starts, and you have beaten the timed trial.
    [SerializeField] DialogueLine[] platformerLines;   

    [Header("Death screen")]
    //This is a panel with a message and a Restart button
    [SerializeField] GameObject deathScreen;      
    //This is the text that says why the player died, either "That's EMBARASSING", or "You fell off the platform!"
    [SerializeField] TMP_Text deathMessage;      
    //This is a empty object just below the platformer so that if either Chuck or BARBARA fall off, the death screen appears.
    [SerializeField] Transform fallLimit;        

    [Header("Players")]
    //Chuck is always first, and then Barbara
    [SerializeField] PlayerMovement[] players;    

    [Header("Platformer transition")]
    // coconuts, baskets, trial texts: hidden when the platformer starts so that it's a seamless transition and you don't
    //see any objects from the timed trial appear
    [SerializeField] GameObject[] hideOnComplete;
    // PlatformerRoot: shown when the dialogue is concluded, and the players are transported to the platformer1 level
    [SerializeField] GameObject[] showOnComplete;   
    //This is where both BARBARA and Chuck start on the platform
    [SerializeField] Transform[] spawnPoints;
    // A slight pause of 2 seconds, so "You did it!" can be read
    [SerializeField] float nextLevelDelay = 2f;
    // This is the seconds remaining in the Timed trial
    float timeLeft;          
    //This is your coconuts delivered to the baskets so far
    int score;                
    //This is true once the platformer has started.
    bool platformerActive;    
    //This is true once the death screen has shown on the players' screen
    bool isDead;             

    // This is true only while the trial is being played (not during dialogue or after it ends)
    public bool IsRunning { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // This unfreezes time in case the last run ended on the death screen
        Time.timeScale = 1f;               
        timeLeft = timeLimit;
        score = 0;
        isDead = false;
        platformerActive = false;
        // the clock waits for the dialogue to conclude
        IsRunning = false;                 

        if (resultText != null) resultText.gameObject.SetActive(false);
        if (deathScreen != null) deathScreen.SetActive(false);
        // Both players, BARBARA and Chuck, stay frozen while the dialogue plays
        SetPlayersEnabled(false);          
        UpdateUI();

        // Play the intro dialogue, and then start the game.
        if (DialogueManager.Instance != null && introLines != null && introLines.Length > 0)
            DialogueManager.Instance.Play(Fill(introLines), BeginTrial);
        else
            BeginTrial();
    }

    // This is called when the intro dialogue finishes: it starts the clock and lets the players move
    void BeginTrial()
    {
        IsRunning = true;
        SetPlayersEnabled(true);
    }

    void Update()
    {
        // In the platformer, keep checking whether anyone has fallen off of the platform
        if (platformerActive && !isDead) CheckFall();

        if (!IsRunning) return;

        // This counts the timer down: when the timer runs out, you have lost
        timeLeft -= Time.deltaTime;
        if (timeLeft <= 0f)
        {
            timeLeft = 0f;
            EndTrial();
        }

        UpdateUI();
    }

    // This is caled by PlayerCarry when a player delivers coconuts to the basket
    public void AddScore(int amount)
    {
        if (!IsRunning) return;

        score += amount;
        UpdateUI();

        if (score >= targetScore) EndTrial();
    }

    //This ends the trial: a win starts the platformer level, and a loss shows the death screen
    void EndTrial()
    {
        // This stops this running twice if time and score hit together
        if (!IsRunning) return;    
        IsRunning = false;

        SetPlayersEnabled(false);

        bool won = score >= targetScore;

        if (won)
        {
            if (resultText != null)
            {
                resultText.gameObject.SetActive(true);
                resultText.text = "You did it!";
            }
            //There's a short pause before moving on, giving the players time to catch their breath and prepare
            Invoke(nameof(StartPlatformer), nextLevelDelay);   
        }
        else
        {
            Die($"Time's up! {score}/{targetScore} coconuts");
        }
    }

    // This just swaps the trial for the platformer, all inside the same scene
    void StartPlatformer()
    {
        foreach (var go in hideOnComplete)
            if (go != null) go.SetActive(false);

        foreach (var go in showOnComplete)
            if (go != null) go.SetActive(true);

        if (resultText != null) resultText.gameObject.SetActive(false);

        for (int i = 0; i < players.Length; i++)
        {
            // Move each player to their spawn point, but keep their own Z so they stay in front of the platform art
            if (i < spawnPoints.Length && spawnPoints[i] != null)
            {
                Vector3 pos = spawnPoints[i].position;
                pos.z = players[i].transform.position.z;
                players[i].transform.position = pos;
            }
            // This switch on gravity and real jumping; which is still frozen until dialogue ends
            players[i].SetPlatformerMode(true);   
        }

        platformerActive = true;

        // This plays the platformer dialogue, then hands control back to the players
        if (DialogueManager.Instance != null && platformerLines != null && platformerLines.Length > 0)
            DialogueManager.Instance.Play(Fill(platformerLines), () => SetPlayersEnabled(true));
        else
            SetPlayersEnabled(true);
    }

    // This shows the death screen if any player drops below the Fall Limit, either BARBARA or Chuck
    void CheckFall()
    {
        if (fallLimit == null) return;

        foreach (var p in players)
        {
            if (p.transform.position.y < fallLimit.position.y)
            {
                Die("You fell off the platform!");
                return;
            }
        }
    }

    // This shows the death screen with a message and freezes the game for both players
    void Die(string message)
    {
        if (isDead) return;
        isDead = true;
        IsRunning = false;
        // This cancels a pending platformer start, if any are applicable
        CancelInvoke();    

        if (resultText != null) resultText.gameObject.SetActive(false);
        if (deathMessage != null) deathMessage.text = message;
        if (deathScreen != null) deathScreen.SetActive(true);
        // This freezes everything behind the screen
        Time.timeScale = 0f;    
    }

    // This directly coincides with the Restart button, and reloads the scene from the beginning
    public void Restart()
    {
        //This unfreezes before reloading the game
        Time.timeScale = 1f;    
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // This turns both players' movement scripts on or off together
    void SetPlayersEnabled(bool on)
    {
        foreach (var p in players) p.enabled = on;
    }

    // This replaces target and time in the dialogue text with the real trial values
    DialogueLine[] Fill(DialogueLine[] lines)
    {
        var result = new DialogueLine[lines.Length];
        for (int i = 0; i < lines.Length; i++)
        {
            result[i] = new DialogueLine
            {
                speaker = lines[i].speaker,
                text = lines[i].text
                    .Replace("{target}", targetScore.ToString())
                    .Replace("{time}", Mathf.CeilToInt(timeLimit).ToString())
            };
        }
        return result;
    }

    // This refreshes the timer and score on the screen
    void UpdateUI()
    {
        if (timerText != null) timerText.text = Mathf.CeilToInt(timeLeft).ToString();
        if (scoreText != null) scoreText.text = $"{score} / {targetScore}";
    }
}