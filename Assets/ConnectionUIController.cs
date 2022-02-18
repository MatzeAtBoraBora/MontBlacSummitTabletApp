using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class ConnectionUIController : MonoBehaviour
{
    public GameObject serverUI;
    public AnimationPanelController connectingUI;
    public AnimationPanelController notConnectedUI;
    public AnimationPanelController connectedDeviceUI;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnStartServer()
    {
        serverUI.GetComponent<CanvasGroup>().DOFade(0, 0.5f);

        connectingUI.ShowElements();
        notConnectedUI.HideElements();
        connectedDeviceUI.HideElements();

    }
    public void OnClientConnected()
    {
        connectingUI.HideElements();
        notConnectedUI.HideElements();
        connectedDeviceUI.ShowElements();

    }
}
