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
    public GameObject swipeScreensContainer;
    [System.Serializable]
	public class IndexEvent : UnityEvent<int>
	{
	}
	public IndexEvent onIndexChanged;

    private AnimationPanelController[] animatedPanels;
    private int currentIndex = -1;
    private int currentChapterIndex = 0;



    // Start is called before the first frame update
    async void Start()
    {
        // instantiate animatedPanels
        animatedPanels = swipeScreensContainer.GetComponentsInChildren<AnimationPanelController>();


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
        if (!isFromNetwork) onIndexChanged.Invoke(targetIndex);

        var tasks = new List<Task>();
        int _currentChapterIndex = -1; // will turn 0 on first loop run
        AnimationPanelController panelToShow = null;


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

    public AnimationPanelController[] GetSwipePanels()
    {
        return animatedPanels;
    }

    public AnimationPanelController GetSwipePanelByIndex(int index)
    {
        return animatedPanels[index];
    }

    public void UpdateRotation(float[] newRotation)
    {
        // TODO reference to the render model
    }

}
