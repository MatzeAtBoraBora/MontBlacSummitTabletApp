using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WatchController : MonoBehaviour
{
    public Text uiText;
    // Start is called before the first frame update
    void Start()
    {
        if (SystemInfo.supportsGyroscope) {
            Input.gyro.enabled = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        uiText.text = "Gyrodata: " + Input.gyro.attitude + " / "+Input.anyKey;
    }
}
