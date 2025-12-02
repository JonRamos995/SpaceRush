using UnityEngine;

namespace SpaceRush.Data
{
    [CreateAssetMenu(fileName = "New Technology", menuName = "SpaceRush/Technology")]
    public class TechDefinition : ScriptableObject
    {
        public string ID;
        public string Name;
        public string Description;
        public float Cost;
        public int ResearchPointsRequired;

        public TechDefinition(string id, string name, string desc, float cost, int rp)
        {
            ID = id;
            Name = name;
            Description = desc;
            Cost = cost;
            ResearchPointsRequired = rp;
        }

        public TechDefinition() {}
    }
}
