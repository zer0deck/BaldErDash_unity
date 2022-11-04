using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioInit : MonoBehaviour
{

    public static AudioInit instance { get; private set;}

    private void Start() 
    {
        if (instance == null) {
            instance = this;
            // DontDestroyOnLoad(this.gameObject);
            SetAudio();
            return;
        }

        Destroy(this.gameObject);    
    }
    public AudioMixerGroup Mixer;

    void SetAudio()
    {
        Mixer.audioMixer.SetFloat("MusicVol", Mathf.Lerp(-80, 0, DataSaver.instance.state.musicVolume/10f));
        Mixer.audioMixer.SetFloat("EffectsVol", Mathf.Lerp(-80, 0, DataSaver.instance.state.soundVolume/10f));
    }
}
