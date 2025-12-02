using System.Collections.Generic;
using SpaceRush.Data;
using SpaceRush.Models;

namespace SpaceRush.Data
{
    [System.Serializable]
    public class LocationDataWrapper
    {
        public List<LocationDefinition> Items;
    }

    [System.Serializable]
    public class TechDataWrapper
    {
        public List<TechDefinition> Items;
    }

    [System.Serializable]
    public class RecipeDataWrapper
    {
        public List<RecipeDefinition> Items;
    }
}
