using UnityEngine;
using Improbable.Unity;
using Improbable.Unity.Visualizer;
using Assets.Gamelogic.Core;
using Assets.Gamelogic.Ship;
using Assets.Gamelogic.Island;

namespace Assets.Gamelogic.ClientManager
{
    [WorkerType(WorkerPlatform.UnityClient)]
    public class ActionRouter : MonoBehaviour
    {
        public Camera mainCamera;

        // Update is called once per frame
        private void Update()
        {
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
                            var shipMetadata = gameObject.GetComponent<ShipMetadataReceiver>();
                            Debug.Log("Clicked on a ship: " + shipMetadata.ShipName());
                            break;

                        case SimulationSettings.IslandTagName:
                            var islandMetadata = gameObject.GetComponent<IslandMetadataReceiver>();
                            Debug.Log("Clicked on an island: " + islandMetadata.IslandName());
                            break;
                    }
                }
            }
        }
    }
}