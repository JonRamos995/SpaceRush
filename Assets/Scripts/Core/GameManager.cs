using UnityEngine;
using SpaceRush.Systems;

namespace SpaceRush.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("System References")]
        public ResourceManager resourceManager;
        public FleetManager fleetManager;
        public TradingSystem tradingSystem;
        public IdleManager idleManager;

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

        private void Start()
        {
            Debug.Log("Space Rush: Idle Trading Empire - Game Started");

            // Ensure Systems are initialized if not dragged in via Inspector
            if (resourceManager == null) resourceManager = gameObject.AddComponent<ResourceManager>();
            if (fleetManager == null) fleetManager = gameObject.AddComponent<FleetManager>();
            if (tradingSystem == null) tradingSystem = gameObject.AddComponent<TradingSystem>();
            if (idleManager == null) idleManager = gameObject.AddComponent<IdleManager>();

            // Start simple gameplay loop (e.g. mining tick)
            StartCoroutine(GameLoop());
        }

        private System.Collections.IEnumerator GameLoop()
        {
            while (true)
            {
                // Simulate mining tick based on fleet stats
                // In a real implementation, this might be event driven or on a faster timer
                yield return new WaitForSeconds(1.0f / FleetManager.Instance.MiningSpeed);

                // Simple logic: Mine 1 Iron every tick
                ResourceManager.Instance.AddResource(SpaceRush.Models.ResourceType.Iron, 1);
            }
        }
    }
}
