using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class AnimationSytemController : MonoBehaviour
{
    public AnimationPanelController[] animatedPanels;
    public int currentIndex = 0;
    // Start is called before the first frame update
    void Start()
    {
        showScreen(currentIndex);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void step(int delta)
    {
        showScreen((currentIndex + delta) % animatedPanels.Length);
    }

    public void showScreen(int targetIndex)
    {

        for (int index = 0; index <= animatedPanels.Length; index++)
        {
            AnimationPanelController element = animatedPanels[index];
            if (targetIndex == index)
            {
                // TODO wait until hideElements is done to showElements
                element.showElements();
            } else
            {
                element.hideElements();
            }
        }

        currentIndex = targetIndex;
    }

}
