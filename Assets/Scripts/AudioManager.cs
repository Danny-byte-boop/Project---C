using UnityEngine;
using UnityEngine.SceneManagement;
/// <summary>
/// This AudioManager script allows for main menu music to be played, along with the game music, and the end credits music.
/// It allows for seamless transitions between the different songs as you navigate through the game and different menus
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    //Instantiates a private AudioSource and assigns it the variable, audioSource
    private AudioSource audioSource;

    // You would assign each different track in the Inspector for this script, so main menu music, guide menu music,
    //end credits menu music, chapter1Music, and mainStoryMusic
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

        // Plays music for the starting scene
        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }
    //Switches the music based on what scene the player is in. So a case by case basis for all the different menus in STRANDED TOGETHER
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
    //This simply changes the audioClip depending on the scene the player is in. Some music is the same, and there are really
    //only a three different songs played throughout STRANDED TOGETHER.
    void ChangeClip(AudioClip clip)
    {
        if (clip != null && audioSource.clip != clip)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }
    }
    //OnDestroy() makes it so the audioClip gets "destroyed" or stops playing entirely so that the new one can play.
    //This ensures seamless transitions between the different scenes in STRANDED TOGETHER.
    void OnDestroy()
    {
        if (instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            instance = null;
        }
    }
}

