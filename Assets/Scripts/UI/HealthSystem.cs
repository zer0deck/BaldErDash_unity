using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthSystem : MonoBehaviour
{

    [SerializeField] private GameObject _linebar;
    private RectTransform bar;
    public static HealthSystem instance { get; private set;}

    private void Start() 
    {
        bar = _linebar.GetComponent<RectTransform>();
        if (instance == null) {
            instance = this;
            // DontDestroyOnLoad(this.gameObject);
            return;
        }
        Destroy(this.gameObject);
    }

    private const float SHIFT = -232;
    public void SetValue(float value)
    {
        if (value < 0) value = 0f;
        if (value > 1) value = 1;
        bar.offsetMax = new Vector2( SHIFT * (1 - value), bar.offsetMax[1] );
    }

}