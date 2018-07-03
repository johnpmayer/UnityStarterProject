﻿using Assets.Gamelogic.Core;
using Improbable;
using Improbable.Core;
using Improbable.Player;
using Improbable.Unity.Core.Acls;
using Improbable.Worker;
using UnityEngine;
using Improbable.Unity.Entity;
using Improbable.Collections;
using Improbable.Island;
using Improbable.Ship;

namespace Assets.Gamelogic.EntityTemplates
{
    public class EntityTemplateFactory : MonoBehaviour
    {
        public static Entity CreatePlayerCreatorTemplate(IComponentData<PlatformPosition> platformPosiionData)
        {
            var playerCreatorEntityTemplate = EntityBuilder.Begin()
                .AddPositionComponent(Coordinates.ZERO.ToUnityVector(), CommonRequirementSets.PhysicsOnly)
                .AddMetadataComponent(entityType: SimulationSettings.PlayerCreatorPrefabName)
                .SetPersistence(true)
                .SetReadAcl(CommonRequirementSets.PhysicsOrVisual)
                .AddComponent(new Rotation.Data(UnityEngine.Quaternion.identity.ToNativeQuaternion()), CommonRequirementSets.PhysicsOnly)
                .AddComponent(new PlayerCreation.Data(), CommonRequirementSets.PhysicsOnly)
                .AddComponent(new ClientEntityStore.Data(new Map<string, EntityId>()), CommonRequirementSets.PhysicsOnly)
                .AddComponent(platformPosiionData, CommonRequirementSets.PhysicsOnly)
                .Build();

            return playerCreatorEntityTemplate;
        }

        public static Entity CreatePlayerTemplate(string clientId, EntityId playerCreatorId, IComponentData<PlatformPosition> platformPosiionData)
        {
            var playerTemplate = EntityBuilder.Begin()
                .AddPositionComponent(Coordinates.ZERO.ToUnityVector(), CommonRequirementSets.PhysicsOnly)
                .AddMetadataComponent(entityType: SimulationSettings.PlayerPrefabName)
                .SetPersistence(false)
                .SetReadAcl(CommonRequirementSets.PhysicsOrVisual)
                .AddComponent(new Rotation.Data(UnityEngine.Quaternion.identity.ToNativeQuaternion()), CommonRequirementSets.PhysicsOnly)
                .AddComponent(new ClientAuthorityCheck.Data(), CommonRequirementSets.SpecificClientOnly(clientId))
                .AddComponent(new ClientConnection.Data(SimulationSettings.TotalHeartbeatsBeforeTimeout, clientId, playerCreatorId), CommonRequirementSets.PhysicsOnly)
                .AddComponent(new WalkControls.Data(new Vector3f(0.0f, 0.0f, 0.0f)), CommonRequirementSets.SpecificClientOnly(clientId))
                .AddComponent(new PilotControls.Data(0.0f, 0.0f), CommonRequirementSets.SpecificClientOnly(clientId))
                .AddComponent(platformPosiionData, CommonRequirementSets.PhysicsOnly)
                .Build();

            return playerTemplate;
        }

        public static Entity CreateIslandTemplate(double x, double z, string name)
        {
            // TODO - Only one Island...
            var position = new Coordinates(x, 0.0f, z).ToUnityVector();

            var islandTemplate = EntityBuilder.Begin()
                .AddPositionComponent(position, CommonRequirementSets.PhysicsOnly)
                .AddMetadataComponent(entityType: SimulationSettings.IslandPrefabName)
                .SetPersistence(true)
                .SetReadAcl(CommonRequirementSets.PhysicsOrVisual)
                .AddComponent(new Rotation.Data(UnityEngine.Quaternion.identity.ToNativeQuaternion()), CommonRequirementSets.PhysicsOnly)
                .AddComponent(new IslandMetadata.Data(name), CommonRequirementSets.PhysicsOnly)
                .AddComponent(new PlatformBounds.Data(-30.0f, 30.0f, -20.0f, 20.0f), CommonRequirementSets.PhysicsOnly)
                .Build();

            return islandTemplate;
        }

        public static Entity CreateShipTemplate(double x, double z, float rotation, string name)
        {
            var position = new Coordinates(x, 0.0f, z).ToUnityVector();
            var rotationQuaternion = UnityEngine.Quaternion.Euler(0.0f, rotation, 0.0f);

            var shipTemplate = EntityBuilder.Begin()
                .AddPositionComponent(position, CommonRequirementSets.PhysicsOnly)
                .AddMetadataComponent(entityType: SimulationSettings.ShipPrefabName)
                .SetPersistence(true)
                .SetReadAcl(CommonRequirementSets.PhysicsOrVisual)
                .AddComponent(new Rotation.Data(rotationQuaternion.ToNativeQuaternion()), CommonRequirementSets.PhysicsOnly)
                .AddComponent(new ShipMetadata.Data(name), CommonRequirementSets.PhysicsOnly)
                .AddComponent(new PlatformBounds.Data(-5.0f, 5.0f, -3.0f, 3.0f), CommonRequirementSets.PhysicsOnly)
                .Build();

            return shipTemplate;
        }
    }
}
