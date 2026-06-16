using OperenciaManager.Core;
using OperenciaManager.Systems;
using OperenciaManager.Commands;
using OperenciaManager.Systems.Cron;
using System.Reflection;
using UnityEngine;
using HarmonyLib;
using OperenciaManager.Modules;
using OperenciaManager.Systems.Events.Core;



namespace OperenciaManager
{
    public class ModAPI : IModApi
    {
        public void InitMod(Mod modInstance)
        {
            Log.Out("[OperenciaManager] ModAPI v0.6 initializing...");

            // Parancsok és események
            CommandProcessor.RegisterCommands();
            ModEvents.ChatMessage.RegisterHandler(CommandProcessor.HandleCommand);

            // Harmony patchelés
            var harmony = new Harmony("OperenciaManager");
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            // =========================
            // WATCHEREK / FEATUREK
            // =========================

            // POI belépésfigyelő
            GameObject poiWatcher = new GameObject("PoiEntryWatcher");
            PoiEntryWatcher poiComp = poiWatcher.AddComponent<PoiEntryWatcher>();
            Object.DontDestroyOnLoad(poiWatcher);
            FeatureManager.Register("PoiEntryWatcher", poiComp);
            Log.Out("[OperenciaManager] PoiEntryWatcher regisztrálva.");

            // Claim figyelő
            GameObject claimWatcher = new GameObject("ClaimWatcher");
            ClaimWatcher claimComp = claimWatcher.AddComponent<ClaimWatcher>();
            Object.DontDestroyOnLoad(claimWatcher);
            FeatureManager.Register("ClaimWatcher", claimComp);
            Log.Out("[OperenciaManager] ClaimWatcher regisztrálva.");

            // Autorepair claimen belül
            GameObject autoRepair = new GameObject("ClaimAutoRepair");
            ClaimAutoRepair autoRepairComp = autoRepair.AddComponent<ClaimAutoRepair>();
            Object.DontDestroyOnLoad(autoRepair);
            FeatureManager.Register("ClaimAutoRepair", autoRepairComp);
            Log.Out("[OperenciaManager] ClaimAutoRepair regisztrálva.");

            // Trader ajtófigyelő
            GameObject traderWatcher = new GameObject("TraderDoorWatcher");
            TraderDoorWatcher traderComp = traderWatcher.AddComponent<TraderDoorWatcher>();
            Object.DontDestroyOnLoad(traderWatcher);
            FeatureManager.Register("TraderDoorWatcher", traderComp);
            Log.Out("[OperenciaManager] TraderDoorWatcher regisztrálva.");

            // AntiCheat figyelő
            GameObject antiCheat = new GameObject("AntiCheatItemScanner");
            AntiCheatItemScanner antiCheatComp = antiCheat.AddComponent<AntiCheatItemScanner>();
            Object.DontDestroyOnLoad(antiCheat);
            FeatureManager.Register("AntiCheatItemScanner", antiCheatComp);
            Log.Out("[OperenciaManager] AntiCheatItemScanner regisztrálva.");

            // Zombi védelem figyelő
            GameObject sanctuaryWatcher = new GameObject("SanctuaryEntityWatcher");
            SanctuaryEntityWatcher sanctuaryComp = sanctuaryWatcher.AddComponent<SanctuaryEntityWatcher>();
            Object.DontDestroyOnLoad(sanctuaryWatcher);
            FeatureManager.Register("SanctuaryEntityWatcher", sanctuaryComp);
            Log.Out("[OperenciaManager] SanctuaryEntityWatcher regisztrálva.");

            // Háziállat figyelő
            GameObject petWatcherGO = new GameObject("PetWatcher");
            OperenciaManager.Systems.PetWatcher petComp =
                petWatcherGO.AddComponent<OperenciaManager.Systems.PetWatcher>();
            Object.DontDestroyOnLoad(petWatcherGO);
            FeatureManager.Register("PetWatcher", petComp);
            Log.Out("[OperenciaManager] PetWatcher regisztrálva.");

            // Token / loot watcher
            GameObject lootWatcherGO = new GameObject("LootContainerWatcher");
            OperenciaManager.Systems.LootContainerWatcher lootComp =
                lootWatcherGO.AddComponent<OperenciaManager.Systems.LootContainerWatcher>();
            Object.DontDestroyOnLoad(lootWatcherGO);
            FeatureManager.Register("LootContainerWatcher", lootComp);
            Log.Out("[OperenciaManager] LootContainerWatcher regisztrálva.");

            // Jármű sebesség figyelő (csak dedikált szerveren)
            if (GameManager.IsDedicatedServer)
            {
                GameObject vehicleSpeedGO = new GameObject("VehicleSpeedUpdater");
                OperenciaManager.Systems.VehicleSpeedUpdater vehicleSpeedComp =
                    vehicleSpeedGO.AddComponent<OperenciaManager.Systems.VehicleSpeedUpdater>();
                Object.DontDestroyOnLoad(vehicleSpeedGO);
                FeatureManager.Register("VehicleSpeedUpdater", vehicleSpeedComp);
                Log.Out("[OperenciaManager] VehicleSpeedUpdater regisztrálva.");
            }

            // =========================
            // STATIKUS / KERET MODULOK
            // (ezek most mindig aktívak)
            // =========================

            Log.Out("[OperenciaManager] ClaimZoneResolver elérhető.");

            PlayerEventHooks.Init();
            Log.Out("[OperenciaManager] PlayerEventHooks aktiválva.");

            ServerStatusReporter.Init();
            Log.Out("[OperenciaManager] ServerStatusReporter aktiválva.");

            VehicleMemoryStorage.Init();

            InternalCronService.Init();
            Log.Out("[OperenciaManager] InternalCronService aktiválva.");

            BloodMoonAnnouncer.Init();
            Log.Out("[OperenciaManager] BloodMoonAnnouncer aktiválva.");

            EventManager.Instance.Init();
            Log.Out("[OperenciaManager] EventManager aktiválva.");
            InternalCronService.Register(CronType.Hourly, () =>
            {
                if (!EventManager.Instance.IsActive)
                    EventManager.Instance.StartRandomEvent();
            });


            // Pet death handler
            OperenciaManager.Systems.PetDeathHandler.Init();
            Log.Out("[OperenciaManager] PetDeathHandler aktiválva.");

           



            // Gyári game message suppressor
            GameMessageSuppressor.Init();

            // =========================
            // SETTINGS BETÖLTÉS + APPLY
            // =========================

            OperenciaSettings.Load();
            FeatureManager.Reload();

            Log.Out("[OperenciaManager] settings.json betöltve, feature flag-ek alkalmazva.");
        }
    }
}
