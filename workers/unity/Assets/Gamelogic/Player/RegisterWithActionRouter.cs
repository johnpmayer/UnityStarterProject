using Assets.Gamelogic.ClientManager;
using Improbable.Unity;
using Improbable.Unity.Visualizer;
using UnityEngine;

namespace Assets.Gamelogic.Player
{
    [WorkerType(WorkerPlatform.UnityClient)]
    public class RegisterWithActionRouter : MonoBehaviour
    {
        private void OnEnable()
        {
            var controller = GameObject.FindGameObjectWithTag("GameController");
            var router = controller.GetComponent<ActionRouter>();
            router.RegisterPlayerObject(gameObject);
        }
    }
}