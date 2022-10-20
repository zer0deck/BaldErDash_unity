using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Audio;

public class GameSettings : MonoBehaviour
{

    public GameObject notificationsOn,notificationsOff;
    public GameObject[] musicDots, soundDots;
    public TextMeshProUGUI langText;
    public AudioMixerGroup Mixer;
    private List<string> langs = new List<string> {"English", "Русский", "中文", "Українська", "Deutch"};
    // Start is called before the first frame update
    private void Start()
    {
        if (DataSaver.instance.state.notificationsStats) {
            notificationsOn.SetActive(true);
            notificationsOff.SetActive(false);
        }
        else {
            notificationsOn.SetActive(false);
            notificationsOff.SetActive(true);
        }
        int inx = 0;
        foreach (GameObject musicDot in musicDots) {
            if (inx < DataSaver.instance.state.musicVolume) {
                musicDot.SetActive(true);
            }
            inx++;
        }
        inx = 0;
        foreach (GameObject soundDot in soundDots) {
            if (inx < DataSaver.instance.state.soundVolume) {
                soundDot.SetActive(true);
            }
            inx++;
        }
        langText.text = DataSaver.instance.state.lang;
        Mixer.audioMixer.SetFloat("MusicVol", Mathf.Lerp(-80, 0, DataSaver.instance.state.musicVolume/10f));
        Mixer.audioMixer.SetFloat("EffectsVol", Mathf.Lerp(-80, 0, DataSaver.instance.state.soundVolume/10f));
        
    }

    public void MakeSoundBigger() {
        if (DataSaver.instance.state.soundVolume < 10) {
            DataSaver.instance.state.soundVolume++;
            soundDots[DataSaver.instance.state.soundVolume-1].SetActive(true);
            Mixer.audioMixer.SetFloat("EffectsVol", Mathf.Lerp(-80, 0, DataSaver.instance.state.soundVolume/10f));
        }
        // Debug.Log(string.Format("SV = {0}", DataSaver.instance.state.soundVolume));
    }

    public void MakeSoundSmaller() {
        if (DataSaver.instance.state.soundVolume > 0) {
            DataSaver.instance.state.soundVolume--;
            soundDots[DataSaver.instance.state.soundVolume].SetActive(false);
            Mixer.audioMixer.SetFloat("EffectsVol", Mathf.Lerp(-80, 0, DataSaver.instance.state.soundVolume/10f));
        }
        // Debug.Log(string.Format("SV = {0}", DataSaver.instance.state.soundVolume));
    }

    public void MakeMusicBigger() {
        if (DataSaver.instance.state.musicVolume < 10) {
            DataSaver.instance.state.musicVolume++;
            musicDots[DataSaver.instance.state.musicVolume-1].SetActive(true);
            Mixer.audioMixer.SetFloat("MusicVol", Mathf.Lerp(-80, 0, DataSaver.instance.state.musicVolume/10f));
        }
        // Debug.Log(string.Format("SV = {0}", DataSaver.instance.state.musicVolume));
    }

    public void MakeMusicSmaller() {
        if (DataSaver.instance.state.musicVolume > 0) {
            DataSaver.instance.state.musicVolume--;
            musicDots[DataSaver.instance.state.musicVolume].SetActive(false);
            Mixer.audioMixer.SetFloat("MusicVol", Mathf.Lerp(-80, 0, DataSaver.instance.state.musicVolume/10f));
        }
        // Debug.Log(string.Format("SV = {0}", DataSaver.instance.state.musicVolume));
    }

    public void SwitchNotifications()
    {
        if (DataSaver.instance.state.notificationsStats) {
            notificationsOn.SetActive(false);
            notificationsOff.SetActive(true);
            DataSaver.instance.state.notificationsStats = false;
        }
        else {
            notificationsOn.SetActive(true);
            notificationsOff.SetActive(false);
            DataSaver.instance.state.notificationsStats = true;
        }
    }

    public void SwitchLanguage()
    {
        if (DataSaver.instance.state.lang_inx < langs.Count-1) {
            DataSaver.instance.state.lang_inx++;
        }
        else {
            DataSaver.instance.state.lang_inx = 0;
        }
        DataSaver.instance.state.lang = langs[DataSaver.instance.state.lang_inx];
        langText.text = DataSaver.instance.state.lang;
    }

}
