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
        public ResourceManager resourceManager;
        public FleetManager fleetManager;
        public TradingSystem tradingSystem;
        public IdleManager idleManager;
        public LocationManager locationManager;
        public ResearchManager researchManager;
        public PlanetarySystem planetarySystem;
        public LogisticsSystem logisticsSystem;

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
            GameLogger.Log("Space Rush: Idle Trading Empire - Game Started");

            // Ensure Systems are initialized
            if (resourceManager == null) resourceManager = gameObject.AddComponent<ResourceManager>();
            if (fleetManager == null) fleetManager = gameObject.AddComponent<FleetManager>();
            if (tradingSystem == null) tradingSystem = gameObject.AddComponent<TradingSystem>();
            if (idleManager == null) idleManager = gameObject.AddComponent<IdleManager>();
            if (locationManager == null) locationManager = gameObject.AddComponent<LocationManager>();
            if (researchManager == null) researchManager = gameObject.AddComponent<ResearchManager>();
            if (planetarySystem == null) planetarySystem = gameObject.AddComponent<PlanetarySystem>();
            if (logisticsSystem == null) logisticsSystem = gameObject.AddComponent<LogisticsSystem>();

            StartCoroutine(GameLoop());
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

            Location currentLoc = LocationManager.Instance.CurrentLocation;
            if (currentLoc != null && currentLoc.State == DiscoveryState.ReadyToMine)
            {
                LogisticsSystem.Instance.CollectLocalResources(currentLoc);
            }
        }
    }
}
