using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class SnapScrolling : MonoBehaviour
{
    [Header("Other Objects")]
    public GameObject panPrefab;

    [Range(1,50)]
    [Header("Controllers")]
    public const int panCount = 3;

    [Range(1f,20f)] 
    [Header("SnapTime")]
    public float snapSpeed = 6f;

    public GameObject[] pages;
    public GameObject leftButton, rightButton;

    private Animator leftAnim, rightAnim;
    private GameObject[] instPans;
    private Vector2[] pansPos;
    private RectTransform contentRect;
    private Vector2 contentVector;
    private int currentPanID;
    private bool isScrolling, buttonClicked = false;
    private int tempPanID=0;
    private void Start() 
    {
        leftAnim = leftButton.GetComponent<Animator>();
        rightAnim = rightButton.GetComponent<Animator>();
        contentRect = GetComponent<RectTransform>();
        instPans = new GameObject[panCount];
        pansPos = new Vector2[panCount];
        for (int i = 0; i<panCount; i++)
        {
            instPans[i] = Instantiate(panPrefab, transform, false);
            if (i == 0) continue;
            instPans[i].transform.localPosition = new Vector2(instPans[i-1].transform.localPosition.x + 1011f, instPans[i-1].transform.localPosition.y);
            pansPos[i] = -instPans[i].transform.localPosition;
        }
    }

    private void FixedUpdate() 
    {
        if (!buttonClicked) {
            float nearestPos = float.MaxValue;
            for (int i = 0; i<panCount; i++)
            {
                float distance = Mathf.Abs(contentRect.anchoredPosition.x - pansPos[i].x);
                if (distance < nearestPos)
                {
                    nearestPos = distance;
                    currentPanID = i;
                }
            }
            if (isScrolling) return;
            contentVector.x = Mathf.SmoothStep(contentRect.anchoredPosition.x, pansPos[currentPanID].x, snapSpeed * Time.fixedDeltaTime);
            contentRect.anchoredPosition = contentVector;
        }
    }

    public void LeftClick()
    {
        if (!buttonClicked){
            // buttonClicked = true;
            // SwitchPage(-1).Forget();
        }
    }

    public void RightClick()
    {
        if (!buttonClicked){
            // buttonClicked = true;
            // SwitchPage(1).Forget();
        }
    }

    // private async UniTask SwitchPage(int i)
    // {
    //     currentPanID += i;
    //     do 
    //     {
    //         contentVector.x = Mathf.SmoothStep(contentRect.anchoredPosition.x, pansPos[currentPanID].x, snapSpeed * Time.fixedDeltaTime);
    //         contentRect.anchoredPosition = contentVector;
    //     } while (contentVector.x != 0);
    //     buttonClicked = false;
    // }

    private void Update() 
    {
        if (tempPanID == currentPanID) return;

        if (currentPanID == 0) 
        {
            leftButton.SetActive(false);
            rightButton.SetActive(true);
        } else if (currentPanID == panCount-1) 
        {
            leftButton.SetActive(true);
            rightButton.SetActive(false);
        } else
        {
            leftButton.SetActive(true);
            rightButton.SetActive(true);
        }


        for (int i=0; i<panCount; i++)
        {
            if (i == currentPanID) 
            {
                pages[i].SetActive(true);
            }
            else {
                pages[i].SetActive(false);
            }
        }
        tempPanID = currentPanID;
    }

    public void Scrolling(bool scroll)
    {
        isScrolling = scroll;
    }
}
