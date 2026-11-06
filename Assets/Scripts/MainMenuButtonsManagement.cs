using UnityEngine;
using UnityEngine.SceneManagement;
public class MainMenuButtonsManagement : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadSceneAsync("Chapter 1, Finding the Calm in the Storm");
    }

    public void BackButton()
    {
        SceneManager.LoadSceneAsync("Main Menu Scene");
    }

    public void GuideButton()
    {
        SceneManager.LoadSceneAsync("Guide Scene");
    }
    public void MainStoryButton()
    {
        SceneManager.LoadSceneAsync("Main Story Scene");
    }
    public void EndCreditsButton()
    {
        SceneManager.LoadSceneAsync("End Credits");
    }
}
