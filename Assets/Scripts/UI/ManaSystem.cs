using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class ManaSystem : MonoBehaviour
{
 
    [SerializeField] private GameObject _linebar;
    private RectTransform bar;
    public static ManaSystem instance { get; private set;}

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

    private const float SHIFT = 185;
    public void SetValue(float value)
    {
        if (value < 0) value = 0f;
        if (value > 1) value = 1;
        ChangeMana(value).Forget();
    }
    
    private async UniTask ChangeMana(float value)
    {   
        while (bar.offsetMax[0] > (- ( SHIFT * (1 - value) )))
        {
            bar.offsetMax = new Vector2( Mathf.Lerp(bar.offsetMax[0], - ( SHIFT * (1 - value) ), 3f*Time.deltaTime), bar.offsetMax[1] );
            await UniTask.NextFrame();
        }   

    }           

}   
