using UnityEngine;
using UnityEngine.SceneManagement;
public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    private AudioSource audioSource;

    // Assign these in the Inspector
    public AudioClip mainMenuMusic;
    public AudioClip guideMenuMusic;
    public AudioClip endCreditsMenuMusic;
    public AudioClip chapter1Music;
    public AudioClip mainStoryMusic;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>();
        audioSource.loop = true;

        SceneManager.sceneLoaded += OnSceneLoaded;

        // Play music for the starting scene
        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        switch (scene.name)
        {
            case "Main Menu Scene":
                ChangeClip(mainMenuMusic);
                break;

            case "Guide Scene":
                ChangeClip(guideMenuMusic);
                break;

            case "Main Story Scene":
                ChangeClip(mainStoryMusic);
                break;

            case "Chapter 1, Finding the Calm in the Storm":
                ChangeClip(chapter1Music);
                break;

            case "End Credits":
                ChangeClip(endCreditsMenuMusic);
                break;

            default:
                ChangeClip(mainMenuMusic);
                break;
        }
    }

    void ChangeClip(AudioClip clip)
    {
        if (clip != null && audioSource.clip != clip)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }
    }

    void OnDestroy()
    {
        if (instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            instance = null;
        }
    }
}

