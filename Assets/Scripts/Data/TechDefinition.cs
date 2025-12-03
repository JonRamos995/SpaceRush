using UnityEngine;
using System.Collections.Generic;
using SpaceRush.Models;

namespace SpaceRush.Data
{
    [System.Serializable]
    public class TechDefinition
    {
        public string ID;
        public string Name;
        public string Description;
        public float Cost;
        public int ResearchPointsRequired;

        // New: Command Pattern for effects
        public List<TechEffect> Effects = new List<TechEffect>();
        // Data-driven effects from JSON
        public List<EffectData> EffectDataList = new List<EffectData>();

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
