﻿using UnityEngine;

namespace NetworkTutorial.Client.Player
{
	public class CameraController : MonoBehaviour
	{
		public PlayerClient player;
		public float sensitivity = 100f;
		public float clampAngle = 85f;

		private float verticalRotation;
		private float horizontalRotation;

		private void Start()
		{
			verticalRotation = transform.localEulerAngles.x;
			horizontalRotation = player.transform.eulerAngles.y;
			Cursor.lockState = CursorLockMode.Locked;
		}

		private void Update()
		{
			if (player.currentHealth > 0)
				Look();
		}

		private void Look()
		{
			float _mouseVertical = -Input.GetAxis("Mouse Y");
			float _mouseHorizontal = Input.GetAxis("Mouse X");

			verticalRotation += _mouseVertical * sensitivity * Time.deltaTime;
			horizontalRotation += _mouseHorizontal * sensitivity * Time.deltaTime;

			verticalRotation = Mathf.Clamp(verticalRotation, -clampAngle, clampAngle);

			transform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
			player.transform.rotation = Quaternion.Euler(0f, horizontalRotation, 0f);
		}
	}
}