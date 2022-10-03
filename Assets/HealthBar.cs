using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBar : MonoBehaviour
{
    private float dmg = 0.9f;

    [SerializeField] private GameObject _linebar;

    private RectTransform bar;

    private const float SHIFT = 232;
    public void SetValue(float value)
    {
        if (value < 0) value = 0f;
        if (value > 1) value = 1;
        bar.offsetMax = new Vector2( - ( SHIFT * (1 - value) ), bar.offsetMax [1] );
    }
    
    private void Start()
    {

        bar = _linebar.GetComponent<RectTransform>();

    }

    private void Update()
    {
        if (Input.GetKeyDown("space"))
        {
            SetValue(dmg);
            dmg = dmg - 0.1f;
            
        }
    }
}
 