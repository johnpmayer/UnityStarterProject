using Improbable.Unity;
using Improbable.Unity.Visualizer;
using Improbable.Core;
using Improbable.Ship;
using Improbable;
using UnityEngine;
using Improbable.Worker;
using Improbable.Worker.Query;
using Improbable.Entity.Component;

namespace Assets.Gamelogic.Ship
{
    [WorkerType(WorkerPlatform.UnityWorker)]
    public class PilotReservation : MonoBehaviour
    {
        [Require] private ActivePilot.Writer ActivePilotWriter;

        private void OnEnable()
        {
            ActivePilotWriter.CommandReceiver.OnPilot.RegisterResponse(OnPilot);
        }

        private void OnDisable()
        {
            ActivePilotWriter.CommandReceiver.OnPilot.DeregisterResponse();
        }

        private PilotResponse OnPilot(PilotRequest request, ICommandCallerInfo callerInfo) {
            EntityId currentPilot;
            if (ActivePilotWriter.Data.playerId.TryGetValue(out currentPilot)) {
                return new PilotResponse(false);
            } else {
                ActivePilotWriter.Send(new ActivePilot.Update().SetPlayerId(request.playerId));
                return new PilotResponse(true);
            }
        }
    }
}