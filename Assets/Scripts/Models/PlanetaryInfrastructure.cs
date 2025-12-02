namespace SpaceRush.Models
{
    [System.Serializable]
    public class PlanetaryInfrastructure
    {
        public int MiningLevel = 0;       // Extracts resources
        public int LogisticsLevel = 0;    // Moves from surface to station
        public int StationLevel = 0;      // Storage capacity at station
    }
}
