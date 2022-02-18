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
    private int currentIndex = -1;
    private int currentChapterIndex = 0;

    private bool isMenuOpen = false;
    private bool isNavbarShown = false;

    // Start is called before the first frame update
    async void Start()
    {
        // start faded out for now
        crownIcon.HideElements();
        navbarUiPanel.HideElements();
        HideMenu();
        await animatedPanels[0].HideElements();
        ShowScreen(0);
    }

    // Update is called once per frame
    void Update()
    {

    }

    // normally the entry point by the user of the iPad
    public void Step(int delta)
    {
        int targetIndex = (animatedPanels.Length + currentIndex + delta) % animatedPanels.Length;
        ShowScreen(targetIndex);

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

    public void ShowScreenByIndex(int index) {
        ShowScreen(index);
    }

    public async void ShowScreen(int targetIndex, bool isFromNetwork = false)
    {
        // do nothing if same screen
        if (targetIndex == currentIndex)
            return;

        // only send bluetooth if the change was triggered in this device
        if (!isFromNetwork) onIndexChanged.Invoke("index_" + targetIndex.ToString());

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
        currentIndex = targetIndex;
        currentChapterIndex = _currentChapterIndex;

        await Task.WhenAll(tasks);

        panelToShow.ShowElements();

        }

    }

    private void OnScreenChange(int targetIndex)
    {
        // close menu
        if (isMenuOpen)
        {
            HideMenu();
        }
        // move crown and show navbar when not in idle screen
        if (targetIndex != 0 && !isNavbarShown)
        {
            TranslateCrownIcon();
            navbarUiPanel.ShowElements();
            isNavbarShown = true;
        }

        // TODO FIX avoid crown show/translate interruption
        // very first screen
        if (targetIndex == 0)
        {
            ShowCrownIcon();
            navbarUiPanel.HideElements();
            isNavbarShown = false;
        }

        // show name of current panel
        //navbarUiPanel.text = animatedPanels[targetIndex].panelName;
        TMPro.TextMeshProUGUI TMProObject = navbarUiPanel.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        TMProObject.text = animatedPanels[targetIndex].panelName;
    }

    public async void TranslateCrownIcon(bool backwards = false)
    {
        crownIcon.transform.DOKill();
        int factor = backwards ? -1 : 1;
        Vector3 transform = new Vector3(-680 * factor, -100 * factor);

        // to state
        crownIcon.transform.DOLocalMove(transform, 1.5f).SetDelay(0.25f).SetEase(Ease.InOutSine); // X -712
        await crownIcon.transform.DOScale(1f, 1.5f).SetDelay(0.5f).AsyncWaitForCompletion(); // Y 1100

    }


    public async void ShowCrownIcon()
    {

        crownIcon.gameObject.SetActive(true);
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

        menuPanel.gameObject.SetActive(true);
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
        menuPanel.gameObject.SetActive(false);
    }

    public async void ToggleMenu()
    {
        if (isMenuOpen){
           HideMenu();
         } else {
            ShowMenu();
        }

    }

    public void UpdateRotation(float[] newRotation)
    {
        // TODO reference to the render model
    }

}
