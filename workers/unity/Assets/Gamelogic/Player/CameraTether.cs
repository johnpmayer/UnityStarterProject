using Assets.Gamelogic.Core;
using Assets.Gamelogic.Utils;
using Improbable.Core;
using Improbable.Player;
using Improbable.Unity;
using Improbable.Unity.Core;
using Improbable.Unity.Visualizer;
using UnityEngine;
using System.Collections;

namespace Assets.Gamelogic.Player
{
    [WorkerType(WorkerPlatform.UnityClient)]
    public class CameraTether : MonoBehaviour
    {
        // Only enable on the player we control
        [Require]
        private ClientAuthorityCheck.Writer ClientAuthorityCheckWriter;

        public Vector3 fixedOffset;

        private GameObject mainCamera;

        private void Start()
        {
            mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            if (mainCamera == null) {
                Debug.LogError("Did not get a reference to the main camera");
            }
        }

		private void Update()
		{
            var cameraPosition = this.transform.position + fixedOffset;
            var cameraRotation = UnityEngine.Quaternion.LookRotation(-fixedOffset, new Vector3(0,1,0));

            mainCamera.transform.SetPositionAndRotation(cameraPosition, cameraRotation);
		}

	}
}