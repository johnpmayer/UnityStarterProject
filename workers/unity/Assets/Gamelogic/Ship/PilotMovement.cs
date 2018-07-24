using Improbable.Unity;
using Improbable.Unity.Visualizer;
using Improbable.Core;
using Improbable.Ship;
using Improbable;
using UnityEngine;
using Improbable.Worker;
using Improbable.Worker.Query;

namespace Assets.Gamelogic.Ship
{
    [WorkerType(WorkerPlatform.UnityWorker)]
    public class PilotMovement : MonoBehaviour
    {
        // TODO - https://www.marineinsight.com/naval-architecture/rudder-ship-turning/

        [Require] private Position.Writer PositionWriter;

        private float _targetThrotttle;
        private float _targetRudder;

        private Rigidbody rb;

        private void OnEnable()
        {
            rb = gameObject.GetComponent<Rigidbody>();
        }

        public void SetControls(PilotControls.Update update) {
            if (update.rudder.HasValue) {
                _targetRudder = update.rudder.Value;
            }
            if (update.propellor.HasValue) {
                _targetThrotttle = update.propellor.Value;
            }
            Debug.LogWarning("TODO: Setting controls on ship behavior");
        }

        public void FixedUpdate()
        {
            var force = new Vector3(_targetThrotttle, 0.0f, 0.0f);
            var position = new Vector3(0.0f, 0.0f, 0.0f);
            rb.AddForceAtPosition(force, position);
        }

        public void Update()
        {
            PositionWriter.Send(new Position.Update().SetCoords(gameObject.transform.position.ToCoordinates()));
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