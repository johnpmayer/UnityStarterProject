using Improbable.Unity;
using Improbable.Unity.Visualizer;
using Improbable.Core;
using Improbable.Ship;
using Improbable;
using UnityEngine;

namespace Assets.Gamelogic.Ship
{
    [WorkerType(WorkerPlatform.UnityWorker)]
    public class PilotMovement : MonoBehaviour
    {
        // TODO - https://www.marineinsight.com/naval-architecture/rudder-ship-turning/

        [Require] private Position.Writer PositionWriter;
        [Require] private Rotation.Writer RotationWriter;

        private float _targetThrotttle; // positive means forward
        private float _targetRudder; // positive means turn starboard (right)

        private Rigidbody rb;

        private float maxPropellorForce = 5f;
        private float maxRudderForce = 0.01f;
        private float baseSwayForce = 10f;

        private const float shipHalfLength = 7f;
        private const float keelCenterOffsetDistance = 0.5f;

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
            //Debug.LogWarning(string.Format("Set controls on ship behavior {0}, {1}", _targetRudder, _targetThrotttle));
        }

        delegate Vector3 Vector3Transform(Vector3 input);

        public void FixedUpdate()
        {
            // Local names
            var bow = Vector3.right;
            var stern = Vector3.left;
            var port = Vector3.forward;
            var starboard = Vector3.back;

            // Local Constants
            Vector3 propellorOffset = shipHalfLength * stern;
            Vector3 rudderOffset = shipHalfLength * stern;
            Vector3 keelCenterOffset = keelCenterOffsetDistance * bow;

            Vector3Transform fromShipLocal = (input) =>
                rb.position + rb.rotation * input;

            // Absolute Tick-Constants
            Vector3 propellorPosition = fromShipLocal(propellorOffset);
            Vector3 rudderPosition = fromShipLocal(rudderOffset);
            Vector3 keelCenterPosition = fromShipLocal(keelCenterOffset);
            Vector3 relativeShipVelocity = UnityEngine.Quaternion.Inverse(rb.rotation) * rb.velocity;

            // Positive means moving to bow, water approaching from bow
            // Negative means moving to stern, water approaching from stern
            float surgeVelocityX = relativeShipVelocity.x;

            // Positive means swaying to port, water approaching from port
            // Negative means swaying to starboard, water approaching from starboard
            float swayVelocityZ = relativeShipVelocity.z;

            // Propellor
            Vector3 propellorForce = rb.rotation * (_targetThrotttle * maxPropellorForce * bow);
            rb.AddForceAtPosition(propellorForce, propellorPosition);

            // Rudder
            /* 
             * Considering the case where pilot intends to turn the ship right.
             * 
             * If surge is positive (moving forward) and rudder is positive 
             * (intent to turn right) then we want to sway the stern to port. 
             * So the rudder force, with the rudder being at the stern, should 
             * be towards port. 
             */
            Vector3 rudderForce = rb.rotation * (_targetRudder * maxRudderForce * surgeVelocityX * port);
            rb.AddForceAtPosition(rudderForce, rudderPosition);

            // Keel
            /*
             * Considering the case where pilot intends to turn the ship right.
             * 
             * If sway is positive (swaying to port, water approaching from 
             * port), the keel force is towards starboard.
             */
            Vector3 keelForce = rb.rotation * (baseSwayForce * swayVelocityZ * starboard);
            rb.AddForceAtPosition(keelForce, keelCenterPosition);

            // TODO! need to take into account rotational intertia!!!
        }

        public void Update()
        {
            PositionWriter.Send(new Position.Update().SetCoords(gameObject.transform.position.ToCoordinates()));
            RotationWriter.Send(new Rotation.Update().SetRotation(gameObject.transform.rotation.ToNativeQuaternion()));
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