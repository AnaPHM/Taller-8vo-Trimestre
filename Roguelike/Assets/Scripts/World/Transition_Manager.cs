using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class Transition_Manager : MonoBehaviour
{
    private bool showTurotial;
    public GameObject tutorial;

    private void Awake()
    {
        showTurotial = false;

        if(tutorial != null)
            tutorial.SetActive(false);
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadSceneAsync("MainMenu");
    }

    public void LoadGameover()
    {
        SceneManager.LoadSceneAsync("GameOver");
    }

    public void LoadGame()
    {
        SceneManager.LoadSceneAsync("GamePlay");
        Time.timeScale = 1.0f;
    }

    public void Turotial()
    {
        if (tutorial == null) return;

        showTurotial = !showTurotial;

        tutorial.SetActive(showTurotial);
    }

    public void Victory()
    {
        SceneManager.LoadSceneAsync("Vicroty");
    }

    public void Quit()
    {
        Application.Quit();
        Debug.Log("Stop!");
    }
}
