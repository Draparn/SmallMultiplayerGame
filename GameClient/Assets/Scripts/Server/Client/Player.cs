﻿using NetworkTutorial.Server.Managers;
using NetworkTutorial.Server.Net;
using UnityEngine;

namespace NetworkTutorial.Server.Client
{
	public class Player : MonoBehaviour
	{
		private Vector2 InputDirection;

		public Transform ShootOrigin;
		private CharacterController controller;

		public string PlayerName;

		public float CurrentHealth = 100.0f;
		public float maxHealth = 100.0f;
		public float PrimaryFireDamage = 10.0f;
		public float ThrowForce = 600.0f;
		public float gravity = -40f;
		public float moveSpeed = 5.0f;
		private float jumpSpeed = 8.0f;
		private float yVelocity = 0;
		private bool hitScan = false;

		public int PlayerId;

		private bool[] playerInput;

		private void Start()
		{
			controller = gameObject.GetComponent<CharacterController>();
			gravity *= Time.fixedDeltaTime * Time.fixedDeltaTime;
			moveSpeed *= Time.fixedDeltaTime;
			jumpSpeed *= Time.fixedDeltaTime;
		}

		public void FixedUpdate()
		{
			if (CurrentHealth <= 0)
				return;

			InputDirection = Vector2.zero;
			if (playerInput[0])         //W
				InputDirection.y += 1;
			if (playerInput[1])         //S
				InputDirection.y -= 1;
			if (playerInput[2])         //A
				InputDirection.x -= 1;
			if (playerInput[3])         //D
				InputDirection.x += 1;

			MovePlayer(InputDirection);
		}

		public void Init(int id, string name)
		{
			PlayerName = name;
			PlayerId = id;
			CurrentHealth = maxHealth;

			playerInput = new bool[5];
		}

		private void MovePlayer(Vector2 inputDirection)
		{
			var moveDirection = transform.right * inputDirection.x + transform.forward * inputDirection.y;
			moveDirection *= moveSpeed;

			if (controller.isGrounded)
			{
				yVelocity = 0;

				if (playerInput[4]) //Spacebar
					yVelocity = jumpSpeed;
			}
			else
				yVelocity += gravity;

			moveDirection.y = yVelocity;
			controller.Move(moveDirection);

			ServerSend.SendPlayerPositionUpdate_UDP(this);
			ServerSend.SendPlayerRotationUpdate_UDP(this);
		}

		public void PrimaryFire(Vector3 viewDirection)
		{
			if (CurrentHealth <= 0)
				return;

			if (hitScan)
			{
				if (Physics.Raycast(ShootOrigin.position, viewDirection, out RaycastHit hit))
				{
					if (hit.collider.CompareTag("Player"))
					{
						hit.collider.GetComponent<Player>().TakeDamage(PrimaryFireDamage);
					}
				}
			}
			else
			{
				NetworkManager.instance.InstantiateProjectile(ShootOrigin).Init(viewDirection, ThrowForce, PlayerId);
			}
		}

		public void TakeDamage(float damage)
		{
			if (CurrentHealth <= 0)
				return;

			CurrentHealth -= damage;

			if (CurrentHealth <= 0)
				PlayerDied();

			ServerSend.SendPlayerHealthUpdate_TCP(this);
		}

		private void PlayerDied()
		{
			CurrentHealth = 0;
			controller.enabled = false;

			Invoke(nameof(PlayerRespawn), 3);
		}

		private void PlayerRespawn()
		{
			transform.position = new Vector3(0, 0.75f, 0);
			CurrentHealth = maxHealth;
			controller.enabled = true;

			ServerSend.SendPlayerRespawned_TCP(this);
		}

		public void UpdatePosAndRot(bool[] inputs, Quaternion rot)
		{
			playerInput = inputs;
			transform.rotation = rot;
		}

	}
}
