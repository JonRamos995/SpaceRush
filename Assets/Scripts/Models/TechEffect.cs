using UnityEngine;
using SpaceRush.Systems;
using SpaceRush.Core;

namespace SpaceRush.Models
{
    public abstract class TechEffect : ScriptableObject
    {
        public abstract void Apply();
    }

    [CreateAssetMenu(fileName = "StatBonusEffect", menuName = "SpaceRush/Tech/StatBonusEffect")]
    public class StatBonusEffect : TechEffect
    {
        public string StatName; // e.g., "MiningSpeed"
        public float Modifier; // e.g., 0.1 for +10%

        public override void Apply()
        {
            if (ResearchManager.Instance != null)
            {
                ResearchManager.Instance.AddStatBonus(StatName, Modifier);
                Debug.Log($"Applied StatBonus: {StatName} +{Modifier}");
            }
        }
    }

    [CreateAssetMenu(fileName = "UnlockFeatureEffect", menuName = "SpaceRush/Tech/UnlockFeatureEffect")]
    public class UnlockFeatureEffect : TechEffect
    {
        public string FeatureID; // e.g., "REPAIR_DROID"

        public override void Apply()
        {
            if (ResearchManager.Instance != null)
            {
                ResearchManager.Instance.UnlockFeature(FeatureID);
            }
        }
    }
}
