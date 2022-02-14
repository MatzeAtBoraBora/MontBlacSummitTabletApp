using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class AnimationPanelController : MonoBehaviour {
    public GameObject[] backgroundElements;
    public GameObject[] typoElements;
    public float sequenceDelay = 0.5f;
    public float animationSpeed = 2.0f;
    public float fadeOutSpeed = 0.5f;

    public GameObject[] delayedElements;
    public float delay = 2f;

    // Start is called before the first frame update
    void Start()
    {
        //ShowElements();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void showElements() {
        this.GetComponent<CanvasGroup>().DOFade(1, 0f);
        fadeInBackground();
        fadeInTypo();
        fadeInDelayed();

    }

    public void hideElements()
    {
        fadeOutSelf();
    }

    public void fadeInBackground()
    {
        foreach (GameObject element in backgroundElements)
        {
            // init state
            float initY = element.transform.localPosition.y;
            element.GetComponent<CanvasGroup>().DOFade(0, 0f);
            element.transform.DOLocalMoveY(initY-50, 0f);

            // to state
            element.transform.DOLocalMoveY(initY, animationSpeed).SetEase(Ease.OutCubic);
            element.GetComponent<CanvasGroup>().DOFade(1, animationSpeed);
        }
    }

    public void fadeInTypo()
    {
        int index = 1;
        foreach (GameObject element in typoElements)
        {
            // init state
            float initY = element.transform.localPosition.y;
            element.GetComponent<CanvasGroup>().DOFade(0, 0f);
            element.transform.DOLocalMoveY(initY - 50, 0f);

            // to state
            element.transform.DOLocalMoveY(initY, animationSpeed).SetEase(Ease.OutCubic).SetDelay(index * sequenceDelay);
            element.GetComponent<CanvasGroup>().DOFade(1, animationSpeed).SetDelay(index * sequenceDelay);
            index++;
        }
    }

    public void fadeInDelayed()
    {
        foreach (GameObject element in delayedElements)
        {
            // init state
            float initY = element.transform.localPosition.y;
            element.GetComponent<CanvasGroup>().DOFade(0, 0f);
            element.transform.DOLocalMoveY(initY - 50, 0f);

            // to state
            element.transform.DOLocalMoveY(initY, animationSpeed).SetEase(Ease.OutCubic).SetDelay(delay); ;
            element.GetComponent<CanvasGroup>().DOFade(1, animationSpeed).SetDelay(delay); ;

        }
    }

    public void fadeOutSelf()
    {
        this.GetComponent<CanvasGroup>().DOFade(0, fadeOutSpeed);
    }

    public void fadeOutAll()
    {
        foreach (GameObject element in backgroundElements)
        {
            element.GetComponent<CanvasGroup>().DOFade(0, animationSpeed);
        }
        foreach (GameObject element in typoElements)
        {
            element.GetComponent<CanvasGroup>().DOFade(0, animationSpeed);
        }
        foreach (GameObject element in delayedElements)
        {
            element.GetComponent<CanvasGroup>().DOFade(0, animationSpeed);
        }
    }
}
