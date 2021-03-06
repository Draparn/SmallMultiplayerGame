using SmallMultiplayerGame.Server.Client;
using SmallMultiplayerGame.Server.Net;
using SmallMultiplayerGame.Shared;
using System.Collections.Generic;
using UnityEngine;

namespace SmallMultiplayerGame.Server.Gameplay.Pickups
{
	public class GrenadeLauncherServer : MonoBehaviour
	{
		public static List<GrenadeLauncherServer> GrenadeLaunchers = new List<GrenadeLauncherServer>();
		
		public WeaponSlot WeaponSlot = WeaponSlot.GrenadeLauncher;

		public float RespawnTime = 60.0f, CurrentRespawnTime;
		public bool IsActive = true;
		public static byte NextGrenadeLauncherId = 0;
		public byte MyId, AmmoPickup = 8;

		private void Start()
		{
			MyId = NextGrenadeLauncherId;
			NextGrenadeLauncherId++;
			GrenadeLaunchers.Add(this);
		}

		private void Update()
		{
			if (CurrentRespawnTime > 0)
			{
				CurrentRespawnTime -= Time.deltaTime;
				if (CurrentRespawnTime <= 0)
				{
					IsActive = true;
					ServerSend.SendWeaponPickupStatus_ALL(MyId, IsActive);
				}
			}
		}

		private void OnTriggerEnter(Collider other)
		{
			if (other.CompareTag("Player") && IsActive)
			{
				var playerComp = other.GetComponent<PlayerObjectServer>();
				var weapon = playerComp.pickedUpWeapons[(byte)WeaponSlot.GrenadeLauncher];

				if (weapon.IsPickedUp)
				{
					if (weapon.Ammo < weapon.MaxAmmo)
						weapon.Ammo = weapon.Ammo + AmmoPickup > weapon.MaxAmmo ? weapon.MaxAmmo : weapon.Ammo += AmmoPickup;
					else
						return;
				}
				else
				{
					playerComp.pickedUpWeapons[(byte)WeaponSlot.GrenadeLauncher].IsPickedUp = true;
					playerComp.pickedUpWeapons[(byte)WeaponSlot.GrenadeLauncher].Ammo = AmmoPickup;
				}

				IsActive = false;
				CurrentRespawnTime = RespawnTime;

				ServerSend.SendWeaponPickedUp_CLIENT(playerComp.PlayerId, WeaponSlot.GrenadeLauncher, weapon.IsPickedUp, weapon.Ammo);
				ServerSend.SendWeaponPickupStatus_ALL(MyId, IsActive);
			}
		}

	}
}
