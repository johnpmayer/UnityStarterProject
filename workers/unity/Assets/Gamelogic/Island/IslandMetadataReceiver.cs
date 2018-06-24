using Improbable.Island;
using Improbable.Unity.Visualizer;
using Improbable.Worker;
using UnityEngine;

namespace Assets.Gamelogic.Island
{
    public class IslandMetadataReceiver : MonoBehaviour
    {
        [Require] private IslandMetadata.Reader MetadataReader;

        private string _islandName;

        public string IslandName()
        {
            return _islandName;
        }

        private void OnEnable()
        {
            _islandName = MetadataReader.Data.name;

            MetadataReader.ComponentUpdated.Add(OnMetadataUpdated);
        }

        private void OnDisable()
        {
            MetadataReader.ComponentUpdated.Remove(OnMetadataUpdated);
        }

        private void OnMetadataUpdated(IslandMetadata.Update update)
        {
            if (MetadataReader.Authority == Authority.NotAuthoritative && update.name.HasValue)
            {
                _islandName = update.name.Value;
            }
        }
    }
}