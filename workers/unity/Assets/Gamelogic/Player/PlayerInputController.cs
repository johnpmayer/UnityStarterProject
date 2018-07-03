using Improbable;
using Improbable.Unity;
using Improbable.Core;
using Improbable.Island;
using Improbable.Ship;
using Improbable.Player;
using Improbable.Unity.Visualizer;
using UnityEngine;
using UnityEngine.SceneManagement;
using Improbable.Unity.Core;
using Assets.Gamelogic.Player;
using UnityEngine.UI;
using Assets.Gamelogic.Ship;
using Assets.Gamelogic.Island;
using Assets.Gamelogic.Core;
using System;
using System.Collections;

namespace Assets.Gamelogic.Player
{

    [WorkerType(WorkerPlatform.UnityClient)]
    public class PlayerInputController : MonoBehaviour
    {
        interface IOnPlatform {}

        class OnIsland : IOnPlatform {
            public EntityId IslandId { get; set; }

            public string UserStatus() {
                return string.Format("On Island {0}.",
                    IslandId
                );
            }
        }

        class OnShip : IOnPlatform {
            public EntityId ShipId { get; set; }
            public bool IsPilot { get; set; } 

            public string UserStatus() {
                var pilotMessage = IsPilot ? "Piloting." : "Press 'P' to pilot.";
                return string.Format("On Ship {0}. {1}",
                    ShipId, pilotMessage
                );
            }
        }

        interface ISelection {}

        class SelectedNothing : ISelection { }
        
        class SelectedShip : ISelection
        {
            public GameObject ShipObject { get; set; }
            public ShipMetadataReceiver ShipMetadataReceiver { get; set; }

            public double DistanceFromPlayer(GameObject playerObject)
            {
                var displacement = ShipObject.transform.position - playerObject.transform.position;
                return displacement.magnitude;
            }

            public string UserMessage(GameObject playerObject)
            {
                var distanceFromPlayer = DistanceFromPlayer(playerObject);
                var boardMessage = (distanceFromPlayer < 10) ? "; Press 'B' to board." : ".";
                return string.Format("Selected {0}. It is {1} from the player{2}",
                    ShipMetadataReceiver.ShipName(),
                    distanceFromPlayer,
                    boardMessage);
            }

        }

        class SelectedIsland : ISelection
        {
            public GameObject IslandObject { get; set; }
            public IslandMetadataReceiver IslandMetadataReceiver { get; set; }

            public string UserMessage()
            {
                return string.Format("Selected {0}", IslandMetadataReceiver.IslandName());
            }
        }

		// State
		private ISelection _currentSelection;
		private IOnPlatform _currentPlatform;

		// Component References
        private Camera _mainCamera;
		private Text _userMessage;
        private Text _userStatus;

        private bool _initialized = false;

        private bool SetOnPlatform(EntityId entityId) {
            var metaComponent = SpatialOS.GetLocalEntityComponent<Metadata>(entityId);
                if (metaComponent == null) {
                    Debug.Log("Local entities not available, no metadata component...");
                    return false;
                } else {
                    Debug.Log("Got platform metadata component!");

                    string platformType = metaComponent.Get().Value.entityType;
                    if (platformType == SimulationSettings.IslandTagName) {
                        _currentPlatform = new OnIsland {
                            IslandId = entityId,
                        };
                    } else if (platformType == SimulationSettings.ShipTagName) {
                        _currentPlatform = new OnShip {
                            ShipId = entityId,
                            IsPilot = false,
                        };
                    } else {
                        throw new Exception("Invalid component state, platform neither island nor ship");
                    }

                    return true;
                }
        }

        private IEnumerator PollForPlatform() {
            while(true) {
                Debug.Log("Polling for platform");
                var platformEntityId = PlatformPositionReader.Data.platformEntity;
                if (SetOnPlatform(platformEntityId)) {
                    _initialized = true;
                    yield break;
                } else {
                    yield return new WaitForSeconds(.25f);
                }
            }
        }

        private void OnEnable()
        {
            _mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();

			_userMessage = GameObject.FindGameObjectWithTag("UserMessage").GetComponent<Text>();
            _userMessage.text = "";

            _userStatus = GameObject.FindGameObjectWithTag("UserStatus").GetComponent<Text>();
            _userStatus.text = "Initializing...";

            PlatformPositionReader.PlatformEntityUpdated.Add(OnPlatformEntityUpdated);

            StartCoroutine("PollForPlatform");
        }

        private void OnDisable()
        {
            PlatformPositionReader.PlatformEntityUpdated.Remove(OnPlatformEntityUpdated);
        }

        private void OnPlatformEntityUpdated(EntityId newPlatformId) {
            _initialized = false; // freeze user input
            StartCoroutine("PollForPlatform"); // kind of janky
        }

        [Require] private WalkControls.Writer WalkControlsWriter;
        [Require] private PlatformPosition.Reader PlatformPositionReader;

        private void Update()
        {
            if (!_initialized) {
                return;
            }
            UpdateSelection();
			UpdateWalkControls();
			UpdateActionControls();
			UpdateUI();
        }

        private void UpdateActionControls()
        {
            if (_currentSelection is SelectedShip) {
                var ship = (SelectedShip)_currentSelection;
                if (Input.GetKeyDown(KeyCode.B))
                {
                    TryBoard(ship.ShipObject.EntityId());
                }
            }

        }

		private void UpdateWalkControls()
		{
			float targetX = Input.GetAxis("Horizontal");
            float targetZ = Input.GetAxis("Vertical");
            WalkControlsWriter.Send(new WalkControls.Update()
                .SetTargetSpeed(new Improbable.Vector3f(targetX, 0.0f, targetZ)));
		}

        private void UpdateSelection()
        {
            if (Input.GetMouseButtonDown(0)) // left clieck
            {
                Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    var hitGameObject = hit.collider.gameObject;
                    switch (hitGameObject.tag)
                    {
                        case SimulationSettings.ShipTagName:
                            var shipMetadata = hitGameObject.GetComponent<ShipMetadataReceiver>();
                            Debug.Log("Clicked on a ship: " + shipMetadata.ShipName());
                            _currentSelection = new SelectedShip {
                                ShipObject = hitGameObject,
                                ShipMetadataReceiver = shipMetadata,
                            };
                            break;

                        case SimulationSettings.IslandTagName:
                            var islandMetadata = hitGameObject.GetComponent<IslandMetadataReceiver>();
                            Debug.Log("Clicked on an island: " + islandMetadata.IslandName());
                            _currentSelection = new SelectedIsland {
                                IslandObject = hitGameObject,
                                IslandMetadataReceiver = islandMetadata,
                            };
                            break;
                    }
                }
                else
                {
                    _currentSelection = new SelectedNothing();
                }
            }
        }

        public void TryBoard(EntityId platformEntityId)
        {
            SpatialOS.Commands.SendCommand(
                WalkControlsWriter,
                PlatformPosition.Commands.Board.Descriptor,
                new BoardRequest(platformEntityId),
                gameObject.EntityId())
                .OnSuccess(OnBoardSuccess)
                .OnFailure(OnBoardFailure);
        }

        public void TryPilot(EntityId shipEntityId)
        {

        }

        private void OnBoardSuccess(BoardResponse response)
        {
            Debug.Log("Board success");
            _currentSelection = new SelectedNothing();
        }

        private void OnBoardFailure(ICommandErrorDetails response)
        {
            Debug.LogError("Board failed: " + response.ErrorMessage);
        }

		private void UpdateUI()
        {
            if (_userMessage != null)
            {
                if (_currentSelection is SelectedShip) {
                    var ship = (SelectedShip)_currentSelection;
                    _userMessage.text = ship.UserMessage(gameObject);
                } else if (_currentSelection is SelectedIsland) {
                    var island = (SelectedIsland)_currentSelection;
                    _userMessage.text = island.UserMessage();
                } else if (_currentSelection is SelectedNothing) {
                    _userMessage.text = "";
                }
            }

            if (_userStatus != null)
            {
                if (_currentPlatform is OnShip) {
                    var ship = (OnShip)_currentPlatform;
                    _userStatus.text = ship.UserStatus();
                } else if (_currentPlatform is OnIsland) {
                    var island = (OnIsland)_currentPlatform;
                    _userStatus.text = island.UserStatus();
                }
            }
        }

    }
}