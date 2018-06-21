using Improbable;
using Improbable.Unity;
using Improbable.Player;
using Improbable.Unity.Visualizer;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Gamelogic.Player {

	[WorkerType(WorkerPlatform.UnityWorker)]
	public class WalkMovement : MonoBehaviour {

		[Require] private Position.Writer PositionWriter;
        [Require] protected WalkControls.Reader WalkControlsReader;

		// private Vector3 targetVelocity;
		// private Vector3 currentVelocity;

		private void OnEnable()
		{
			transform.position = PositionWriter.Data.coords.ToUnityVector();
		}

		public float tickSpeed;

		public void FixedUpdate()
		{
			var deltaTime = Time.deltaTime;
			var targetVelocity = WalkControlsReader.Data.targetSpeed.ToUnityVector();
			var clampedVelocity = Vector3.ClampMagnitude(targetVelocity, 1.0f);
			transform.position += clampedVelocity * tickSpeed;
			PositionWriter.Send(new Position.Update().SetCoords(transform.position.ToCoordinates()));
		}

	}

	public static class Vector3Extensions
    {
        public static Coordinates ToCoordinates(this Vector3 vector3)
        {
            return new Coordinates(vector3.x, vector3.y, vector3.z);
        }
    }

}