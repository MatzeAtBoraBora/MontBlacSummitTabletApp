using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class GyroAccelerationController : MonoBehaviour
{
    public GameObject icon;
    public bool isGyro = false;

    //by now static because of canvas scaler
    private float screenWith = 1668.0f;
    private float screenHeight = 2224.0f;
    private float verticalGap = 220.0f;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateData(float[] dataSet)
    {
        if (!isGyro)
        {
            float direction = dataSet[0];
            if (direction == 0)
            {
                moveLeft();
            }
            else
            {
                moveRight();
            }
        }
        else {
            float xPos = screenWith / 2 + dataSet[0] * screenWith / 3;
            float yPos = screenHeight / 2 + dataSet[1] * screenHeight / 3 + verticalGap;
            icon.transform.DOMove(new Vector3(xPos, yPos, 0), 0.3f).SetEase(Ease.Linear);
        }
    }

    public void moveLeft()
    {
        icon.transform.DOMove(new Vector3(screenWith / 2 -screenWith / 3 , screenHeight / 2 + verticalGap, 0), 0.3f).SetEase(Ease.InOutSine);
        icon.transform.DOMove(new Vector3(screenWith / 2, Screen.height / 2 + verticalGap, 0), 1f).SetDelay(1.0f);
    }
    public void moveRight()
    {
        icon.transform.DOMove(new Vector3(screenWith / 2 + screenWith / 3, screenHeight / 2 + verticalGap, 0), 0.3f).SetEase(Ease.InOutSine);
        icon.transform.DOMove(new Vector3(screenWith / 2, screenHeight / 2 + verticalGap, 0), 1f).SetDelay(1.0f);
    }
}
