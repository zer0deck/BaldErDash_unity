using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenesManagement : MonoBehaviour
{

    public static ScenesManagement Instance
    {
        get;
        private set;
    }

    public GameObject PauseMenu, SettingsMenu;

    public void PauseButtonPressed()
    {
        if (PauseMenu.activeInHierarchy)
        {
            PauseMenu.SetActive(false); 
            Time.timeScale = 1f;
        }
        else
        {
            PauseMenu.SetActive(true);
            Time.timeScale = 0f;
        }

    }



    public void SettingsButtonPressed()
    {
        if (SettingsMenu.activeInHierarchy)
        {
            SettingsMenu.SetActive(false);
            Time.timeScale = 1f;
        }
        else
        {
            SettingsMenu.SetActive(true);
            Time.timeScale = 0f;
        }
    }

    public void RestartButtonPressed()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        Time.timeScale = 1f;
    }

    public void MainMenuButtonPressed()
    {
        SceneManager.LoadScene("MainMenu");
        Time.timeScale = 1f;
    }


    public void LoadScene(string sceneName)
    {
        LoadSceneCoroutine(sceneName).Forget();
    }

    private async UniTask LoadSceneCoroutine(string sceneName)
    {
        Scene exitScene = SceneManager.GetActiveScene();
        LoadSceneParameters param = new LoadSceneParameters(LoadSceneMode.Single);
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, param);
        asyncLoad.allowSceneActivation = false;
        while (!asyncLoad.isDone)
        {
            await UniTask.Yield();
        }
        asyncLoad.allowSceneActivation = true;
        await SceneManager.UnloadSceneAsync(exitScene);
    }

}
