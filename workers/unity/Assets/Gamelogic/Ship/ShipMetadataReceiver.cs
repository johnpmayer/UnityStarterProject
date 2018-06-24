using Improbable.Ship;
using Improbable.Unity.Visualizer;
using Improbable.Worker;
using UnityEngine;

namespace Assets.Gamelogic.Ship
{
    public class ShipMetadataReceiver : MonoBehaviour
    {
        [Require] private ShipMetadata.Reader MetadataReader;

        private string _shipName;

        public string ShipName()
        {
            return _shipName;
        }

        private void OnEnable()
        {
            _shipName = MetadataReader.Data.name;

            MetadataReader.ComponentUpdated.Add(OnMetadataUpdated);
        }

        private void OnDisable()
        {
            MetadataReader.ComponentUpdated.Remove(OnMetadataUpdated);
        }

        private void OnMetadataUpdated(ShipMetadata.Update update)
        {
            if (MetadataReader.Authority == Authority.NotAuthoritative && update.name.HasValue)
            {
                _shipName = update.name.Value;
            }
        }
    }
}