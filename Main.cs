using Kitchen;
using KitchenLib;
using KitchenLib.Event;
using KitchenMods;
using PreferenceSystem;
using System.Reflection;
using UnityEngine;

// Namespace should have "Kitchen" in the beginning
namespace KitchenInGameTimer
{
    public class Main : BaseMod, IModSystem
    {
        // GUID must be unique and is recommended to be in reverse domain name notation
        // Mod Name is displayed to the player and listed in the mods menu
        // Mod Version must follow semver notation e.g. "1.2.3"
        public const string MOD_GUID = "IcedMilo.PlateUp.InGameTimer";
        public const string MOD_NAME = "In-Game Timer";
        public const string MOD_VERSION = "0.1.0";
        public const string MOD_AUTHOR = "IcedMilo";
        public const string MOD_GAMEVERSION = ">=1.1.5";
        // Game version this mod is designed for in semver
        // e.g. ">=1.1.3" current and all future
        // e.g. ">=1.1.3 <=1.2.3" for all from/until

        // Boolean constant whose value depends on whether you built with DEBUG or RELEASE mode, useful for testing
#if DEBUG
        public const bool DEBUG_MODE = true;
#else
        public const bool DEBUG_MODE = false;
#endif

        internal static PreferenceSystemManager PrefManager { get; private set; }

        internal static bool RequestStart = false;
        internal static bool RequestPause = false;
        internal static bool RequestReset = false;

        internal bool IsHost => Session.CurrentGameNetworkMode == GameNetworkMode.Host;

        public Main() : base(MOD_GUID, MOD_NAME, MOD_AUTHOR, MOD_VERSION, MOD_GAMEVERSION, Assembly.GetExecutingAssembly()) { }

        protected override void OnInitialise()
        {
            LogWarning($"{MOD_GUID} v{MOD_VERSION} in use!");
        }

        protected override void OnUpdate()
        {
            try
            {

            } catch { }
        }

        protected override void OnPostActivate(KitchenMods.Mod mod)
        {
            PrefManager = new PreferenceSystemManager(MOD_GUID, MOD_NAME);

            PrefManager.AddLabel("In-Game Timer")
                .AddSpacer()
                .AddButton("Reset", delegate
                {
                    RequestReset = IsHost;
                })
                .AddSpacer()
                .AddSubmenu("Configuration", "configuration")
                    .AddLabel("Timer Configuration")
                    .AddSpacer()
                    .AddLabel("Use Custom Timer")
                    .AddOption<bool>(
                        "Enabled",
                        true,
                        new bool[] { false, true },
                        new string[] { "Disabled", "Enabled" })
                    .AddLabel("Progress")
                    .AddOption<string>(
                        "TimerRunDuring",
                        "ALWAYS_RUN",
                        new string[] { "ALWAYS_RUN", "DURING_NIGHT", "DURING_DAY" },
                        new string[] { "Always Run", "During Prep and Practice", "During Day" })
                    .AddLabel("Automatically Reset")
                    .AddOption<string>(
                        "AutomaticallyReset",
                        "NEVER",
                        new string[] { "NEVER", "END_OF_DAY", "START_OF_DAY", "START_AND_END" },
                        new string[] { "Never", "At End of Day", "At Start of Day", "Start and End of Day" })
                    .AddLabel("When Game Paused")
                    .AddOption<bool>(
                        "FreezeTimerWhenGamePaused",
                        true,
                        new bool[] { true, false },
                        new string[] { "Pause Timer", "Continue Running" })
                    .AddSpacer()
                    .AddSpacer()
                .SubmenuDone()
                .AddSpacer()
                .AddSpacer();

            PrefManager.RegisterMenu(PreferenceSystemManager.MenuType.PauseMenu);
        }

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
