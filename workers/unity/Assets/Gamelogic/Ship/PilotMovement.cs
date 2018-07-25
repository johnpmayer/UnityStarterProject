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

        private float _targetThrotttle; // positive means forward
        private float _targetRudder; // positive means turn starboard (right)

        private Rigidbody rb;

        public float maxPropellorForce;
        public float maxRudderForce;

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
            Debug.LogWarning(string.Format("Set controls on ship behavior {0}, {1}", _targetRudder, _targetThrotttle));
        }

        public void FixedUpdate()
        {
            // TODO all magic numbers (excluding obvious zeroes)

            var propellorForce = new Vector3(_targetThrotttle * maxPropellorForce, 0.0f, 0.0f);
            var propellorPosition = new Vector3(0.0f, 0.0f, 0.0f); // Should put this at the "back"
            rb.AddForceAtPosition(propellorForce, propellorPosition);

            var rudderForce = new Vector3(0.0f, 0.0f, _targetRudder * maxRudderForce);
            var rudderPosition = new Vector3(-7.0f, 0.0f, 0.0f); // ship is 14 long, so half that for now
            rb.AddForceAtPosition(rudderForce, rudderPosition);

            // TODO sway force & moment - should be applied somewhere in front of center of mass
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