﻿using NetworkTutorial.Server.Gameplay;
using NetworkTutorial.Server.Net;
using NetworkTutorial.Shared;
using NetworkTutorial.Shared.Utils;
using UnityEngine;

namespace NetworkTutorial.Server.Client
{
	public class PlayerServer : MonoBehaviour
	{
		private Vector2 inputDirection;

		private Vector3[] respawnPoints = new Vector3[3]
		{
			new Vector3(0, 0, 0),
			new Vector3(0, 2.4f, -18),
			new Vector3(-5, 0, 20)
		};

		public Transform ShootOrigin;
		private CharacterController controller;
		private Vector3 prevPos;

		public string PlayerName;

		public float CurrentHealth = 100.0f;
		public float MaxHealth = 100.0f;
		private float PrimaryFireDamage = 10.0f;
		private float ThrowForce = 600.0f;
		private float yVelocity;

		private bool hitScan = false;

		public byte PlayerId;
		public ushort SequenceNumber = ushort.MaxValue;

		private InputsStruct playerInput;

		private void Start()
		{
			controller = gameObject.GetComponent<CharacterController>();
		}

		public void Init(byte id, string name)
		{
			PlayerName = name;
			PlayerId = id;
			CurrentHealth = MaxHealth;

			playerInput = new InputsStruct();
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
						hit.collider.GetComponent<PlayerServer>().TakeDamage(PrimaryFireDamage);
					}
				}
			}
			else
			{
				GameManagerServer.Instance.InstantiateProjectile(ShootOrigin).Init(viewDirection, ThrowForce, PlayerId);
			}
		}

		public void TakeDamage(float damage)
		{
			if (CurrentHealth <= 0)
				return;

			CurrentHealth -= damage;

			if (CurrentHealth <= 0)
				PlayerDied();

			ServerSend.SendPlayerHealthUpdate_ALL(this);
		}
		public void HealDamage(float healing)
		{
			CurrentHealth = CurrentHealth + healing > MaxHealth ? MaxHealth : CurrentHealth + healing;
		}

		private void PlayerDied()
		{
			CurrentHealth = 0;
			controller.enabled = false;

			Invoke(nameof(PlayerRespawn), 3);
		}

		private void PlayerRespawn()
		{
			transform.position = respawnPoints[Random.Range(0, 3)];
			CurrentHealth = MaxHealth;
			controller.enabled = true;

			ServerSend.SendPlayerRespawned_ALL(this);
		}

		public void UpdatePosAndRot(ushort sequenceNumber, InputsStruct inputs, Quaternion rot)
		{
			if (!IsMoreRecent(sequenceNumber))
				return;

			SequenceNumber = sequenceNumber;
			playerInput = inputs;
			transform.rotation = rot;

			prevPos = transform.position;
			controller.Move(PlayerMovementCalculations.CalculatePlayerPosition(playerInput, transform, ref yVelocity, controller.isGrounded));

			if (transform.position != prevPos)
				ServerSnapshot.AddPlayerMovement(PlayerId, transform.position, SequenceNumber);
			ServerSend.SendPlayerRotationUpdate_ALLEXCEPT(this);
		}

		private bool IsMoreRecent(ushort newSequenceNumber)
		{
			return ((newSequenceNumber > SequenceNumber) && (newSequenceNumber - SequenceNumber <= 32768)) ||
				((newSequenceNumber < SequenceNumber) && (SequenceNumber - newSequenceNumber > 32768));
		}
	}
}
