using Improbable.Unity;
using Improbable.Unity.Visualizer;
using Improbable.Core;
using Improbable;
using UnityEngine;
using Improbable.Ship;
using Improbable.Player;
using Improbable.Unity.Core;
using Improbable.Worker;
using Improbable.Entity.Component;
using UnityEngine.SceneManagement;
using Improbable.Unity.Entity;
using System.Collections;

namespace Assets.Gamelogic.Ship
{
    [WorkerType(WorkerPlatform.UnityWorker)]
    public class PilotController : MonoBehaviour
    {
        [Require] PilotControls.Reader PilotControlsReader;
        [Require] PlatformPosition.Reader PlatformPositionReader;

        private IState _state = new RunningToTheWheel();

        private void OnEnable()
        {
            PilotControlsReader.ComponentUpdated.Add(OnPilotControls);
            PilotControlsReader.ActiveUpdated.Add(OnIsActive);

            // TODO how should it listen for PlatformPositionReader? 
            // PilotControls should go inactive... what behavior is authority?
        }

        private void OnDisable()
        {
            PilotControlsReader.ActiveUpdated.Remove(OnIsActive);
            PilotControlsReader.ComponentUpdated.Remove(OnPilotControls);
        }

        private void OnIsActive(bool isActive) {
            if (isActive)
            {
                Debug.LogWarning("PilotController: Running to the helm");
                _state = new RunningToTheWheel
                {
                    ShipId = PlatformPositionReader.Data.platformEntity,
                };
                StartCoroutine("PollForHelm");
            } else {
                _state = new Inactive();
            }
        }

        private void OnPilotControls(PilotControls.Update update) {
            if (_state is AtTheHelm) {
                var helm = (AtTheHelm)_state;
                //Debug.Log("Setting pilot controls on player behavior");
                helm.PilotMovementBehavior.SetControls(update);
            }
        }

        private IEnumerator PollForHelm()
        {
            // TODO - these while(true) should actually be a sort of retry-N, crashing if it times out
            while(_state is RunningToTheWheel) {
                Debug.LogWarning("Polling for the ship to helm");
                var helmPoller = (RunningToTheWheel)_state;
                var shipEntity = LocalEntities.Instance.Get(helmPoller.ShipId);
                if (shipEntity == null) {
                    yield return new WaitForSeconds(.25f);
                } else {
                    var shipObject = shipEntity.UnderlyingGameObject;
                    var pilotMovement = shipObject.GetComponent<PilotMovement>();
                    _state = new AtTheHelm
                    {
                        ShipId = helmPoller.ShipId,
                        ShipObject = shipObject,
                        PilotMovementBehavior = pilotMovement,
                    };
                }
            }
            yield break;
        }

        interface IState {}
        class Inactive : IState {}
        class RunningToTheWheel: IState {
            public EntityId ShipId { get; set; }
        }
        class AtTheHelm : IState
        {
            public EntityId ShipId { get; set; }
            public GameObject ShipObject { get; set; }
            public PilotMovement PilotMovementBehavior { get; set; }
        }
    }
}