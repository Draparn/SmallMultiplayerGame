﻿using NetworkTutorial.Server.Client;
using NetworkTutorial.Shared.Net;
using UnityEngine;

namespace NetworkTutorial.Server.Net
{
	public class ServerSend
	{
		public static void SendWelcomeMessage_CLIENT(byte clientId, string msg)
		{
			using (Packet packet = new Packet((byte)ServerPackets.welcome))
			{
				packet.Write(msg);
				packet.Write(clientId);

				SendToClient(clientId, packet);
			}
		}

		public static void SendSnapshot()
		{
			using (Packet packet = new Packet((byte)ServerPackets.serverSnapshot))
			{
				packet.Write(ServerSnapshot.currentSnapshot.PlayerPosition.Count);
				foreach (var data in ServerSnapshot.currentSnapshot.PlayerPosition)
				{
					packet.Write(data.Value.Id);
					packet.Write(data.Value.FrameNumber);
					packet.Write(data.Value.Position);
				}

				packet.Write(ServerSnapshot.currentSnapshot.ProjectilePositions.Count);
				foreach (var proj in ServerSnapshot.currentSnapshot.ProjectilePositions)
				{
					packet.Write(proj.id);
					packet.Write(proj.transform.position);
				}

				SendToAllClients(packet);
			}

			ServerSnapshot.ClearSnapshot();
		}

		public static void SendPlayerConnected_CLIENT(byte clientId, Player player)
		{
			using (Packet packet = new Packet((byte)ServerPackets.spawnPlayer))
			{
				packet.Write(player.PlayerId);
				packet.Write(player.PlayerName);
				packet.Write(player.transform.position);
				packet.Write(player.transform.rotation);

				SendToClient(clientId, packet);
			}
		}
		public static void SendPlayerDisconnected_ALL(byte clientId)
		{
			using (Packet packet = new Packet((byte)ServerPackets.playerDisconnected))
			{
				packet.Write(clientId);

				SendToAllClients(packet);
			}
		}

		public static void SendPlayerRotationUpdate_ALLEXCEPT(Player player)
		{
			using (Packet packet = new Packet((byte)ServerPackets.playerRotation))
			{
				packet.Write(player.PlayerId);
				packet.Write(player.transform.rotation);

				SendToAllClientsExcept(player.PlayerId, packet);
			}
		}

		public static void SendPlayerHealthUpdate_ALL(Player player)
		{
			using (Packet packet = new Packet((byte)ServerPackets.playerHealth))
			{
				packet.Write(player.PlayerId);
				packet.Write(player.CurrentHealth);

				SendToAllClients(packet);
			}
		}
		public static void SendPlayerRespawned_ALL(Player player)
		{
			using (Packet packet = new Packet((byte)ServerPackets.playerRespawn))
			{
				packet.Write(player.PlayerId);
				packet.Write(player.transform.position);

				SendToAllClients(packet);
			}
		}

		public static void SendHealthpackDeactivate_ALL(byte healthpackId)
		{
			using (Packet packet = new Packet((byte)ServerPackets.healthpackDeactivate))
			{
				packet.Write(healthpackId);

				SendToAllClients(packet);
			}
		}
		public static void SendHealthpackActivate_ALL(byte healthpackId)
		{
			using (Packet packet = new Packet((byte)ServerPackets.healthpackActivate))
			{
				packet.Write(healthpackId);

				SendToAllClients(packet);
			}
		}
		public static void SendHealthpackSpawn_CLIENT(byte clientId, byte healthPackId, Vector3 position)
		{
			using (Packet packet = new Packet((byte)ServerPackets.healthpackSpawn))
			{
				packet.Write(healthPackId);
				packet.Write(position);

				SendToClient(clientId, packet);
			}
		}

		public static void SendProjectileSpawn_ALL(Projectile projectile)
		{
			using (Packet packet = new Packet((byte)ServerPackets.projectileSpawn))
			{
				packet.Write(projectile.id);
				packet.Write(projectile.transform.position);

				SendToAllClients(packet);
			}
		}

		public static void SendProjectileExplosion_ALL(Projectile projectile)
		{
			using (Packet packet = new Packet((byte)ServerPackets.projectileExplosion))
			{
				packet.Write(projectile.id);
				packet.Write(projectile.transform.position);

				SendToAllClients(packet);
			}
		}

		#region BroadcastOptions
		/*
		private static void SendTCPDataToClient(int clientId, Packet packet)
		{
			packet.WriteLength();
			Server.Clients[clientId].udp.SendData(packet);
		}
		*/
		private static void SendToClient(byte clientId, Packet packet)
		{
			packet.WriteLength();
			Server.Clients[clientId].udp.SendData(packet);
		}
		/*
		private static void SendTCPDataToAllClients(Packet packet)
		{
			packet.WriteLength();
			for (ushort i = 1; i <= Server.MaxPlayers; i++)
			{
				Server.Clients[i].udp.SendData(packet);
			}
		}
		*/
		private static void SendToAllClients(Packet packet)
		{
			packet.WriteLength();
			for (byte i = 1; i <= Server.MaxPlayers; i++)
			{
				if (Server.Clients[i].udp != null)
				{
					Server.Clients[i].udp.SendData(packet);
				}
			}
		}

		private static void SendToAllClientsExcept(byte clientIdToExclude, Packet packet)
		{
			packet.WriteLength();
			for (byte i = 1; i <= Server.MaxPlayers; i++)
			{
				if (i == clientIdToExclude)
					continue;

				Server.Clients[i].udp.SendData(packet);
			}
		}
		#endregion
	}
}
