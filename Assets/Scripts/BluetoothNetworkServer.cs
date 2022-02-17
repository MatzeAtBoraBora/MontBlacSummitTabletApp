using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Shatalmic;
using System;
using UnityEngine.Events;
using DG.Tweening;
public class BluetoothNetworkServer : MonoBehaviour
{
	/* External UI attributes */
	[Space]
	[Header("Network Settings")]
	public String networkName = "summit";
	public String clientName = "summitClient";
	public bool isClient = true;

	[Space]
	[Header("Action Buttons")]
	public GameObject ButtonStartServer;
	public GameObject ButtonStartClient;
	public GameObject ButtonStopNetwork;
	public GameObject ButtonSendTestData;

	
	[Space]
	[Header("Debug & Explaination Layer")]
	public Text ServerOutputText;
	public GameObject NetworkDialog;
	public GameObject BluetoothSettingsDialog;
	public GameObject ExplainationDialog;


	[Space]
	[Header("Custom Events")]
	public UnityEvent OnStartServer;
	public UnityEvent OnStartClient;
	public UnityEvent OnClientConnected;
	[System.Serializable]
	public class MessageEvent : UnityEvent<String>
	{
	}
	public MessageEvent OnMessageReceived;
	public UnityEvent OnStopServer;


	/* Internal Server attributes */
	private Networking networking = null;
	private List<Networking.NetworkDevice> connectedDeviceList = null;
	private bool isServer = true;
	private bool writeDeviceBytes = false;
	private byte[] bytesToWrite = null;
	private int deviceToWriteIndex = 0;
	private Networking.NetworkDevice deviceToSkip = null;


	// Use this for initialization
	void Start()
	{
		BluetoothSettingsDialog.SetActive(false);
		if (isClient)
		{
			ButtonStartClient.SetActive(true);
			ButtonStartServer.SetActive(false);
		}
		else
		{
			ButtonStartServer.SetActive(true);
			ButtonStartClient.SetActive(false);
		}

		ButtonStopNetwork.SetActive(false);
		ButtonSendTestData.SetActive(false);
		
		if (networking == null)
		{
			networking = GetComponent<Networking>();
			networking.Initialize((error) =>
			{
				// see if we got a standard bluetooth error
				if (bluetoothErrors.Contains(error))
				{
					// if the error is one of these then show the dialog asking the user
					// to enable bluetooth
					if (error.Equals(bluetoothErrors[0]) || error.Equals(bluetoothErrors[1]))
						BluetoothSettingsDialog.SetActive(true);
				}

				BluetoothLEHardwareInterface.Log("Error: " + error);
			}, (message) =>
			{
				if (ServerOutputText != null)
					ServerOutputText.text = message;

				BluetoothLEHardwareInterface.Log("Message: " + message);
			});
		}
	}

	// because the networking library is asynchronous due to the nature of
	// writing to the bluetooth stack, we have to write each client device
	// and then wait for that write to be completed before we write the next
	// device.
	// notice how this update loop watches for the writeDeviceBytes flag to be
	// true, calls the bluetooth write and then when the bluetooth
	// callback occurs goes on to the next device or signals that it is done
	private void Update()
	{
		if (writeDeviceBytes)
		{
			writeDeviceBytes = false;

			if (bytesToWrite != null && connectedDeviceList != null && deviceToWriteIndex >= 0 && deviceToWriteIndex < connectedDeviceList.Count)
			{
				Action afterWrite = () =>
				{
					deviceToWriteIndex++;
					writeDeviceBytes = (deviceToWriteIndex < connectedDeviceList.Count);
					if (!writeDeviceBytes)
					{
						bytesToWrite = null;
					}
				};

				var deviceToWrite = connectedDeviceList[deviceToWriteIndex];
				if (deviceToWrite != deviceToSkip)
				{
					networking.WriteDevice(deviceToWrite, bytesToWrite, afterWrite);
				}
				else
				{
					afterWrite();
				}
			}
		}
	}

	public void OnButton(Button button)
	{
		switch (button.name)
		{		
			case "ButtonStartServer":
				
					ServerOutputText.text = "Start Server";

					deviceToSkip = null;
					deviceToWriteIndex = 0;
					isServer = true;
					ButtonStartServer.SetActive(false);
					ButtonStartClient.SetActive(false);
					ButtonStopNetwork.SetActive(true);
					ButtonSendTestData.SetActive(true);

					HidePanel();

					networking.StartServer(networkName, (connectedDevice) =>
					{
						if (connectedDeviceList == null)
							connectedDeviceList = new List<Networking.NetworkDevice>();

						if (!connectedDeviceList.Contains(connectedDevice))
						{
							connectedDeviceList.Add(connectedDevice);

							//We are connected
							OnClientConnected.Invoke();
							HidePanel();
						}
						//Start Server
						OnStartServer.Invoke();
					}, (disconnectedDevice) =>
					{
						if (connectedDeviceList != null && connectedDeviceList.Contains(disconnectedDevice))
							connectedDeviceList.Remove(disconnectedDevice);
					}, (dataDevice, characteristic, bytes) =>
					{
						deviceToSkip = dataDevice;
						//We are getting bytes in
						ReadOutBytes(bytes);
					});
				
				break;

			case "ButtonStartClient":
				if (string.IsNullOrEmpty(networkName) || string.IsNullOrEmpty(clientName))
				{
					ServerOutputText.text = "Enter both a network and client name";
				}
				else
				{
					ServerOutputText.text = "Start Client";

					isServer = false;
					ButtonStartServer.SetActive(false);
					ButtonStartClient.SetActive(false);
					ButtonStopNetwork.SetActive(true);
					ButtonSendTestData.SetActive(true);

					networking.StartClient(networkName, clientName, () =>
					{
						networking.StatusMessage = "Started advertising";
						OnStartClient.Invoke();
					}, (clientName, characteristic, bytes) =>
					{
						ReadOutBytes(bytes);
					});
				}
				break;

			case "ButtonStopNetwork":
				if (isServer)
				{
					if (connectedDeviceList != null)
						connectedDeviceList = null;
					networking.StopServer(() =>
					{
						ButtonStartServer.SetActive(true);
						ButtonStartClient.SetActive(true);
						ButtonStopNetwork.SetActive(false);
						ButtonSendTestData.SetActive(false);
						OnStopServer.Invoke();
					});
				}
				else
				{
					networking.StopClient(() =>
					{
						ButtonStartServer.SetActive(true);
						ButtonStartClient.SetActive(true);
						ButtonStopNetwork.SetActive(false);
						ButtonSendTestData.SetActive(false);
						OnStopServer.Invoke();
					});
				}
				break;

			case "ButtonSendTestData":
				{
					byte[] bytes = System.Text.Encoding.UTF8.GetBytes("Hello "+ UnityEngine.Random.Range(0, 100) + "from Client:"+isClient);

					if (isServer)
					{
						if (connectedDeviceList != null)
						{
							if (connectedDeviceList.Count == 1)
							{
								if (deviceToSkip == null)
								{
									networking.WriteDevice(connectedDeviceList[0], bytes, () =>
									{
										//we are sending data in our channel
									});
								}
								else
								{
									deviceToSkip = null;
									//we are not writing in our channel
								}
							}
							else
							{
								deviceToWriteIndex = 0;
								writeDeviceBytes = true;
								bytesToWrite = bytes;
							}
						}
						else if (deviceToSkip != null)
							deviceToSkip = null;
					}
					else
					{
						//sending out test data
						networking.SendFromClient(bytes);
					}
				}
				break;
		}
	}
	private void HidePanel(){
		gameObject.GetComponent<CanvasGroup>().DOFade(0,0.5f);
		gameObject.GetComponent<CanvasGroup>().interactable = false;
		gameObject.GetComponent<CanvasGroup>().blocksRaycasts = false;
	}	public void OnOK()
	{
		BluetoothSettingsDialog.SetActive(false);
	}

	int count = 0;
	private void ReadOutBytes(byte[] bytes) {
		ServerOutputText.text = "Getting Bytes in!";
		String bytesAsString = System.Text.Encoding.UTF8.GetString(bytes);
		ServerOutputText.text = bytesAsString;
		OnMessageReceived.Invoke(bytesAsString);
	}
	public void OnApplicationQuit()
	{
		if (Application.isEditor)
		{
			BluetoothLEHardwareInterface.DeInitialize(() =>
			{
				Console.WriteLine("Deinit done");
			});
		}
	}
public void SendServerMessage(string message){
		Debug.Log("Message" + message);
		byte[] bytes = System.Text.Encoding.UTF8.GetBytes(message);
			if (isServer)
			{
				Debug.Log("Message is Server" + message);
				if (connectedDeviceList != null)
				{
					Debug.Log("Message Device List" + message);
					if (connectedDeviceList.Count == 1)
					{
						Debug.Log("Message Device List Count" + message);
						if (deviceToSkip == null)
						{
							Debug.Log("Message Device to Skip" + message);
							networking.WriteDevice(connectedDeviceList[0], bytes, () =>
							{
								//we are sending data in our channel
								Debug.Log("Message Final" + message);
							});
						}
						else
						{
							deviceToSkip = null;
							//we are not writing in our channel
						}
					}
					else
					{
						deviceToWriteIndex = 0;
						writeDeviceBytes = true;
						bytesToWrite = bytes;
					}
				}
				else if (deviceToSkip != null)
					deviceToSkip = null;
			}
			else
			{
				//sending out test data
				networking.SendFromClient(bytes);
			}
	}
	List<string> bluetoothErrors = new List<string>
	{
		"Bluetooth LE Not Enabled",
		"Bluetooth LE Not Powered Off",
		"Bluetooth LE Not Available",
		"Bluetooth LE Not Supported",
	};
}
