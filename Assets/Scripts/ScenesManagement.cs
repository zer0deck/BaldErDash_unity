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
    private void Awake() 
    {
        if (Instance == null) {
            Instance = this;
            // DontDestroyOnLoad(this.gameObject);
            return;
        }

        Destroy(this.gameObject);    
    }

    private Animator transitionAnimator;
    public TextMeshProUGUI Loading_info;
    public Image Loading_bar;
    private RectTransform LoadingBar_Rect;
    private bool playerDataSaved = false;
    // Legacy: SettingsMenu, CheckMenuOut, GameOverMenu;

    private void Start() {
        transitionAnimator = this.GetComponent<Animator>();
        LoadingBar_Rect = Loading_bar.GetComponent<RectTransform>();
        LoadingBar_Rect.offsetMax = new Vector2 (0,LoadingBar_Rect.offsetMax[1]);
        Loading_info.text = "Loading completed";
        transitionAnimator.SetTrigger("Break");
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
        await UniTask.Delay(TimeSpan.FromSeconds(1), ignoreTimeScale: true);
        LoadSceneParameters param = new LoadSceneParameters(LoadSceneMode.Single);
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, param);
        asyncLoad.allowSceneActivation = false;
        SavePlayerData();
        await UniTask.WaitUntil(() => playerDataSaved == false);
        
        float right = 1200;
        do {
            right = (1- asyncLoad.progress)*right;
            LoadingBar_Rect.offsetMax = new Vector2 (-right,LoadingBar_Rect.offsetMax[1]);
            Loading_info.text = $"Loading... {Convert.ToInt16(100-right/12)}%";
            await UniTask.Yield();
        } while (asyncLoad.progress < 0.89);
        await UniTask.Delay(TimeSpan.FromSeconds(0.5f), ignoreTimeScale: true);
        LoadingBar_Rect.offsetMax = new Vector2 (0,LoadingBar_Rect.offsetMax[1]);
        Loading_info.text = "Loading completed";
        await UniTask.Delay(TimeSpan.FromSeconds(0.5f), ignoreTimeScale: true);
        // await UniTask.WaitUntil(() => transitionFinished==true);
        asyncLoad.allowSceneActivation = true;
        // await SceneManager.UnloadSceneAsync(exitScene);
        transitionAnimator.SetTrigger("Break");
    }
    public void Quit() {
        Application.Quit();
    }

    /* Saving Manager */
    public void SavePlayerData() {
        SavePlayerDataCoroutine().Forget();
    }
    private async UniTask SavePlayerDataCoroutine() {
        DataSaver.instance.Save();
        await UniTask.DelayFrame(1);
        playerDataSaved = true;
        playerDataSaved = false;
    }

}
