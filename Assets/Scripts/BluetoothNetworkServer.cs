using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Shatalmic;
using System;
using System.Text;

public class BluetoothNetworkServer : MonoBehaviour
{
	private Networking networking = null;
	private List<Networking.NetworkDevice> connectedDeviceList = null;
	private bool isServer = true;
	private bool writeDeviceBytes = false;
	private byte[] bytesToWrite = null;
	private int deviceToWriteIndex = 0;
	private bool sendingCubeRotation = false;
	private Quaternion newCubeRotation = Quaternion.identity;
	private Networking.NetworkDevice deviceToSkip = null;

	public GameObject Cube;
	public Text TextStatus;
	public GameObject BluetoothSettingsDialog;

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
						sendingCubeRotation = false;
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

		if (newCubeRotation != Quaternion.identity)
		{
			if (Cube != null)
				Cube.transform.rotation = newCubeRotation;
			newCubeRotation = Quaternion.identity;
		}
	}

	protected void SendCubeRotation(Quaternion rotation, Networking.NetworkDevice device = null)
	{
		if (!sendingCubeRotation)
		{
			sendingCubeRotation = true;

			var bytes = new byte[16];
			Array.Copy(BitConverter.GetBytes(rotation.x), 0, bytes, 0, 4);
			Array.Copy(BitConverter.GetBytes(rotation.y), 0, bytes, 4, 4);
			Array.Copy(BitConverter.GetBytes(rotation.z), 0, bytes, 8, 4);
			Array.Copy(BitConverter.GetBytes(rotation.w), 0, bytes, 12, 4);

			if (isServer)
			{
				if (device == null)
				{
					if (connectedDeviceList != null)
					{
						if (connectedDeviceList.Count == 1)
						{
							if (deviceToSkip == null)
							{
								networking.WriteDevice(connectedDeviceList[0], bytes, () =>
								{
									sendingCubeRotation = false;
								});
							}
							else
							{
								deviceToSkip = null;
								sendingCubeRotation = false;
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
					networking.WriteDevice(device, bytes, () =>
					{
						sendingCubeRotation = false;
					});
				}
			}
			else
			{
				networking.SendFromClient(bytes);
				sendingCubeRotation = false;
			}
		}
	}

	int count = 0;
	protected void ParseCubePosition(byte[] bytes)
	{
		if (count > 0 || (bytes[0] == '0' && bytes[1] == '1' && bytes[2] == '2' && bytes[3] == '3' && bytes[4] == '4'))
		{
			count += bytes.Length;
			BluetoothLEHardwareInterface.Log("Large bytes: " + count.ToString() + " : " + BitConverter.ToString(bytes));
			if (count >= 300)
				count = 0;
		}
		else
		{
			newCubeRotation = new Quaternion(BitConverter.ToSingle(bytes, 0), BitConverter.ToSingle(bytes, 4), BitConverter.ToSingle(bytes, 8), BitConverter.ToSingle(bytes, 12));

			if (isServer)
				SendCubeRotation(newCubeRotation);
		}
	}

	public String networkName = "summit";
	public String clientName = "summitClient";
	public GameObject ButtonStartServer;
	public GameObject ButtonStartClient;
	public GameObject ButtonStopNetwork;
	public GameObject ButtonSendTestData;

	public void OnButton(Button button)
	{
		switch (button.name)
		{		
			case "ButtonStartServer":
				
					TextStatus.text = "Start Server";

					deviceToSkip = null;
					deviceToWriteIndex = 0;
					sendingCubeRotation = false;
					isServer = true;
					ButtonStartServer.SetActive(false);
					ButtonStartClient.SetActive(false);
					ButtonStopNetwork.SetActive(true);
					ButtonSendTestData.SetActive(true);

					if (Cube != null)
						Cube.SetActive(false);

					networking.StartServer(networkName, (connectedDevice) =>
					{
						if (connectedDeviceList == null)
							connectedDeviceList = new List<Networking.NetworkDevice>();

						if (!connectedDeviceList.Contains(connectedDevice))
						{
							connectedDeviceList.Add(connectedDevice);

							if (Cube != null)
							{
								Cube.SetActive(true);
								SendCubeRotation(Cube.transform.rotation, connectedDevice);
							}
						}
					}, (disconnectedDevice) =>
					{
						if (connectedDeviceList != null && connectedDeviceList.Contains(disconnectedDevice))
							connectedDeviceList.Remove(disconnectedDevice);
					}, (dataDevice, characteristic, bytes) =>
					{
						deviceToSkip = dataDevice;
						ParseCubePosition(bytes);
					});
				
				break;

			case "ButtonStartClient":
				if (string.IsNullOrEmpty(networkName) || string.IsNullOrEmpty(clientName))
				{
					TextStatus.text = "Enter both a network and client name";
				}
				else
				{
					TextStatus.text = "Start Client";

					isServer = false;
					ButtonStartServer.SetActive(false);
					ButtonStartClient.SetActive(false);
					ButtonStopNetwork.SetActive(true);
					ButtonSendTestData.SetActive(true);

					if (Cube != null)
						Cube.SetActive(false);

					networking.StartClient(networkName, clientName, () =>
					{
						networking.StatusMessage = "Started advertising";
					}, (clientName, characteristic, bytes) =>
					{
						if (Cube != null)
							Cube.SetActive(true);
						ParseCubePosition(bytes);
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

						if (Cube != null)
							Cube.SetActive(false);
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

						if (Cube != null)
							Cube.SetActive(false);
					});
				}
				break;

			case "ButtonSendTestData":
				{
					var bytes = new byte[300];
					bytes[0] = (byte)'0';
					bytes[1] = (byte)'1';
					bytes[2] = (byte)'2';
					bytes[3] = (byte)'3';
					bytes[4] = (byte)'4';
					for (int i = 5; i < 300; i++)
						bytes[i] = (byte)('A' + (i % 26));

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
										sendingCubeRotation = false;
									});
								}
								else
								{
									deviceToSkip = null;
									sendingCubeRotation = false;
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
						networking.SendFromClient(bytes);
						sendingCubeRotation = false;
					}
				}
				break;
		}
	}

	public void OnOK()
	{
		BluetoothSettingsDialog.SetActive(false);
	}

	List<string> bluetoothErrors = new List<string>
	{
		"Bluetooth LE Not Enabled",
		"Bluetooth LE Not Powered Off",
		"Bluetooth LE Not Available",
		"Bluetooth LE Not Supported",
	};

	// Use this for initialization
	void Start()
	{
		BluetoothSettingsDialog.SetActive(false);
		ButtonStartServer.SetActive(true);
		ButtonStartClient.SetActive(true);
		ButtonStopNetwork.SetActive(false);
		ButtonSendTestData.SetActive(false);
		if (Cube != null)
			Cube.SetActive(false);

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

					// TODO: I leave it to the plugin user to determine how to handle the other errors

					// TODO: You will also want to restructure your code so that when the app comes back from setting
					//       it will retry initializing. Some users like to have a different scene for that.
				}

				BluetoothLEHardwareInterface.Log("Error: " + error);
			}, (message) =>
			{
				if (TextStatus != null)
					TextStatus.text = message;

				BluetoothLEHardwareInterface.Log("Message: " + message);
			});
		}

		if (Cube != null)
		{
			var mouseDrageRotate = Cube.GetComponent<MouseDragRotate>();
			if (mouseDrageRotate != null)
			{
				mouseDrageRotate.OnMouseEvent = (rotation) =>
				{
					deviceToSkip = null;
					SendCubeRotation(rotation);
				};
			}
		}
	}
}
