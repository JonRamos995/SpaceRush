using SpaceRush.Data;

namespace SpaceRush.Models
{
    [System.Serializable]
    public class TechState
    {
        public string ID;
        public bool IsUnlocked;

        [System.NonSerialized]
        public TechDefinition Definition;

        public TechState(TechDefinition def)
        {
            ID = def.ID;
            Definition = def;
            IsUnlocked = false;
        }

        public TechState(string id)
        {
            ID = id;
            IsUnlocked = false;
        }
    }
}
