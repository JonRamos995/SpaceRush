namespace SpaceRush.Systems
{
    public enum DiscoveryState
    {
        Hidden,
        Discovered,   // Just found it
        Investigated, // Knows biome and resources
        ReadyToMine   // Infrastructure started
    }

    public enum BiomeType
    {
        Terrestrial, // Earth-like
        Barren,      // Moon
        Volcanic,
        Ice,
        GasGiant,
        AsteroidField
    }
}
