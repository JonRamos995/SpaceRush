using System.Collections.Generic;
using SpaceRush.Data;
using SpaceRush.Systems;

namespace SpaceRush.Models
{
    [System.Serializable]
    public class LocationState
    {
        public string ID;

        // Runtime State
        public bool IsUnlocked;
        public DiscoveryState State = DiscoveryState.Hidden;

        // Local Economy
        public PlanetaryInfrastructure Infrastructure = new PlanetaryInfrastructure();
        public Dictionary<ResourceType, int> Stockpile = new Dictionary<ResourceType, int>();

        // Processing Configuration
        public string ActiveRecipeID;

        // Reference to static data (not serialized, loaded at runtime)
        [System.NonSerialized]
        public LocationDefinition Definition;

        public LocationState(LocationDefinition def)
        {
            ID = def.ID;
            Definition = def;
            Stockpile = new Dictionary<ResourceType, int>();
            Infrastructure = new PlanetaryInfrastructure();
        }

        // For loading from save
        public LocationState(string id)
        {
            ID = id;
            Stockpile = new Dictionary<ResourceType, int>();
            Infrastructure = new PlanetaryInfrastructure();
        }
    }
}
