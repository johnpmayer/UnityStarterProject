using Improbable.Unity;
using Improbable.Player;
using Improbable.Unity.Visualizer;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Gamelogic.Player {

	[WorkerType(WorkerPlatform.UnityClient)]
	public class PlayerInputController : MonoBehaviour {

		[Require] private WalkControls.Writer WalkControlsWriter;

		void Update()
		{
			float targetX = Input.GetAxis("Horizontal");
			float targetZ = Input.GetAxis("Vertical");

			WalkControlsWriter.Send(new WalkControls.Update()
				.SetTargetSpeed(new Improbable.Vector3f(targetX, 0.0f, targetZ)));
		}

	}
}