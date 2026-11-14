using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class TrialManager : MonoBehaviour
{
    public static TrialManager Instance { get; private set; }

    [Header("Trial")]
    [SerializeField] float timeLimit = 60f;
    [SerializeField] int targetScore = 10;

    [Header("UI")]
    [SerializeField] TMP_Text timerText;
    [SerializeField] TMP_Text scoreText;
    [SerializeField] TMP_Text resultText;

    [Header("Death screen")]
    [SerializeField] GameObject deathScreen;        // panel with a message and a Restart button
    [SerializeField] TMP_Text deathMessage;
    [SerializeField] Transform fallLimit;           // empty object below the platformer; falling under its Y = death

    [Header("Players")]
    [SerializeField] PlayerMovement[] players;      // Chuck first, then Barbara

    [Header("Platformer transition")]
    [SerializeField] GameObject[] hideOnComplete;   // coconuts, baskets, timer/score/result texts
    [SerializeField] GameObject[] showOnComplete;   // PlatformerRoot
    [SerializeField] Transform[] spawnPoints;       // same order as Players; leave empty to keep positions
    [SerializeField] float nextLevelDelay = 2f;

    float timeLeft;
    int score;
    bool platformerActive;
    bool isDead;

    public bool IsRunning { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (timerText == null) Debug.LogError("TrialManager: Timer Text isn't assigned.");
        if (scoreText == null) Debug.LogError("TrialManager: Score Text isn't assigned.");

        Time.timeScale = 1f;    // in case the last run ended on the death screen
        timeLeft = timeLimit;
        score = 0;
        isDead = false;
        platformerActive = false;
        IsRunning = true;

        if (resultText != null) resultText.gameObject.SetActive(false);
        if (deathScreen != null) deathScreen.SetActive(false);

        UpdateUI();
    }

    void Update()
    {
        if (platformerActive && !isDead) CheckFall();

        if (!IsRunning) return;

        timeLeft -= Time.deltaTime;
        if (timeLeft <= 0f)
        {
            timeLeft = 0f;
            EndTrial();
        }

        UpdateUI();
    }

    public void AddScore(int amount)
    {
        if (!IsRunning) return;

        score += amount;
        UpdateUI();

        if (score >= targetScore) EndTrial();
    }

    void EndTrial()
    {
        if (!IsRunning) return;   // prevents running twice if time and score hit together
        IsRunning = false;

        foreach (var p in players) p.enabled = false;

        bool won = score >= targetScore;

        if (won)
        {
            if (resultText != null)
            {
                resultText.gameObject.SetActive(true);
                resultText.text = "You did it!";
            }
            Invoke(nameof(StartPlatformer), nextLevelDelay);
        }
        else
        {
            Die($"Time's up! {score}/{targetScore} coconuts");
        }
    }

    void StartPlatformer()
    {
        foreach (var go in hideOnComplete)
            if (go != null) go.SetActive(false);

        foreach (var go in showOnComplete)
            if (go != null) go.SetActive(true);

        if (resultText != null) resultText.gameObject.SetActive(false);

        for (int i = 0; i < players.Length; i++)
        {
            if (i < spawnPoints.Length && spawnPoints[i] != null)
            {
                Vector3 pos = spawnPoints[i].position;
                pos.z = players[i].transform.position.z;   // keep the character's own depth
                players[i].transform.position = pos;
            }

            players[i].SetPlatformerMode(true);
            players[i].enabled = true;
        }

        platformerActive = true;
    }

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

    void Die(string message)
    {
        if (isDead) return;
        isDead = true;
        IsRunning = false;
        CancelInvoke();

        if (resultText != null) resultText.gameObject.SetActive(false);
        if (deathMessage != null) deathMessage.text = message;
        if (deathScreen != null) deathScreen.SetActive(true);

        Time.timeScale = 0f;    // freeze everything behind the screen
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void UpdateUI()
    {
        if (timerText != null) timerText.text = Mathf.CeilToInt(timeLeft).ToString();
        if (scoreText != null) scoreText.text = $"{score} / {targetScore}";
    }
}