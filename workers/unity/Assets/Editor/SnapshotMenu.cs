using Assets.Gamelogic.Core;
using Assets.Gamelogic.EntityTemplates;
using Improbable;
using Improbable.Worker;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Assets.Editor
{
	public class SnapshotMenu : MonoBehaviour
	{
		[MenuItem("Improbable/Snapshots/Generate Default Snapshot")]
		private static void GenerateDefaultSnapshot()
		{
			var snapshotEntities = new Dictionary<EntityId, Entity>();
			var currentEntityId = 1;

			snapshotEntities.Add(new EntityId(currentEntityId++), EntityTemplateFactory.CreatePlayerCreatorTemplate());

			BuildTiles (ref snapshotEntities, ref currentEntityId);

			SaveSnapshot(snapshotEntities);
		}

		private static void SaveSnapshot(IDictionary<EntityId, Entity> snapshotEntities)
		{
			File.Delete(SimulationSettings.DefaultSnapshotPath);
			using (SnapshotOutputStream stream = new SnapshotOutputStream(SimulationSettings.DefaultSnapshotPath))
			{
				foreach (var kvp in snapshotEntities)
				{
					var error = stream.WriteEntity(kvp.Key, kvp.Value);
					if (error.HasValue)
					{
						Debug.LogErrorFormat("Failed to generate initial world snapshot: {0}", error.Value);
						return;
					}
				}
			}

			Debug.LogFormat("Successfully generated initial world snapshot at {0}", SimulationSettings.DefaultSnapshotPath);
		}

		private static void BuildTiles(ref Dictionary<EntityId, Entity> snapshotEntities, ref int currentEntityId)
		{
			// TODO move magic constants to settings

			for (var offsetX = -20; offsetX <= 20; offsetX += 1) {
				for (var offsetZ = -20; offsetZ <= 20; offsetZ += 1) {

					if (offsetX + offsetZ > 20 || offsetX + offsetZ < 20)
						continue;

					snapshotEntities.Add(new EntityId(currentEntityId++), EntityTemplateFactory.CreateTileTemplate(offsetX, offsetZ));
				}
			}

		}
	}
}
