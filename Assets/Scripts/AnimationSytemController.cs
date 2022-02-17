using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Threading.Tasks;
using System.Linq;
using System;
using UnityEngine.Events;
public class AnimationSytemController : MonoBehaviour
{

    [Header("Animated Panels")]
    public AnimationPanelController[] animatedPanels;
    public AnimationPanelController crownIcon;
    public AnimationPanelController navbarUiPanel;
    public AnimationPanelController menuPanel;
    [System.Serializable]
	public class IndexEvent : UnityEvent<String>
	{
	}
	public IndexEvent onIndexChanged;
    private int currentIndex = 0;
    private int currentChapterIndex = 0;

    private bool isMenuOpen = false;

    // Start is called before the first frame update
    async void Start()
    {
        // start faded out for now
        crownIcon.SetAlpha(0);
        navbarUiPanel.SetAlpha(0);
        await animatedPanels[currentIndex].HideElements();
        ShowScreen(currentIndex);
    }

    // Update is called once per frame
    void Update()
    {

    }

    // normally the entry point by the user of the iPad
    public void Step(int delta)
    {
        int targetIndex = (animatedPanels.Length + currentIndex + delta) % animatedPanels.Length;
        // Debug.Log("Step to: " + targetIndex);
        onIndexChanged.Invoke(targetIndex.ToString());
        Debug.Log("Step" + targetIndex.ToString());

        ShowScreen(targetIndex);

    }

public void messageReceived(String message)
    {
        Debug.Log("messageReceived" + message);
        int messageIndex = System.Convert.ToInt32(message);
        // TODO make sure we document the types of data and which functions to use
        Step(messageIndex);
    }

    public void ChapterStep(int delta)
    {
        // find next or previous chapter start
        AnimationPanelController[] chapterPanels = animatedPanels.Where(c => c.isChapterStart).ToArray();
        int targetChapterIndex = (chapterPanels.Length + currentChapterIndex + delta) % chapterPanels.Length;

        // get the panel index for the panel which matches the start chapter panel
        int targetIndex = Array.FindIndex(animatedPanels, p => p == chapterPanels[targetChapterIndex]);

        ShowScreen(targetIndex);
    }

    public async void ShowScreen(int targetIndex)
    {

        var tasks = new List<Task>();
        int _currentChapterIndex = -1; // will turn 0 on first loop run
        AnimationPanelController panelToShow = null;

        OnScreenChange(targetIndex);

        for (int index = 0; index < animatedPanels.Length; index++)
        {
            // set the chapter index
            if (panelToShow && animatedPanels[index].isChapterStart)
            {
                _currentChapterIndex++;
            }

            if (targetIndex == index)
            {
                // found the index to show
                panelToShow = animatedPanels[index];

            } else
            {
                tasks.Add(animatedPanels[index].HideElements());
            }

        }

        if (panelToShow)
        {

        await Task.WhenAll(tasks);

        panelToShow.ShowElements();

        currentIndex = targetIndex;
        currentChapterIndex = _currentChapterIndex;
        }

    }

    private void OnScreenChange(int targetIndex)
    {
        // close menu
        if (isMenuOpen)
        {
            HideMenu();
        }
        // move crown and show navbar when exiting idle screen
        if (currentIndex == 0 && targetIndex != 0)
        {
            TranslateCrownIcon();
            navbarUiPanel.ShowElements();
        }

        // TODO FIX avoid crown show/translate interruption
        // very first screen
        if (targetIndex == 0)
        {
            ShowCrownIcon();
            navbarUiPanel.HideElements();
        }
    }

    public void TranslateCrownIcon(bool backwards = false)
    {
        // init state
        //float initY = crownIcon.transform.localPosition.y; // Y 891
        //float initX = crownIcon.transform.localPosition.x; // X 0
        //crownIcon.GetComponent<CanvasGroup>().DOFade(0, 0f);
        //crownIcon.transform.DOLocalMoveY(initY - 50, 0f);
        //crownIcon.transform.DOLocalMoveY(initX - 50, 0f);

        int factor = backwards ? -1 : 1;
        Vector3 transform = new Vector3(-680 * factor, -100 * factor);

        // to state
        crownIcon.transform.DOLocalMove(transform, 1.5f).SetDelay(0.25f).SetEase(Ease.InOutSine); // X -712
        crownIcon.transform.DOScale(1f, 1.5f).SetDelay(0.5f); // Y 1100
        //crownIcon.GetComponent<CanvasGroup>().DOFade(1, 2f); // not needed
    }


    public async void ShowCrownIcon()
    {
        // fade out slowly
        await crownIcon.GetComponent<CanvasGroup>().DOFade(0, 0.5f).AsyncWaitForCompletion();

        // init state
        Vector3 initPos = new Vector3(0, -260);

        crownIcon.transform.DOLocalMoveX(initPos.x, 0f);
        crownIcon.transform.DOLocalMoveY(initPos.y - 50, 0f);
        crownIcon.transform.DOScale(3.0f, 0f);

        // to state
        crownIcon.transform.DOLocalMove(initPos, 2.0f).SetEase(Ease.OutCubic).SetDelay(0.6f);
        crownIcon.GetComponent<CanvasGroup>().DOFade(1, 2.0f).SetDelay(0.6f); ;
    }

    public async void ShowMenu()
    {
        menuPanel.transform.DOLocalMoveY(-20, 0f);
        menuPanel.GetComponent<CanvasGroup>().DOFade(0, 0f) ;

        // to state
        menuPanel.transform.DOLocalMoveY(0, 0.5f).SetEase(Ease.OutCubic);
        await menuPanel.GetComponent<CanvasGroup>().DOFade(1, 0.5f).AsyncWaitForCompletion();
        isMenuOpen = true;
    }

    public async void HideMenu()
    {
        // fade out slowly
        await menuPanel.HideElements();
        isMenuOpen = false;
    }

    public async void ToggleMenu()
    {
        if (isMenuOpen){
               HideMenu();
         } else {
            ShowMenu();
        }

    }

}
