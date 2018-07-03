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

namespace Assets.Gamelogic.Player
{

    [WorkerType(WorkerPlatform.UnityClient)]
    public class PlayerInputController : MonoBehaviour
    {
        interface IOnPlatform {}

        class OnIsland : IOnPlatform {} // TODO Entity ID

        class OnShip : IOnPlatform {} // TODO Entity ID

        interface ISelection {}

        class SelectedNothing : ISelection { }
        
        class SelectedShip : ISelection
        {
            public GameObject ShipObject { get; set; }
            public ShipMetadataReceiver ShipMetadataReceiver { get; set; }

            public SelectedShip(GameObject shipObject, ShipMetadataReceiver shipMetadataReceiver) 
            {
                ShipObject = shipObject;
                ShipMetadataReceiver = shipMetadataReceiver;
            }

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

            public SelectedIsland(GameObject islandObject, IslandMetadataReceiver islandMetadataReceiver)
            {
                IslandObject = islandObject;
                IslandMetadataReceiver = islandMetadataReceiver;
            }

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

        private void OnEnable()
        {
            _mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
			_userMessage = GameObject.FindGameObjectWithTag("UserMessage").GetComponent<Text>();
        }

        [Require] private WalkControls.Writer WalkControlsWriter;

        private void Update()
        {
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
                            _currentSelection = new SelectedShip(
                                shipObject: hitGameObject,
                                shipMetadataReceiver: shipMetadata);
                            break;

                        case SimulationSettings.IslandTagName:
                            var islandMetadata = hitGameObject.GetComponent<IslandMetadataReceiver>();
                            Debug.Log("Clicked on an island: " + islandMetadata.IslandName());
                            _currentSelection = new SelectedIsland(
                                islandObject: hitGameObject,
                                islandMetadataReceiver: islandMetadata);
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
                }
            }
        }

    }
}