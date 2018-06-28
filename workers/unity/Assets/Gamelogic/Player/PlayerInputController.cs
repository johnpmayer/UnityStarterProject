using Improbable;
using Improbable.Unity;
using Improbable.Player;
using Improbable.Unity.Visualizer;
using UnityEngine;
using UnityEngine.SceneManagement;
using Improbable.Unity.Core;
using Improbable.Core;

namespace Assets.Gamelogic.Player {

	[WorkerType(WorkerPlatform.UnityClient)]
	public class PlayerInputController : MonoBehaviour {

		[Require] private WalkControls.Writer WalkControlsWriter;

		private void Update()
		{
			float targetX = Input.GetAxis("Horizontal");
			float targetZ = Input.GetAxis("Vertical");

			WalkControlsWriter.Send(new WalkControls.Update()
				.SetTargetSpeed(new Improbable.Vector3f(targetX, 0.0f, targetZ)));
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

		private void OnBoardSuccess(BoardResponse response) {
			Debug.Log("Board success");
		}

		private void OnBoardFailure(ICommandErrorDetails response) {
			Debug.LogError("Board failed: " + response.ErrorMessage);
		}

	}
}