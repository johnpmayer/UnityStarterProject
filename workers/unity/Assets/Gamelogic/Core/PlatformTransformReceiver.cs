using Improbable;
using Improbable.Core;
using Improbable.Unity.Visualizer;
using Improbable.Unity.Entity;
using UnityEngine;
using System.Collections;
using Improbable.Worker;

namespace Assets.Gamelogic.Core
{
    public class PlatformTransformReceiver : MonoBehaviour
    {
        [Require] private PlatformPosition.Reader PlatformPositionReader;

        private bool SetOnPlatform(EntityId entityId)
        {
            var platformObject = LocalEntities.Instance.Get(entityId);
            if (platformObject == null)
            {
                Debug.Log("Local object not available...");
                return false;
            }
            else
            {
                Debug.Log("Got platform object");

                transform.SetParent(platformObject.UnderlyingGameObject.transform);
                return true;
            }
        }

        private IEnumerator PollForPlatform()
        {
            while (true)
            {
                Debug.Log("Polling for platform");
                var platformEntityId = PlatformPositionReader.Data.platformEntity;
                if (SetOnPlatform(platformEntityId))
                {
                    yield break;
                }
                else
                {
                    yield return new WaitForSeconds(.25f);
                }
            }
        }

        private void OnPlatformEntityUpdated(EntityId entityId) 
        {
            if (PlatformPositionReader.Authority == Authority.NotAuthoritative)
            {
                StartCoroutine("PollForPlatform");
            }
        }

        private void OnEnable()
        {
            // get initial?

            PlatformPositionReader.LocalPositionUpdated.Add(OnLocalPositionUpdated);
            PlatformPositionReader.PlatformEntityUpdated.Add(OnPlatformEntityUpdated);
        }

        private void OnDisable()
        {
            PlatformPositionReader.LocalPositionUpdated.Remove(OnLocalPositionUpdated);
            PlatformPositionReader.PlatformEntityUpdated.Remove(OnPlatformEntityUpdated);
        }

        private void OnLocalPositionUpdated(Vector3f localPosition) {
            if (PlatformPositionReader.Authority == Authority.NotAuthoritative)
            {
                transform.localPosition = localPosition.ToUnityVector();
            }
        }
    }
}