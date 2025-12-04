# Space Rush: Idle Trading Empire

## Project Goal
**Target:** Production-Ready Google Play Store Release.

The goal of this project is to deliver a polished, scalable, and modular incremental sci-fi trading game for Android. The final product must be ready for submission to the Google Play Store, featuring robust cloud integration, monetization, and a fully data-driven architecture that allows for easy content expansion (new planets, techs, upgrades) without modifying the source code.

---

## Current State
**Status:** Functional Vertical Slice (Backend Heavy)

The project currently possesses a solid backend foundation with verified core loops. The game logic for Mining, Trading, Crafting (Workshop), and Fleet Management is implemented and covered by a comprehensive suite of "Shadow Tests" (headless unit tests).

Key systems are functional:
*   **Economy:** Resources are generated, stored, and traded dynamically.
*   **Progression:** Tech tree and Ship upgrades work.
*   **Persistence:** Local JSON saving/loading is robust.
*   **Quality Assurance:** High test coverage for backend logic.

However, the project is **NOT** yet production-ready. Several systems rely on hardcoded logic or mock implementations that must be replaced with scalable, real-world solutions before release.

**Recent Update:** The user interface has been completely migrated from legacy UGUI to **Unity 6 UI Toolkit**, improving performance and maintainability.

---

## Missing / Incomplete Features
To achieve the "Production Ready" goal, the following critical tasks must be completed:

### 1. Google Play Services Re-integration (0%)
*   **Requirement:** The `READ ME.txt` notes that Google Play Services were removed due to errors.
*   **Task:** Re-import the plugin and implement:
    *   **Authentication:** Silent sign-in.
    *   **Cloud Save:** Sync `PersistenceManager` JSON data to Google Drive/Cloud.
    *   **Achievements:** Link in-game milestones to Play Games Services.
    *   **Leaderboards:** Track "Civilization Level" or "Net Worth".

### 2. Monetization & Notifications (Mocked -> Real)
*   **AdManager (10%):** Currently a stub (`Debug.Log` only). Needs integration with Google Mobile Ads SDK (AdMob) for Rewarded Video ads (double idle gains).
*   **NotificationManager (10%):** Currently a stub. Needs Android Notification Channel implementation for "Research Complete" or "Ship Repaired" local push notifications.

### 3. Data-Driven Refactoring (Scalability)
*   **Civilization System:** The `CivilizationManager` currently defines upgrades (e.g., "Resource Retention") via hardcoded C# code (`InitializeUpgrades`). This must be moved to `Assets/Resources/Data/civilization.json` to be modular.
*   **Tech & Planetary Links:** `PlanetarySystem` and `ResearchManager` currently use hardcoded `switch` statements to check for specific IDs (e.g., `case "ENV_SUIT_MK2"`). This violates the modularity requirement. This needs to be refactored into a generic "Tag" or "Effect" system so new technologies can affect planets without changing C# code.

---

## Feature Completion Report (Backend Logic)
*Percentages reflect Code Quality, Modularity, and Scalability, not just "does it work".*

| Feature | Completion | Details |
| :--- | :--- | :--- |
| **Trading System** | **100%** | Fully modular. Dynamic price fluctuations, events, and buying/selling logic are robust and data-driven. |
| **Fleet Manager** | **95%** | Logic is solid. Uses generic stat lookups. Only minor coupling to specific features (Repair Droid). |
| **Logistics System** | **90%** | cargo transfer logic is clean and testable. Minor hardcoded dependency on specific Tech IDs for automation. |
| **Workshop Manager** | **90%** | Crafting, recipes, and automation are well-implemented using `GameDatabase`. Very stable. |
| **Research System** | **70%** | The command pattern is present, but the mapping of `TechID` -> `Effect` is still hardcoded in `AssignEffectsToTech`. |
| **Planetary System** | **60%** | Functional, but heavily coupled. `ProduceResources` contains hardcoded `if (Tech == "ENV_SUIT_MK2")` checks, making it hard to expand without code changes. |
| **Civilization System**| **40%** | Prestige logic works, but the Upgrade content is hardcoded in the class. Needs a full data-driven refactor. |
| **Ads & Notifications**| **10%** | Purely mock implementations. Logic structure exists, but no real functionality. |
| **UI System** | **100%** | Fully migrated to Unity 6 UI Toolkit. Uses UXML/USS for layout/style and a centralized controller. |

---

## Technical Notes for Developers
*   **Testing:** Run `dotnet test` in `ShadowTests/` to verify backend logic changes.
*   **Data:** All game data should live in `Assets/Resources/Data/`. Avoid adding new content directly in C# classes.
*   **UI:** The project uses **Unity UI Toolkit** (Unity 6). UI Logic is in `Assets/Scripts/UI/GameUIController.cs`. Layouts are in `Assets/UI/MainLayout.uxml` and styling in `Assets/UI/Theme.uss`.
