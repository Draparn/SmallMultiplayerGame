﻿using NetworkTutorial.Client.Gameplay;
using NetworkTutorial.Shared;
using System.Collections.Generic;
using UnityEngine;

namespace NetworkTutorial.Client.Net
{
	public class ClientSnapshot
	{
		public static List<ClientSnapshot> Snapshots = new List<ClientSnapshot>();

		internal List<PlayerPosData> players;
		internal List<ProjectileData> projectiles;

		public ClientSnapshot(List<PlayerPosData> players, List<ProjectileData> projectiles)
		{
			this.players = players.Count > 0 ? players : null;
			this.projectiles = projectiles.Count > 0 ? projectiles : null;

			foreach (var playerData in players)
			{
				if (LocalClient.Instance.MyId == playerData.PlayerId)
				{
					CheckPosAndReconcile(playerData);
					break;
				}
			}
		}

		private void CheckPosAndReconcile(PlayerPosData playerData)
		{
			for (int i = 0; i < GameManagerClient.Instance.LocalPositionPredictions.Count; i++)
			{
				if (GameManagerClient.Instance.LocalPositionPredictions[i].FrameNumber == playerData.FrameNumber)
				{
					if (Vector3.Distance(GameManagerClient.Instance.LocalPositionPredictions[i].Position, playerData.Position) > 0.2f)
					{
						//Debug.LogError($"Correcting. Index:{i}. Frame:{playerData.FrameNumber}, Predicted pos was: {GameManager.Instance.LocalPositionPredictions[i].Position} and should be: {playerData.Position}");
						GameManagerClient.Instance.LocalPositionPredictions.RemoveRange(0, i);

						for (int j = 0; j < GameManagerClient.Instance.LocalPositionPredictions.Count; j++)
						{
							var prediction = GameManagerClient.Instance.LocalPositionPredictions[j];

							if (j == 0)
							{
								prediction.Position = playerData.Position;
								GameManagerClient.Instance.LocalPositionPredictions[j] = new LocalPredictionData(prediction);
							}
							else
							{
								prediction.Position = GameManagerClient.Instance.LocalPositionPredictions[j - 1].Position +
									PlayerMovementCalculations.ReCalculatePlayerPosition(
									GameManagerClient.Instance.LocalPositionPredictions[j].Inputs,
									GameManagerClient.Instance.LocalPositionPredictions[j].TransformRight,
									GameManagerClient.Instance.LocalPositionPredictions[j].TransformForward,
									GameManagerClient.Instance.LocalPositionPredictions[j].yVelocityPreMove,
									GameManagerClient.Instance.LocalPositionPredictions[j].IsGroundedPreMove
									);

								GameManagerClient.Instance.LocalPositionPredictions[j] = new LocalPredictionData(prediction);
							}
						}

						break;
					}

					GameManagerClient.Instance.LocalPositionPredictions.RemoveRange(0, i + 1);
					break;
				}
			}

		}

	}
}
