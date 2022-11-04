using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{

    public GameObject PauseMenu, Pause, Controls, PauseButton;
    private Button PB;

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

    public void Restart()
    {
        ScenesManagement.Instance.RestartButtonPressed();
    }

    public void MM() 
    {
        ScenesManagement.Instance.MainMenuButtonPressed();
    }

    public void tMap() 
    {
        ScenesManagement.Instance.ToMapButtonPressed();
    }
}
