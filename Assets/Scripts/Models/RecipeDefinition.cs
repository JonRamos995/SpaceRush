using UnityEngine;
using SpaceRush.Models;

namespace SpaceRush.Models
{
    [System.Serializable]
    public class RecipeDefinition
    {
        public string ID;
        public string Name;
        public ResourceType InputResource;
        public int InputAmount;
        public ResourceType OutputResource;
        public int OutputAmount;
        public float DurationSeconds;
        public string RequiredTechID;
    }
}
