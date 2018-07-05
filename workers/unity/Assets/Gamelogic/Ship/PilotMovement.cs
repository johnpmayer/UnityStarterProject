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
        [Require] private Position.Writer PositionWriter;

        public void SetControls(PilotControls.Update update) {
            Debug.LogWarning("TODO: Setting controls on ship behavior");
        }
    }
}