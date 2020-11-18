﻿using UnityEngine;

public class PlayerController : MonoBehaviour
{
	private void FixedUpdate()
	{
		SendInputToServer();
	}

	public void SendInputToServer()
	{
		bool[] inputs = new bool[]
		{
			Input.GetKey(KeyCode.W),
			Input.GetKey(KeyCode.S),
			Input.GetKey(KeyCode.A),
			Input.GetKey(KeyCode.D),
		};

		ClientSend.SendPlayerInputs(inputs);
	}
}
