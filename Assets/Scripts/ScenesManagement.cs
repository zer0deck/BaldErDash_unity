using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ScenesManagement : MonoBehaviour
{

    public static ScenesManagement Instance
    {
        get;
        private set;
    }

    public GameObject PauseMenu, Pause, Controls, PauseButton;
    private Button PB;
    // Legacy: SettingsMenu, CheckMenuOut, GameOverMenu;

    private void Start() {
        PB = PauseButton.GetComponent<Button>();
    }
    public void PauseButtonPressed()
    {
        if (PauseMenu.activeInHierarchy)
        {
            PB.interactable = true;
            Controls.SetActive(true);
            Pause.SetActive(false);
            PauseMenu.SetActive(false); 
            Time.timeScale = 1f;
        }
        else
        {
            PB.interactable = false;
            Controls.SetActive(false);
            Pause.SetActive(true);
            PauseMenu.SetActive(true);
            Time.timeScale = 0f;
        }
    }
    public void ContinueButtonPressed()
    {
        PB.interactable = true;
        Controls.SetActive(true);
        Pause.SetActive(false);
        Time.timeScale = 1f;
    }

    // Legacy
    // public void SettingsButtonPressed()
    // {
    //     if (!SettingsMenu.activeInHierarchy)
    //     {
    //         SettingsMenu.SetActive(false);
    //         Time.timeScale = 1f;
    //     }
    //     else
    //     {
    //         SettingsMenu.SetActive(true);
    //         Time.timeScale = 0f;
    //     }
    // }
    

    /* InMenuButtons */

    public void RestartButtonPressed()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        Time.timeScale = 1f;
    }

    public void MainMenuButtonPressed()
    {
        LoadScene("MainMenu");
    }
    public void ToMapButtonPressed()
    {
        LoadScene("Map");
    }

    /* SceneLoader */
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
        await SavePlayerDataCoroutine();
        asyncLoad.allowSceneActivation = true;
        while (!asyncLoad.isDone || asyncLoad.progress < 0.9f)
        {
            await UniTask.Yield();
        }
        await SceneManager.UnloadSceneAsync(exitScene);
    }
    public void Quit() {
        Application.Quit();
    }

    /* Saving Manager */
    public void SavePlayerData() {
        SavePlayerDataCoroutine().Forget();
    }
    private async UniTask SavePlayerDataCoroutine() {
        Debug.Log("Saving player data");
        DataSaver.instance.Save();
        await UniTask.DelayFrame(1);
        Debug.Log("Player data saved");
    }

}
