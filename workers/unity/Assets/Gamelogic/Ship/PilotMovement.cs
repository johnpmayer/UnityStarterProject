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

        public float maxPropellorForce;
        public float maxRudderForce;
        public float baseSwayForce;

        private const float shipHalfLength = 7.0f;
        private const float bowSternSwayOffset = 1.0f;

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
            Vector3 propellorOffset = 6.0f * stern;
            Vector3 rudderOffset = 7.0f * stern;
            Vector3 effectiveSwayOffset = 2.0f * bow;

            Vector3Transform fromShipLocal = (input) =>
                rb.position + rb.rotation * input;

            // Absolute Tick-Constants
            Vector3 propellorPosition = fromShipLocal(propellorOffset);
            Vector3 rudderPosition = fromShipLocal(rudderOffset);
            Vector3 effectiveSwayPosition = fromShipLocal(effectiveSwayOffset);
            Vector3 shipRelativeVelocity = UnityEngine.Quaternion.Inverse(rb.rotation) * -rb.velocity;

            // Positive means water approaching from stern (back)
            // Negative means water approaching from bow (front)
            float surgeVelocityX = shipRelativeVelocity.x;

            // Positive means water approaching from starboard (right)
            // Negative means water approaching from port (left)
            float swayVelocityY = shipRelativeVelocity.y; 

            // Propellor
            Vector3 propellorForce = rb.rotation * (_targetThrotttle * maxPropellorForce * bow);
            rb.AddForceAtPosition(propellorForce, propellorPosition);

            // Rudder
            /* If surge is negative (moving forward) and rudder is positive 
             * (intent to turn right) then we want to sway the stern to port. 
             * So the rudder force, with the rudder being at the stern, should 
             * be towards port. 
             */
            Vector3 rudderForce = rb.rotation * (_targetRudder * maxRudderForce * surgeVelocityX * starboard);
            rb.AddForceAtPosition(rudderForce, rudderPosition);

            // TODO sway force proportional to sway component of velocity

            //var swayForceMagnitude = baseSwayForce * swayVelocityMagnitude;
            //var swayForce = new Vector3(0.0f, 0.0f, swayForceMagnitude);
            //var swayPosition = new Vector3(bowSternSwayOffset, 0.0f, 0.0f); // A bit in front of the center-of-mass
            //rb.AddForceAtPosition(swayForce, swayPosition);
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