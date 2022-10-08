using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System;

public class ScenesManagement : MonoBehaviour
{

    public static ScenesManagement Instance
    {
        get;
        private set;
    }

    public GameObject PauseMenu, Pause, Controls, PauseButton, Transitor;
    private Button PB;
    private Animator transitionAnimator;
    public TextMeshProUGUI Loading_info;
    public Image Loading_bar;
    private RectTransform LoadingBar_Rect;
    private bool playerDataSaved = false;
    // Legacy: SettingsMenu, CheckMenuOut, GameOverMenu;

    private void Start() {
        PB = PauseButton.GetComponent<Button>();
        transitionAnimator = Transitor.GetComponent<Animator>();
        LoadingBar_Rect = Loading_bar.GetComponent<RectTransform>();
        LoadingBar_Rect.offsetMax = new Vector2 (0,LoadingBar_Rect.offsetMax[1]);
        Loading_info.text = "Loading completed";
        transitionAnimator.SetTrigger("Break");
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
        Time.timeScale = 1f;
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
        LoadingBar_Rect.offsetMax = new Vector2 (-1200,LoadingBar_Rect.offsetMax[1]);
        Loading_info.text = "Loading... 10%";
        transitionAnimator.SetTrigger("Use");
        LoadSceneParameters param = new LoadSceneParameters(LoadSceneMode.Single);
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, param);
        asyncLoad.allowSceneActivation = false;
        SavePlayerData();
        await UniTask.WaitUntil(() => playerDataSaved == false);
        
        float right = 1200;
        do {
            Loading_info.text = $"Loading... {asyncLoad.progress*100}%";
            right = Mathf.Lerp(right, asyncLoad.progress, Time.deltaTime * 2);
            LoadingBar_Rect.offsetMax = new Vector2 (-right,LoadingBar_Rect.offsetMax[1]);
            await UniTask.Yield();
        } while (asyncLoad.progress < 0.89);
        LoadingBar_Rect.offsetMax = new Vector2 (0,LoadingBar_Rect.offsetMax[1]);
        Loading_info.text = "Loading completed";
        await UniTask.Delay(TimeSpan.FromSeconds(1), ignoreTimeScale: true);
        // await UniTask.WaitUntil(() => transitionFinished==true);
        asyncLoad.allowSceneActivation = true;
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
        playerDataSaved = true;
        playerDataSaved = false;
    }

}
