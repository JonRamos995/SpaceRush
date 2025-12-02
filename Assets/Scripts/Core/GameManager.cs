using UnityEngine;
using SpaceRush.Systems;
using SpaceRush.Models;
using System.Collections;
using System.Collections.Generic;

namespace SpaceRush.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("System References")]
        public GameDatabase gameDatabase; // Added Database
        public ResourceManager resourceManager;
        public FleetManager fleetManager;
        public TradingSystem tradingSystem;
        public IdleManager idleManager;
        public LocationManager locationManager;
        public ResearchManager researchManager;
        public PlanetarySystem planetarySystem;
        public LogisticsSystem logisticsSystem;
        public WorkshopManager workshopManager;
        public CivilizationManager civilizationManager;
        public PersistenceManager persistenceManager;

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

        private void Start()
        {
            GameLogger.Log("Space Rush: Idle Trading Empire - Game Started");

            // Ensure Database is initialized FIRST
            if (gameDatabase == null) gameDatabase = gameObject.AddComponent<GameDatabase>();

            // Ensure Systems are initialized
            if (resourceManager == null) resourceManager = gameObject.AddComponent<ResourceManager>();
            if (fleetManager == null) fleetManager = gameObject.AddComponent<FleetManager>();
            if (tradingSystem == null) tradingSystem = gameObject.AddComponent<TradingSystem>();
            if (idleManager == null) idleManager = gameObject.AddComponent<IdleManager>();
            if (locationManager == null) locationManager = gameObject.AddComponent<LocationManager>();
            if (researchManager == null) researchManager = gameObject.AddComponent<ResearchManager>();
            if (planetarySystem == null) planetarySystem = gameObject.AddComponent<PlanetarySystem>();
            if (logisticsSystem == null) logisticsSystem = gameObject.AddComponent<LogisticsSystem>();
            if (workshopManager == null) workshopManager = gameObject.AddComponent<WorkshopManager>();
            if (civilizationManager == null) civilizationManager = gameObject.AddComponent<CivilizationManager>();

            // Add Persistence Manager
            if (persistenceManager == null) persistenceManager = gameObject.AddComponent<PersistenceManager>();

            // Load Game Data
            if (persistenceManager.LoadGame())
            {
                GameLogger.Log("Save data loaded.");
            }
            else
            {
                GameLogger.Log("Starting new session.");
            }

            StartCoroutine(GameLoop());
            StartCoroutine(AutoSaveLoop());
        }

        private void OnApplicationQuit()
        {
            if (persistenceManager != null) persistenceManager.SaveGame();
        }

        private void OnApplicationPause(bool pause)
        {
            if (pause && persistenceManager != null) persistenceManager.SaveGame();
        }

        private IEnumerator AutoSaveLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(30f);
                if (persistenceManager != null) persistenceManager.SaveGame();
            }
        }

        private IEnumerator GameLoop()
        {
            while (true)
            {
                // Global Game Heartbeat
                yield return new WaitForSeconds(1.0f);

                // 1. Ship Maintenance Logic
                if (!FleetManager.Instance.IsOperational)
                {
                    HandleRepairs();
                }
                else
                {
                    // 2. Active Ship Logic (If waiting at a planet, collect resources)
                    HandleShipOperations();
                }
            }
        }

        private void HandleRepairs()
        {
             float cost = FleetManager.Instance.RepairCostPerTick;
             if (ResourceManager.Instance.SpendCredits(cost))
             {
                 FleetManager.Instance.RepairShip(5.0f);
             }
             else
             {
                 FleetManager.Instance.RepairShip(0.5f); // Slow free repair
             }
        }

        private void HandleShipOperations()
        {
            // If the ship is at a location, it automatically collects from the stockpile
            // This replaces the old "Mining Phase" which was instant.
            // Now "Mining" happens in PlanetarySystem.cs, and the ship "Collects" here.

            LocationState currentLoc = LocationManager.Instance.CurrentLocation;
            if (currentLoc != null && currentLoc.State == DiscoveryState.ReadyToMine)
            {
                LogisticsSystem.Instance.CollectLocalResources(currentLoc);
            }
        }
    }
}
