using HarmonyLib;
using Kitchen;
using KitchenMods;
using PreferenceSystem;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

// Namespace should have "Kitchen" in the beginning
namespace KitchenInGameTimer
{
    public class Main : IModInitializer
    {
        public const string MOD_GUID = "IcedMilo.PlateUp.InGameTimer";
        public const string MOD_NAME = "In-Game Timer";
        public const string MOD_VERSION = "0.1.3";

        public const string TIMER_ENABLED_ID = "Enabled";
        public const string TIMER_MODE_ID = "TimerRunDuring";
        public const string TIMER_RESET_MODE_ID = "AutomaticallyReset";
        public const string TIMER_PAUSE_MODE_ID = "FreezeTimerWhenGamePaused";
        public const string GROUPS_SERVED_ENABLED_ID = "GroupsServedEnabled";
        public const string GROUPS_REMAINING_ENABLED_ID = "GroupsRemainingEnabled";

        internal static PreferenceSystemManager PrefManager { get; private set; }
        internal static bool RequestReset = false;
        internal bool IsHost => Session.CurrentGameNetworkMode == GameNetworkMode.Host;

        Harmony harmony;
        static List<Assembly> PatchedAssemblies = new List<Assembly>();

        public Main()
        {
            if (harmony == null)
                harmony = new Harmony(MOD_GUID);
            Assembly assembly = Assembly.GetExecutingAssembly();
            if (assembly != null && !PatchedAssemblies.Contains(assembly))
            {
                harmony.PatchAll(assembly);
                PatchedAssemblies.Add(assembly);
            }
        }

        public void PostActivate(KitchenMods.Mod mod)
        {
            LogWarning($"{MOD_GUID} v{MOD_VERSION} in use!");

            PrefManager = new PreferenceSystemManager(MOD_GUID, MOD_NAME);

            PrefManager.AddLabel("In-Game Timer")
                .AddSpacer()
                .AddButton("Reset", delegate
                {
                    RequestReset = IsHost;
                })
                .AddSpacer()
                .AddSubmenu("Timer Configuration", "timerConfiguration")
                    .AddLabel("Timer Configuration")
                    .AddSpacer()
                    .AddLabel("Show Timer")
                    .AddOption<bool>(
                        TIMER_ENABLED_ID,
                        true,
                        new bool[] { false, true },
                        new string[] { "Disabled", "Enabled" })
                    .AddLabel("Progress")
                    .AddOption<string>(
                        TIMER_MODE_ID,
                        "ALWAYS_RUN",
                        new string[] { "ALWAYS_RUN", "DURING_NIGHT", "DURING_DAY" },
                        new string[] { "Always Run", "During Prep and Practice", "During Day" })
                    .AddLabel("Automatically Reset")
                    .AddOption<string>(
                        TIMER_RESET_MODE_ID,
                        "NEVER",
                        new string[] { "NEVER", "END_OF_DAY", "START_OF_DAY", "START_AND_END" },
                        new string[] { "Never", "At End of Day", "At Start of Day", "Start and End of Day" })
                    .AddLabel("When Game Paused")
                    .AddOption<bool>(
                        TIMER_PAUSE_MODE_ID,
                        true,
                        new bool[] { true, false },
                        new string[] { "Pause Timer", "Continue Running" })
                    .AddSpacer()
                    .AddSpacer()
                .SubmenuDone()
                .AddSubmenu("Group Configuration", "groupConfiguration")
                    .AddLabel("Group Configuration")
                    .AddSpacer()
                    .AddLabel("Show Served Groups")
                    .AddOption<bool>(
                        GROUPS_SERVED_ENABLED_ID,
                        true,
                        new bool[] { false, true },
                        new string[] { "Disabled", "Enabled" })
                    .AddLabel("Show Remaining Scheduled Groups")
                    .AddInfo("Does not include additional groups spawned by certain mods.")
                    .AddOption<bool>(
                        GROUPS_REMAINING_ENABLED_ID,
                        true,
                        new bool[] { false, true },
                        new string[] { "Disabled", "Enabled" })
                .SubmenuDone()
                .AddSpacer()
                .AddSpacer();

            PrefManager.RegisterMenu(PreferenceSystemManager.MenuType.PauseMenu);
        }

        public void PreInject() { }

        public void PostInject() { }

        #region Logging
        public static void LogInfo(string _log) { Debug.Log($"[{MOD_NAME}] " + _log); }
        public static void LogWarning(string _log) { Debug.LogWarning($"[{MOD_NAME}] " + _log); }
        public static void LogError(string _log) { Debug.LogError($"[{MOD_NAME}] " + _log); }
        public static void LogInfo(object _log) { LogInfo(_log.ToString()); }
        public static void LogWarning(object _log) { LogWarning(_log.ToString()); }
        public static void LogError(object _log) { LogError(_log.ToString()); }
        #endregion
    }
}
