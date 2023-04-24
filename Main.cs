using Kitchen;
using KitchenData;
using KitchenLib;
using KitchenLib.Event;
using KitchenLib.References;
using KitchenMods;
using PreferenceSystem;
using System.CodeDom;
using System.Reflection;
using TMPro;
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

        public const string TIMER_ENABLED_ID = "Enabled";
        public const string TIMER_MODE_ID = "TimerRunDuring";
        public const string TIMER_RESET_MODE_ID = "AutomaticallyReset";
        public const string TIMER_PAUSE_MODE_ID = "FreezeTimerWhenGamePaused";
        public const string GROUPS_SERVED_ENABLED_ID = "GroupsServedEnabled";
        public const string GROUPS_REMAINING_ENABLED_ID = "GroupsRemainingEnabled";


        internal static PreferenceSystemManager PrefManager { get; private set; }
        internal static bool RequestReset = false;
        internal bool IsHost => Session.CurrentGameNetworkMode == GameNetworkMode.Host;



        public Main() : base(MOD_GUID, MOD_NAME, MOD_AUTHOR, MOD_VERSION, MOD_GAMEVERSION, Assembly.GetExecutingAssembly()) { }

        protected override void OnInitialise()
        {
        }

        protected override void OnUpdate()
        {
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


            Events.BuildGameDataPostViewInitEvent += delegate (object s, BuildGameDataEventArgs args)
            {
            };
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
