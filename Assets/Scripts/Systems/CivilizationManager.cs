using UnityEngine;
using SpaceRush.Core;
using SpaceRush.Models;

namespace SpaceRush.Systems
{
    public class CivilizationManager : MonoBehaviour
    {
        public static CivilizationManager Instance { get; private set; }

        public int Level { get; private set; }
        public float PrestigeCurrency { get; private set; }

        public float Multiplier => 1.0f + (Level * 0.1f);

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void LoadData(CivilizationSaveData data)
        {
            if (data == null)
            {
                Level = 0;
                PrestigeCurrency = 0;
            }
            else
            {
                Level = data.Level;
                PrestigeCurrency = data.PrestigeCurrency;
            }
        }

        public CivilizationSaveData GetSaveData()
        {
            return new CivilizationSaveData
            {
                Level = Level,
                PrestigeCurrency = PrestigeCurrency
            };
        }

        public void Ascend()
        {
            // Calculate gains (simplified logic: +1 Level, +100 Currency)
            Level++;
            PrestigeCurrency += 100f;

            GameLogger.Log($"Ascended to Civilization Level {Level}! Multiplier: {Multiplier}x");

            // Trigger Global Reset via PersistenceManager
            PersistenceManager.Instance.ResetGameKeepCivilization();
        }
    }
}
