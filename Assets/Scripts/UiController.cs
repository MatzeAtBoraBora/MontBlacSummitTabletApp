using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class UiController : MonoBehaviour
{

    public AnimationPanelController crownIcon;
    public AnimationPanelController navbarUiPanel;
    public AnimationPanelController menuPanel;

    private bool isMenuOpen = false;
    private bool isNavbarShown = false;


    void Start()
    {
        // start faded out for now
        crownIcon.HideElements();
        navbarUiPanel.HideElements();
        HideMenu();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void HandleSceenIndexChange(int targetIndex)
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
        // get animatedPanels from sybling component
        AnimationSytemController animationSytemController = this.GetComponent<AnimationSytemController>();
        if (animationSytemController)
        {
            TMProObject.text = animationSytemController.GetSwipePanelByIndex(targetIndex).panelName;
        }
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
        menuPanel.GetComponent<CanvasGroup>().DOFade(0, 0f);

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
        if (isMenuOpen)
        {
            HideMenu();
        }
        else
        {
            ShowMenu();
        }

    }

}
