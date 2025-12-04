using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using SpaceRush.Core;
using SpaceRush.Systems;
using SpaceRush.Models;

namespace SpaceRush.UI
{
    public class GameUIController : MonoBehaviour
    {
        public static GameUIController Instance { get; private set; }

        private UIDocument _doc;
        private VisualElement _root;

        // Header Elements
        private Label _creditsLabel;
        private Label _locationLabel;
        private Label _hullLabel;
        private Label _lastLogLabel;

        // Panels
        private Dictionary<string, VisualElement> _panels = new Dictionary<string, VisualElement>();
        private string _activePanelName = "View-Dashboard";

        // Dashboard Elements
        private Label _dashboardStats;
        private Button _btnRepair;
        private Button _btnUpgradeShip;

        // Location Details Elements
        private Label _locDetailsName;
        private Label _locDetailsInfo;
        private Button _btnInvestigate;
        private Button _btnStartMining;
        private Button _btnUpgradeMining;
        private Button _btnUpgradeStation;
        private Button _btnUpgradeLogistics;
        private Button _btnBackToNav;
        private string _selectedLocationID;

        // Containers
        private ScrollView _locationList;
        private ScrollView _workshopList;
        private ScrollView _marketList;
        private ScrollView _logisticsList;
        private ScrollView _researchList;
        private ScrollView _logList;

        // Civilization Elements
        private Label _civLevelLabel;
        private Label _civNanitesLabel;
        private ScrollView _metaUpgradeList;
        private Button _btnAscend;

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

        private void OnEnable()
        {
            _doc = GetComponent<UIDocument>();
            if (_doc == null)
            {
                Debug.LogError("GameUIController requires a UIDocument component!");
                return;
            }

            _root = _doc.rootVisualElement;

            // Header/Footer Bindings
            _creditsLabel = _root.Q<Label>("CreditsLabel");
            _locationLabel = _root.Q<Label>("LocationLabel");
            _hullLabel = _root.Q<Label>("HullLabel");
            _lastLogLabel = _root.Q<Label>("LastLogLabel");

            // Panel Bindings
            BindPanel("View-Dashboard", "Btn-Dashboard");
            BindPanel("View-Locations", "Btn-Locations");
            BindPanel("View-Workshop", "Btn-Workshop");
            BindPanel("View-Market", "Btn-Market");
            BindPanel("View-Logistics", "Btn-Logistics");
            BindPanel("View-Research", "Btn-Research");
            BindPanel("View-Civilization", "Btn-Civilization");
            BindPanel("View-Log", "Btn-Log");

            // Location Details Panel (No Nav Button, manual switch)
            var locDetails = _root.Q<VisualElement>("View-LocationDetails");
            if (locDetails != null) _panels["View-LocationDetails"] = locDetails;

            // Container Bindings
            _dashboardStats = _root.Q<Label>("Dashboard-Stats");
            _locationList = _root.Q<ScrollView>("Location-List");
            _workshopList = _root.Q<ScrollView>("Workshop-List");
            _marketList = _root.Q<ScrollView>("Market-List");
            _logisticsList = _root.Q<ScrollView>("Logistics-List");
            _researchList = _root.Q<ScrollView>("Research-List");
            _logList = _root.Q<ScrollView>("Log-List");

            // Civilization Bindings
            _civLevelLabel = _root.Q<Label>("Civ-LevelLabel");
            _civNanitesLabel = _root.Q<Label>("Civ-NanitesLabel");
            _metaUpgradeList = _root.Q<ScrollView>("Meta-Upgrade-List");
            _btnAscend = _root.Q<Button>("Btn-Ascend");

            if (_btnAscend != null)
            {
                _btnAscend.clicked += OnAscendClicked;
            }

            // Dashboard Buttons
            _btnRepair = _root.Q<Button>("Btn-RepairShip");
            if (_btnRepair != null) _btnRepair.clicked += OnRepairShip;

            _btnUpgradeShip = _root.Q<Button>("Btn-UpgradeShip");
            if (_btnUpgradeShip != null) _btnUpgradeShip.clicked += OnUpgradeShip;

            // Location Details Buttons
            _locDetailsName = _root.Q<Label>("LocDetails-Name");
            _locDetailsInfo = _root.Q<Label>("LocDetails-Info");

            _btnInvestigate = _root.Q<Button>("Btn-Investigate");
            if (_btnInvestigate != null) _btnInvestigate.clicked += () => OnInvestigate(_selectedLocationID);

            _btnStartMining = _root.Q<Button>("Btn-StartMining");
            if (_btnStartMining != null) _btnStartMining.clicked += () => OnStartMining(_selectedLocationID);

            _btnUpgradeMining = _root.Q<Button>("Btn-UpgradeMining");
            if (_btnUpgradeMining != null) _btnUpgradeMining.clicked += () => OnUpgradeInfra(_selectedLocationID, "Mining");

            _btnUpgradeStation = _root.Q<Button>("Btn-UpgradeStation");
            if (_btnUpgradeStation != null) _btnUpgradeStation.clicked += () => OnUpgradeInfra(_selectedLocationID, "Station");

            _btnUpgradeLogistics = _root.Q<Button>("Btn-UpgradeLogistics");
            if (_btnUpgradeLogistics != null) _btnUpgradeLogistics.clicked += () => OnUpgradeInfra(_selectedLocationID, "Logistics");

            _btnBackToNav = _root.Q<Button>("Btn-BackToNav");
            if (_btnBackToNav != null) _btnBackToNav.clicked += () => SwitchPanel("View-Locations");

            // Logger Subscription
            GameLogger.OnLogMessage += HandleLogMessage;

            // Initial Render
            RefreshActivePanel();
        }

        private void OnDisable()
        {
            GameLogger.OnLogMessage -= HandleLogMessage;
        }

        private void Update()
        {
            UpdateHeader();

            // Periodic refresh for dynamic data
            if (Time.frameCount % 30 == 0)
            {
                if (_activePanelName == "View-Dashboard") UpdateDashboard();
                if (_activePanelName == "View-LocationDetails") UpdateLocationDetails(_selectedLocationID);
            }
        }

        private void BindPanel(string viewName, string btnName)
        {
            var panel = _root.Q<VisualElement>(viewName);
            var btn = _root.Q<Button>(btnName);

            if (panel != null)
            {
                _panels[viewName] = panel;
            }

            if (btn != null)
            {
                btn.clicked += () => SwitchPanel(viewName);
            }
        }

        private void SwitchPanel(string panelName)
        {
            if (!_panels.ContainsKey(panelName)) return;

            // Hide all
            foreach (var p in _panels.Values)
            {
                p.RemoveFromClassList("panel--active");
            }

            // Show target
            _panels[panelName].AddToClassList("panel--active");
            _activePanelName = panelName;

            RefreshActivePanel();
        }

        private void RefreshActivePanel()
        {
            switch (_activePanelName)
            {
                case "View-Dashboard":
                    UpdateDashboard();
                    break;
                case "View-Locations":
                    RebuildLocationList();
                    break;
                case "View-Workshop":
                    RebuildWorkshopList();
                    break;
                case "View-Market":
                    RebuildMarketList();
                    break;
                case "View-Logistics":
                    RebuildLogisticsList();
                    break;
                case "View-Research":
                    RebuildResearchList();
                    break;
                case "View-Civilization":
                    RebuildCivilizationList();
                    break;
                case "View-LocationDetails":
                    UpdateLocationDetails(_selectedLocationID);
                    break;
                case "View-Log":
                    RebuildLogList();
                    break;
            }
        }

        private void HandleLogMessage(string msg)
        {
            if (_lastLogLabel != null)
            {
                _lastLogLabel.text = msg;
            }

            // If Log View is active, append (optimization: don't full rebuild)
            if (_activePanelName == "View-Log" && _logList != null)
            {
                var label = new Label(msg);
                label.style.borderBottomWidth = 1;
                label.style.borderBottomColor = new StyleColor(new Color(1, 1, 1, 0.1f));
                _logList.Add(label);
                // Auto-scroll logic requires scheduling, maybe skip for now
            }
        }

        // --- HUD UPDATE ---
        private void UpdateHeader()
        {
            if (ResourceManager.Instance != null && _creditsLabel != null)
                _creditsLabel.text = $"Credits: {ResourceManager.Instance.Credits:N0}";

            if (LocationManager.Instance != null && _locationLabel != null)
            {
                 string locName = LocationManager.Instance.CurrentLocation != null ?
                    LocationManager.Instance.CurrentLocation.Definition.Name : "Deep Space";
                _locationLabel.text = $"Location: {locName}";
            }

            if (FleetManager.Instance != null && _hullLabel != null)
                _hullLabel.text = $"Hull: {FleetManager.Instance.RepairStatus:F0}%";
        }

        // --- DASHBOARD ---
        private void UpdateDashboard()
        {
            if (_dashboardStats == null) return;
            if (FleetManager.Instance == null) return;

            string stats = "";
            stats += $"Cargo: {ResourceManager.Instance.CurrentCargo}/{FleetManager.Instance.CargoCapacity}\n";
            stats += $"Mining Speed: {FleetManager.Instance.MiningSpeed:F1}/s\n";
            stats += $"Ship Level: {FleetManager.Instance.ShipLevel}\n";
            stats += $"Repairs: {FleetManager.Instance.RepairStatus:F1}%\n";

            _dashboardStats.text = stats;
        }

        private void OnRepairShip()
        {
            if (FleetManager.Instance == null) return;
            // Assuming 10 Credits for 5% repair for manual.
            float cost = 10f;
            if (ResourceManager.Instance.SpendCredits(cost))
            {
                FleetManager.Instance.RepairShip(5.0f);
                UpdateDashboard();
            }
        }

        private void OnUpgradeShip()
        {
            if (FleetManager.Instance == null) return;
            FleetManager.Instance.UpgradeShip();
            UpdateDashboard();
        }

        // --- LOCATIONS ---
        private void RebuildLocationList()
        {
            if (_locationList == null || LocationManager.Instance == null) return;
            _locationList.Clear();

            foreach (var loc in LocationManager.Instance.Locations)
            {
                var row = new VisualElement();
                row.AddToClassList("list-item");

                var label = new Label($"{loc.Definition.Name} ({loc.State})");
                label.style.flexGrow = 1;

                var btnContainer = new VisualElement();
                btnContainer.style.flexDirection = FlexDirection.Row;

                var btnDetails = new Button(() => {
                    _selectedLocationID = loc.ID;
                    SwitchPanel("View-LocationDetails");
                });
                btnDetails.text = "Details";
                btnDetails.AddToClassList("button");

                var btnTravel = new Button(() => {
                    LocationManager.Instance.TryTravel(loc.ID); // Fix: Use ID
                    UpdateHeader();
                });
                btnTravel.text = "Travel";
                btnTravel.AddToClassList("button");

                btnContainer.Add(btnDetails);
                btnContainer.Add(btnTravel);

                row.Add(label);
                row.Add(btnContainer);
                _locationList.Add(row);
            }
        }

        // --- LOCATION DETAILS ---
        private void UpdateLocationDetails(string locID)
        {
            if (string.IsNullOrEmpty(locID)) return;
            var loc = LocationManager.Instance.Locations.Find(l => l.ID == locID);
            if (loc == null) return;

            if (_locDetailsName != null) _locDetailsName.text = loc.Definition.Name;

            string info = $"State: {loc.State}\n";
            info += $"Biome: {loc.Definition.Biome}\n";
            info += $"Mining Level: {loc.Infrastructure.MiningLevel}\n";
            info += $"Station Level: {loc.Infrastructure.StationLevel}\n";
            info += $"Logistics Level: {loc.Infrastructure.LogisticsLevel}\n";

            // Stockpile
            info += "\nStockpile:\n";
            foreach(var kvp in loc.Stockpile)
            {
                info += $"{kvp.Key}: {kvp.Value}\n";
            }

            if (_locDetailsInfo != null) _locDetailsInfo.text = info;

            // Update Button States
            if (_btnInvestigate != null) _btnInvestigate.SetEnabled(loc.State == DiscoveryState.Discovered);
            if (_btnStartMining != null) _btnStartMining.SetEnabled(loc.State == DiscoveryState.Investigated);

            bool canUpgrade = loc.State == DiscoveryState.ReadyToMine;
            if (_btnUpgradeMining != null) _btnUpgradeMining.SetEnabled(canUpgrade);
            if (_btnUpgradeStation != null) _btnUpgradeStation.SetEnabled(canUpgrade);
            if (_btnUpgradeLogistics != null) _btnUpgradeLogistics.SetEnabled(canUpgrade);
        }

        private void OnInvestigate(string locID) => PlanetarySystem.Instance.InvestigatePlanet(locID);
        private void OnStartMining(string locID) => PlanetarySystem.Instance.StartMiningOperations(locID);
        private void OnUpgradeInfra(string locID, string type)
        {
            PlanetarySystem.Instance.UpgradeInfrastructure(locID, type);
            UpdateLocationDetails(locID);
        }

        // --- WORKSHOP ---
        private void RebuildWorkshopList()
        {
            if (_workshopList == null || WorkshopManager.Instance == null) return;
            _workshopList.Clear();

            for (int i = 0; i < WorkshopManager.Instance.Slots.Count; i++)
            {
                var slot = WorkshopManager.Instance.Slots[i];
                int index = i;

                var row = new VisualElement();
                row.AddToClassList("list-item");
                row.style.flexDirection = FlexDirection.Column;

                var headerRow = new VisualElement();
                headerRow.style.flexDirection = FlexDirection.Row;
                headerRow.style.justifyContent = Justify.SpaceBetween;

                var title = new Label($"Slot {index + 1}: {slot.InstalledMachine}");
                title.style.unityFontStyleAndWeight = FontStyle.Bold;
                headerRow.Add(title);

                // AI Button
                if (!slot.IsAutomated)
                {
                    var btnAI = new Button(() => {
                        WorkshopManager.Instance.InstallAI(index);
                        RebuildWorkshopList();
                    });
                    btnAI.text = $"Install AI ({WorkshopManager.AI_INSTALLATION_COST} CR)";
                    btnAI.AddToClassList("button");
                    headerRow.Add(btnAI);
                }
                else
                {
                    var aiLabel = new Label("[AI ACTIVE]");
                    aiLabel.style.color = new StyleColor(Color.cyan);
                    headerRow.Add(aiLabel);
                }

                row.Add(headerRow);

                if (slot.IsWorking)
                {
                    var status = new Label($"Crafting... {slot.Progress:P0}");
                    row.Add(status);

                    var cancelBtn = new Button(() => {
                        WorkshopManager.Instance.CancelJob(index);
                        RebuildWorkshopList();
                    });
                    cancelBtn.text = "Cancel";
                    cancelBtn.AddToClassList("button");
                    row.Add(cancelBtn);
                }
                else
                {
                    var status = new Label("Idle");
                    row.Add(status);

                    var recipeContainer = new VisualElement();
                    recipeContainer.style.flexDirection = FlexDirection.Row;
                    recipeContainer.style.flexWrap = Wrap.Wrap;

                    foreach (var recipe in WorkshopManager.Instance.GetRecipesForMachine(slot.InstalledMachine))
                    {
                        var craftBtn = new Button(() => {
                            WorkshopManager.Instance.StartJob(index, recipe.ID);
                            RebuildWorkshopList();
                        });
                        craftBtn.text = $"Craft {recipe.OutputResource}";
                        craftBtn.AddToClassList("button");
                        recipeContainer.Add(craftBtn);
                    }
                    row.Add(recipeContainer);
                }

                _workshopList.Add(row);
            }
        }

        // --- MARKET ---
        private void RebuildMarketList()
        {
            if (_marketList == null || ResourceManager.Instance == null) return;
            _marketList.Clear();

            foreach (var res in ResourceManager.Instance.GetAllResources())
            {
                var row = new VisualElement();
                row.AddToClassList("list-item");

                var info = new Label($"{res.Name}\nQty: {res.Quantity}\nValue: {res.CurrentMarketValue:F0} CR");
                info.style.flexGrow = 1;

                var btnContainer = new VisualElement();

                var sellBtn = new Button(() => {
                    TradingSystem.Instance.SellResource(res.Type, 1);
                    RebuildMarketList();
                });
                sellBtn.text = "Sell 1";
                sellBtn.AddToClassList("button");

                var buyBtn = new Button(() => {
                    TradingSystem.Instance.BuyResource(res.Type, 1);
                    RebuildMarketList();
                });
                buyBtn.text = "Buy 1";
                buyBtn.AddToClassList("button");

                btnContainer.Add(sellBtn);
                btnContainer.Add(buyBtn);

                row.Add(info);
                row.Add(btnContainer);
                _marketList.Add(row);
            }
        }

        // --- LOGISTICS ---
        private void RebuildLogisticsList()
        {
             if (_logisticsList == null || LogisticsSystem.Instance == null) return;
            _logisticsList.Clear();

            var resources = ResourceManager.Instance.GetAllResources();
            foreach (var res in resources)
            {
                var row = new VisualElement();
                row.AddToClassList("list-item");

                float currentVal = 0;
                if (LogisticsSystem.Instance.CargoAllocations.ContainsKey(res.Type))
                    currentVal = LogisticsSystem.Instance.CargoAllocations[res.Type];

                var label = new Label($"{res.Name} Alloc: {currentVal:P0}");
                label.style.width = 150;

                var slider = new Slider(0, 1);
                slider.value = currentVal;
                slider.style.flexGrow = 1;
                slider.RegisterValueChangedCallback(evt => {
                    LogisticsSystem.Instance.SetAllocation(res.Type, evt.newValue);
                    label.text = $"{res.Name} Alloc: {evt.newValue:P0}";
                });

                row.Add(label);
                row.Add(slider);
                _logisticsList.Add(row);
            }
        }

        // --- RESEARCH ---
        private void RebuildResearchList()
        {
            if (_researchList == null || ResearchManager.Instance == null) return;
            _researchList.Clear();

            var rpLabel = new Label($"Research Points: {ResearchManager.Instance.ResearchPoints:F0}");
            rpLabel.AddToClassList("label-title");
            _researchList.Add(rpLabel);

            foreach (var tech in ResearchManager.Instance.GetAllTechs())
            {
                var row = new VisualElement();
                row.AddToClassList("list-item");

                var info = new Label($"{tech.Definition.Name}\nCost: {tech.Definition.ResearchPointsRequired} RP");
                info.style.flexGrow = 1;
                if (tech.IsUnlocked) info.style.color = new StyleColor(Color.green);

                var btn = new Button();
                if (tech.IsUnlocked)
                {
                    btn.text = "Unlocked";
                    btn.SetEnabled(false);
                }
                else
                {
                    btn.text = "Unlock";
                    btn.clicked += () => {
                        ResearchManager.Instance.UnlockTechnology(tech.ID);
                        RebuildResearchList();
                    };
                }
                btn.AddToClassList("button");

                row.Add(info);
                row.Add(btn);
                _researchList.Add(row);
            }
        }

        // --- LOG VIEW ---
        private void RebuildLogList()
        {
            if (_logList == null) return;
            _logList.Clear();

            // Reverse order to see newest first? Or standard list.
            // Typically logs are newest at bottom, but scrolling...
            // Let's do standard list (oldest top).
            foreach (var msg in GameLogger.LogHistory)
            {
                var label = new Label(msg);
                label.style.borderBottomWidth = 1;
                label.style.borderBottomColor = new StyleColor(new Color(1, 1, 1, 0.1f));
                _logList.Add(label);
            }
        }

        // --- CIVILIZATION / PRESTIGE ---

        private void RebuildCivilizationList()
        {
            if (CivilizationManager.Instance == null) return;

            // 1. Update Header Stats
            if (_civLevelLabel != null)
                _civLevelLabel.text = $"Civ Level: {CivilizationManager.Instance.Level}";

            if (_civNanitesLabel != null)
                _civNanitesLabel.text = $"Nanites: {CivilizationManager.Instance.Nanites:N0}";

            // 2. Build Upgrade List
            if (_metaUpgradeList == null) return;
            _metaUpgradeList.Clear();

            foreach (var upgrade in CivilizationManager.Instance.AvailableUpgrades)
            {
                var row = new VisualElement();
                row.AddToClassList("list-item");

                var info = new Label($"{upgrade.Name}\n{upgrade.Description}\nCost: {upgrade.Cost} Nanites");
                info.style.flexGrow = 1;

                var btn = new Button();
                // Check if unlocked
                bool isUnlocked = CivilizationManager.Instance.IsUpgradeUnlocked(upgrade.ID);
                if (isUnlocked)
                {
                    btn.text = "Owned";
                    btn.SetEnabled(false);
                }
                else
                {
                    btn.text = "Buy";
                    btn.clicked += () => {
                        if (CivilizationManager.Instance.BuyUpgrade(upgrade.ID))
                        {
                            RebuildCivilizationList(); // Refresh to update Nanite count and disable button
                        }
                    };
                    btn.SetEnabled(CivilizationManager.Instance.Nanites >= upgrade.Cost);
                }
                btn.AddToClassList("button");

                row.Add(info);
                row.Add(btn);
                _metaUpgradeList.Add(row);
            }

            // 3. Update Ascend Button Context
            // Show what they will gain if they reset now
            if (_btnAscend != null)
            {
                float gain = CivilizationManager.Instance.GetProjectedAscensionGain();
                _btnAscend.text = $"ASCEND\n(Reset & Gain {gain:N0} Nanites)";
            }
        }

        private void OnAscendClicked()
        {
            if (CivilizationManager.Instance == null) return;

            CivilizationManager.Instance.Ascend();

            // After ascension, the PersistenceManager resets data and saves.
            // We should refresh the dashboard to reflect the "fresh start".
            SwitchPanel("View-Dashboard");

            GameLogger.Log("Civilization Ascended! Welcome to the new era.");
        }
    }
}
