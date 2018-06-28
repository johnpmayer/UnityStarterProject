using Improbable;
using Improbable.Core;
using Improbable.Entity.Component;
using Improbable.Unity;
using Improbable.Player;
using Improbable.Unity.Visualizer;
using Improbable.Worker;
using Improbable.Worker.Query;
using UnityEngine;
using UnityEngine.SceneManagement;
using Improbable.Unity.Core;
using Improbable.Unity.Entity;

// TODO - call this platform movement and move to Core?

namespace Assets.Gamelogic.Player {

	[WorkerType(WorkerPlatform.UnityWorker)]
	public class WalkMovement : MonoBehaviour {

		[Require] private Position.Writer PositionWriter;
		[Require] private PlatformPosition.Writer PlatformPositionWriter;

        [Require] private WalkControls.Reader WalkControlsReader;

		private Vector3 _localPosition;
		private PlatformBoundsData _cachedPlatformBoundsData;
		private GameObject _platformObject;

		private void OnEnable()
		{
			PlatformPositionWriter.PlatformEntityUpdated.AddAndInvoke(OnPlatformEntityUpdated);
			PlatformPositionWriter.CommandReceiver.OnBoard.RegisterResponse(OnBoard);
		}

		private void OnDisable()
		{
			PlatformPositionWriter.PlatformEntityUpdated.Remove(OnPlatformEntityUpdated);
		}

		private BoardResponse OnBoard(BoardRequest request, ICommandCallerInfo callerInfo) {
			if (!LocalEntities.Instance.ContainsEntity(request.platformEntityId)) {
				Debug.LogError("The platform entity isn't being tracked locally...");
				// todo probably should throw here instead
			}

			var targetObject = LocalEntities.Instance.Get(request.platformEntityId).UnderlyingGameObject;

			// TODO - in-range and other validation

			PlatformPositionWriter.Send(
				new PlatformPosition.Update()
				.SetLocalPosition(new Vector3f(0,0,0))
				.SetPlatformEntity(request.platformEntityId));

			return new BoardResponse(); // TODO - richer status?
		}

		private void FetchAndCachePlatformBounds(EntityId entityId)
		{
			var PlatformBoundsComponent = SpatialOS.GetLocalEntityComponent<PlatformBounds>(entityId);
			if (PlatformBoundsComponent ==  null) {
				Debug.LogError("Unable to find platform bounds component for entity: " + entityId.Id);
			}
			_cachedPlatformBoundsData = PlatformBoundsComponent.Get().Value;
		}

		private void AttachLocalPlatformObject(EntityId entityId)
		{
			if (!LocalEntities.Instance.ContainsEntity(entityId)) {
				Debug.LogError("The platform entity isn't being tracked locally...");
				// todo probably should throw here instead
			}
			GameObject _platformObject = LocalEntities.Instance.Get(entityId).UnderlyingGameObject;
			gameObject.transform.SetParent(_platformObject.transform);
		}

		private void UpdateRelativeTransform()
		{
			PlatformPositionData curr = PlatformPositionWriter.Data;
			_localPosition = curr.localPosition.ToUnityVector();
			gameObject.transform.localPosition = _localPosition;
		}

		private void OnPlatformEntityUpdated(EntityId entityId)
		{
			FetchAndCachePlatformBounds(entityId);
			AttachLocalPlatformObject(entityId);
			UpdateRelativeTransform();
		}

		public float tickSpeed;

		public void FixedUpdate()
		{
			// FIXME FIXME - update the platform relative position first
			var deltaTime = Time.deltaTime;
			var targetVelocity = WalkControlsReader.Data.targetSpeed.ToUnityVector();
			targetVelocity.y = 0.0f;
			var clampedVelocity = Vector3.ClampMagnitude(targetVelocity, 1.0f);

			_localPosition += clampedVelocity;
			_localPosition.x = Mathf.Clamp(_localPosition.x, _cachedPlatformBoundsData.minX, _cachedPlatformBoundsData.maxX);
			_localPosition.y = 0.0f;
			_localPosition.z = Mathf.Clamp(_localPosition.z, _cachedPlatformBoundsData.minZ, _cachedPlatformBoundsData.maxZ);

			transform.localPosition = _localPosition;
			
			PlatformPositionWriter.Send(new PlatformPosition.Update().SetLocalPosition(_localPosition.ToNativeVector()));
			
			// send the absolute position... actually looks like this is OK already
			PositionWriter.Send(new Position.Update().SetCoords(transform.position.ToCoordinates()));
		}

	}

	public static class Vector3Extensions
    {
        public static Coordinates ToCoordinates(this Vector3 vector3)
        {
            return new Coordinates(vector3.x, vector3.y, vector3.z);
        }

		public static Vector3f ToNativeVector(this Vector3 vector3)
        {
            return new Vector3f(vector3.x, vector3.y, vector3.z);
        }
    }

}