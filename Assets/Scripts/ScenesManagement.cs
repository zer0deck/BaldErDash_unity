using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenesManagement : MonoBehaviour
{

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
}
