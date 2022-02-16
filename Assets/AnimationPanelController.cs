using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Threading.Tasks;

public class AnimationPanelController : MonoBehaviour {
    public bool isChapterStart = false;
    public float animationSpeed = 2.0f;
    public float fadeOutSpeed = 0.5f;

    [Space]
    [Header("Initial fading elements")]
    public GameObject[] backgroundElements;

    [Header("Delay spaced fading elements")]
    public float sequenceDelay = 0.25f;
    public GameObject[] sequenceElements;


    [Header("Delayed group fading elements")]
    public float delay = 2f;
    public GameObject[] delayedElements;

    // Start is called before the first frame update
    void Start()
    {
        //ShowElements();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowElements() {
        this.GetComponent<CanvasGroup>().DOFade(1, 0f);
        FadeInBackground();
        FadeInSequence();
        FadeInDelayed();

    }

    public async Task HideElements()
    {
        await FadeOutSelf();

    }

    public void FadeInBackground()
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

    public void FadeInSequence()
    {
        int index = 1;
        foreach (GameObject element in sequenceElements)
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

    public void FadeInDelayed()
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

    public async Task FadeOutSelf()
    {
       await this.GetComponent<CanvasGroup>().DOFade(0, fadeOutSpeed).AsyncWaitForCompletion();
    }

    public void FadeOutAll()
    {
        foreach (GameObject element in backgroundElements)
        {
            element.GetComponent<CanvasGroup>().DOFade(0, animationSpeed);
        }
        foreach (GameObject element in sequenceElements)
        {
            element.GetComponent<CanvasGroup>().DOFade(0, animationSpeed);
        }
        foreach (GameObject element in delayedElements)
        {
            element.GetComponent<CanvasGroup>().DOFade(0, animationSpeed);
        }
    }
    public void SetAlpha(float opacity)
    {
        this.GetComponent<CanvasGroup>().DOFade(0, 0.0f);
    }
}
