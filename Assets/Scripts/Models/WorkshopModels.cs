using System;
using System.Collections.Generic;

namespace SpaceRush.Models
{
    public enum MachineType
    {
        None,
        BasicSmelter,   // Smelts Ore -> Ingot
        Assembler       // Assembles Ingot -> Items
    }

    [Serializable]
    public class WorkshopSlot
    {
        public int SlotIndex;
        public MachineType InstalledMachine;

        // Job State
        public string ActiveRecipeID;
        public float Progress;      // 0.0 to 1.0 (or seconds remaining?)
        public bool IsWorking;

        // Upgrades
        public bool IsAutomated;    // Has AI attached
    }

    [Serializable]
    public class WorkshopSaveData
    {
        public int UnlockedSlots;
        public List<WorkshopSlot> Slots = new List<WorkshopSlot>();
    }
}
