using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataSaver : MonoBehaviour
{
    public SaveState state;
    public static DataSaver instance { get; private set;}

    private void Awake() 
    {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
            Load();
            return;
        }

        Destroy(this.gameObject);    
    }

    public void Save ()
    {
        PlayerPrefs.SetString("save",decoder.encode<SaveState>(state));
    }

    public void Load()
    {
        if (PlayerPrefs.HasKey("save"))
        {
            state = decoder.decode<SaveState>(PlayerPrefs.GetString("save"));
        }
        else {
            state = new SaveState();
            Save();

        }
    }

}
