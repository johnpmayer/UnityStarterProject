using Improbable;
using Improbable.Unity.Visualizer;
using Improbable.Cube;
using Improbable.Unity;
using UnityEngine;

namespace Assets.Gamelogic.Cube
{
    [WorkerType(WorkerPlatform.UnityWorker)]
    public class Orbiter : MonoBehaviour
    {
        public float radius;

        [Require] private Position.Writer PositionWriter;
        [Require] private CubeTick.Writer CubeTickWriter;

        private void Start()
        {
            Debug.Log("Started an Orbiter behavior");
        }

        private void OnEnable()
        {
            Debug.Log("Enabled an Orbiter behavior");
        }

        // Update is called once per frame
        void Update()
        {
            var lastTick = CubeTickWriter.Data.tick;
            var nextTick = lastTick + 1 % 48;
            CubeTickWriter.Send(new CubeTick.Update().SetTick(nextTick));

            var angle = nextTick * Mathf.PI / 24.0f;
            var newX = radius * Mathf.Cos(angle);
            var newZ = radius * Mathf.Sin(angle);
            PositionWriter.Send(new Position.Update().SetCoords(new Coordinates(newX, 0.0d, newZ)));
        }
    }
}