﻿using SmallMultiplayerGame.Client.Gameplay.WeaponScrips;
using SmallMultiplayerGame.Client.Net;
using SmallMultiplayerGame.Shared;
using System.Collections;
using UnityEngine;

namespace SmallMultiplayerGame.Client.Gameplay.Player
{
	public class PlayerObjectClient : MonoBehaviour
	{
		public GameObject PlayerMesh, WeaponMeshHolder;
		private GameObject currentWeapon;
		private WeaponClient weapon;
		private MeshRenderer PlayerMeshRenderer;

		private Color originalColor;

		private string PlayerName;
		public float currentHealth;
		public float maxHealth = 100.0f;
		private byte PlayerId;
		private bool flickering = false;

		public void Init(byte id, string playerName)
		{
			PlayerId = id;
			PlayerName = playerName;
			currentHealth = maxHealth;
			PlayerMeshRenderer = PlayerMesh.GetComponent<MeshRenderer>();
			originalColor = PlayerMeshRenderer.material.color;
			SetWeaponMesh(PlayerId, (byte)WeaponSlot.Pistol);
		}

		public void SetHealth(byte clientId, float newHealthValue)
		{
			if (clientId == LocalClient.Instance.MyId)
				FlashUI(newHealthValue);

			if (newHealthValue < currentHealth)
			{
				if (!flickering)
					StartCoroutine(Flicker());
			}

			currentHealth = newHealthValue;

			if (currentHealth <= 0)
			{
				if (clientId == LocalClient.Instance.MyId)
					GameManagerClient.Instance.LocalPositionPredictions.Clear();

				Die();
			}
		}

		public void FireWeapon()
		{
			weapon.Shoot();
		}

		public void SetWeaponMesh(byte id, byte slot)
		{
			if (WeaponMeshHolder.transform.childCount > 0)
				Destroy(WeaponMeshHolder.transform.GetChild(0).gameObject);

			currentWeapon = Instantiate(Weapons.AllWeapons[slot].ClientPrefab, WeaponMeshHolder.transform);
			weapon = currentWeapon.GetComponent<WeaponClient>();

			if (id == LocalClient.Instance.MyId)
				UIManager.Instance.SetAmmoCount(slot == (byte)WeaponSlot.Pistol ? "Inf" : PlayerController.Instance.pickedUpWeapons[slot].Ammo.ToString());

		}

		private void FlashUI(float newHealthValue)
		{
			if (newHealthValue < currentHealth)
				UIManager.Instance.TakeDamage(newHealthValue <= 0);
			else if (newHealthValue > currentHealth)
				UIManager.Instance.HealDamage();

			UIManager.Instance.SetHealthText(newHealthValue.ToString());
			UIManager.Instance.SetHealthTextColor(newHealthValue);
		}

		private void Die()
		{
			PlayerMeshRenderer.gameObject.SetActive(false);
			currentWeapon.SetActive(false);
		}

		public void Respawn(Vector3 position, byte playerId)
		{
			gameObject.transform.position = position;
			SetWeaponMesh(playerId, (byte)WeaponSlot.Pistol);

			currentHealth = maxHealth;
			PlayerMeshRenderer.gameObject.SetActive(true);

			if (playerId == LocalClient.Instance.MyId)
			{
				PlayerController.Instance.SetRespawnValues();
				UIManager.Instance.Respawn();
				UIManager.Instance.SetHealthText(maxHealth.ToString());
				UIManager.Instance.SetHealthTextColor(maxHealth);
			}
		}

		private IEnumerator Flicker()
		{
			flickering = true;

			byte counter = 0;
			do
			{
				PlayerMeshRenderer.material.color = Color.red;
				yield return new WaitForSeconds(0.1f);
				PlayerMeshRenderer.material.color = originalColor;
				yield return new WaitForSeconds(0.1f);

				counter++;
			} while (counter < 2);

			flickering = false;
		}

	}
}
