﻿using NetworkTutorial.Shared;
using UnityEngine;

namespace NetworkTutorial.Server.Net
{
	public class NetworkManager : MonoBehaviour
	{
		public static NetworkManager instance;

		private float snapshotInterval;

		private void Awake()
		{
			if (instance == null)
				instance = this;
			else if (instance != this)
				Destroy(this);
		}

		private void Start()
		{
			QualitySettings.vSyncCount = 0;
			Application.targetFrameRate = 60;
			Server.StartServer(ConstantValues.SERVER_MAX_PLAYERS);
		}

		private void LateUpdate()
		{
			snapshotInterval += Time.deltaTime;

			if (snapshotInterval >= ConstantValues.SERVER_TICK_RATE)
			{
				snapshotInterval = 0;
				ServerSend.SendSnapshot();
			}
		}

		private void OnApplicationQuit()
		{
			Server.StopServer();
		}

	}
}