using UnityEngine;
using UnityEngine.SceneManagement;
public class MainMenuButtonsManagement : MonoBehaviour
{
    //StartGame() is for when you hit the ready button; it automatically starts the game.
    public void StartGame()
    {
        SceneManager.LoadSceneAsync("Chapter 1, Finding the Calm in the Storm");
    }
    //This instance of BackButton() method brings you back to the Main Menu.
    public void BackButton()
    {
        SceneManager.LoadSceneAsync("Main Menu Scene");
    }
    //GuideButton() brings the player to the Guide Scene.
    public void GuideButton()
    {
        SceneManager.LoadSceneAsync("Guide Scene");
    }
    //MainStoryButton() brings the player to the Main Story Scene
    public void MainStoryButton()
    {
        SceneManager.LoadSceneAsync("Main Story Scene");
    }
    //EndCreditsButton() brings you to the End Credits Scene
    public void EndCreditsButton()
    {
        SceneManager.LoadSceneAsync("End Credits");
    }
}
