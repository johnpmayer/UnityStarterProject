using UnityEngine;
using Improbable.Unity;
using Improbable.Unity.Visualizer;
using Assets.Gamelogic.Core;
using Assets.Gamelogic.Ship;
using Assets.Gamelogic.Island;
using UnityEngine.UI;

namespace Assets.Gamelogic.ClientManager
{
    interface ISelection
    {
        string UserMessage();
    }

    class SelectedNothing : ISelection
    {
        public string UserMessage()
        {
            return "";
        }
    }

    class SelectedShip : ISelection
    {
        private readonly GameObject _playerObject;
        private readonly GameObject _shipObject;
        private readonly ShipMetadataReceiver _shipMetadataReceiver;

        public SelectedShip(GameObject playerObject, GameObject shipObject, ShipMetadataReceiver shipMetadataReceiver)
        {
            _playerObject = playerObject;
            _shipObject = shipObject;
            _shipMetadataReceiver = shipMetadataReceiver;
        }

        public double DistanceFromPlayer()
        {
            var displacement = _shipObject.transform.position - _playerObject.transform.position;
            return displacement.magnitude;
        }
        
        public string UserMessage()
        {
            var distanceFromPlayer = DistanceFromPlayer();
            var boardMessage = (distanceFromPlayer < 10) ? "Press 'B' to board." : "";
            return string.Format("Selected {0}. It is {1} from the player. {2}", 
                _shipMetadataReceiver.ShipName(),
                DistanceFromPlayer(),
                boardMessage);
        }
    }

    class SelectedIsland : ISelection
    {
        private readonly GameObject _gameObject;
        private readonly IslandMetadataReceiver _islandMetadataReceiver;

        public SelectedIsland(GameObject gameObject, IslandMetadataReceiver islandMetadataReceiver)
        {
            _gameObject = gameObject;
            _islandMetadataReceiver = islandMetadataReceiver;
        }

        public string UserMessage()
        {
            return string.Format("Selected {0}", _islandMetadataReceiver.IslandName());
        }
    }

    [WorkerType(WorkerPlatform.UnityClient)]
    public class ActionRouter : MonoBehaviour
    {
        public Camera mainCamera;
        public Text userMessage;

        private GameObject _playerGameObject;
        private ISelection _currentSelection = new SelectedNothing();

        public void RegisterPlayerObject(GameObject gameObject)
        {
            _playerGameObject = gameObject;
            Debug.Log(string.Format("Registered player object {0}", _playerGameObject));
        }

        private void Update()
        {
            UpdateSelection();
            UpdateUI();
        }

        private void UpdateSelection() {
            if (Input.GetMouseButtonDown(0)) // left clieck
            {
                Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    var gameObject = hit.collider.gameObject;
                    switch (gameObject.tag)
                    {
                        case SimulationSettings.ShipTagName:
                            // var shipEntityId = gameObject.GetComponent<EntityId>();
                            var shipMetadata = gameObject.GetComponent<ShipMetadataReceiver>();
                            Debug.Log("Clicked on a ship: " + shipMetadata.ShipName());
                            _currentSelection = new SelectedShip(
                                playerObject: _playerGameObject, 
                                shipObject: gameObject, 
                                shipMetadataReceiver: shipMetadata);
                            break;

                        case SimulationSettings.IslandTagName:
                            var islandMetadata = gameObject.GetComponent<IslandMetadataReceiver>();
                            Debug.Log("Clicked on an island: " + islandMetadata.IslandName());
                            _currentSelection = new SelectedIsland(
                                gameObject: gameObject, 
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

        private void UpdateUI()
        {
            if (userMessage != null)
            {
                userMessage.text = _currentSelection.UserMessage();
            }
        }
    }
}