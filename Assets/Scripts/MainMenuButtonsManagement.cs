using UnityEngine;
using UnityEngine.SceneManagement;
public class MainMenuButtonsManagement : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadSceneAsync("Chapter 1, Finding the Calm in the Storm");
    }
}
