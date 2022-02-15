using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Threading.Tasks;
using System.Linq;
using System;

public class AnimationSytemController : MonoBehaviour
{
    public AnimationPanelController[] animatedPanels;
    private int currentIndex = 0;
    private int currentChapterIndex = 0;

    // Start is called before the first frame update
    async void Start()
    {
        // start faded out for now
        await animatedPanels[currentIndex].HideElements();
        ShowScreen(currentIndex);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Step(int delta)
    {
        int targetIndex = (currentIndex + delta) % animatedPanels.Length;
        Debug.Log("Step to: " + targetIndex);
        ShowScreen(targetIndex);
    }

    public void ChapterStep(int delta)
    {
        // find next or previous chapter start
        AnimationPanelController[] chapterPanels = animatedPanels.Where(c => c.isChapterStart).ToArray();
        int targetChapterIndex = (currentChapterIndex + delta) % chapterPanels.Length;

        // get the panel index for the panel which matches the start chapter panel
        int targetIndex = Array.FindIndex(animatedPanels, p => p == chapterPanels[targetChapterIndex]);

        ShowScreen(targetIndex);
    }

    public async void ShowScreen(int targetIndex)
    {

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
                panelToShow = animatedPanels[index];

            } else
            {
                tasks.Add(animatedPanels[index].HideElements());
            }

        }

        await Task.WhenAll(tasks);

        panelToShow.ShowElements();

        currentIndex = targetIndex;
        currentChapterIndex = _currentChapterIndex;

    }

}
