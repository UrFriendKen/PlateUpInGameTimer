using HarmonyLib;
using Kitchen;
using Kitchen.Modules;
using KitchenInGameTimer.Modules;
using KitchenInGameTimer.Utils;
using KitchenMods;
using PreferenceSystem;
using System;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;

// Namespace should have "Kitchen" in the beginning
namespace KitchenInGameTimer
{
    public class Main : IModInitializer
    {
        public const string MOD_GUID = "IcedMilo.PlateUp.InGameTimer";
        public const string MOD_NAME = "In-Game Timer";
        public const string MOD_VERSION = "0.1.6";

        internal const string TIMER_ENABLED_ID = "Enabled";
        internal const string TIMER_MODE_ID = "TimerRunDuring";
        internal const string TIMER_RESET_MODE_ID = "AutomaticallyReset";
        internal const string TIMER_PAUSE_MODE_ID = "FreezeTimerWhenGamePaused";
        internal const string GROUPS_SERVED_ENABLED_ID = "GroupsServedEnabled";
        internal const string GROUPS_QUEUE_ENABLED_ID = "GroupsQueueEnabled";
        internal const string GROUPS_REMAINING_ENABLED_ID = "GroupsRemainingEnabled";
        internal const string GROUPS_SHOW_NEXT_ARRIVAL_ID = "GroupsShowNextArrivalEnabled";

        internal static readonly ViewType TIMER_VIEW_TYPE = (ViewType)HashUtils.GetID($"{MOD_GUID}:TimerViewType");

        internal static PreferenceSystemManager PrefManager { get; private set; }
        internal static bool RequestReset = false;
        internal bool IsHost => Session.CurrentGameNetworkMode == GameNetworkMode.Host;

        public Main()
        {
            Harmony harmony = new Harmony(MOD_GUID);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
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
                    .AddLabel("Show Queue Groups")
                    .AddOption<bool>(
                        GROUPS_QUEUE_ENABLED_ID,
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
                    .AddSpacer()
                    .AddLabel("Show Next Group Arrival Indicator")
                    .AddOption<bool>(
                        GROUPS_SHOW_NEXT_ARRIVAL_ID,
                        true,
                        new bool[] { false, true },
                        new string[] { "Disabled", "Enabled" })
                .SubmenuDone()
                .AddSpacer()
                .AddSpacer();

            PrefManager.RegisterMenu(PreferenceSystemManager.MenuType.PauseMenu);
        }

        static readonly FieldInfo f_DefaultModule = typeof(ModuleDirectory).GetField("DefaultModule", BindingFlags.NonPublic | BindingFlags.Instance);
        static readonly FieldInfo f_GenericAnimator = typeof(Element).GetField("GenericAnimator", BindingFlags.NonPublic | BindingFlags.Instance);
        static readonly FieldInfo f_Label = typeof(LabelElement).GetField("Label", BindingFlags.NonPublic | BindingFlags.Instance);
        static readonly FieldInfo f_LabelTransform = typeof(LabelElement).GetField("LabelTransform", BindingFlags.NonPublic | BindingFlags.Instance);
        public void PreInject()
        {
        }

        public void PostInject()
        {
            if (ModuleDirectory.Main)
            {
                Dictionary<Type, Element> defaultModule = (Dictionary<Type, Element>)f_DefaultModule.GetValue(ModuleDirectory.Main);
                GameObject labelPrefab = ModuleDirectory.Main.GetPrefab<LabelElement>()?.gameObject;
                if (defaultModule != null &&
                    labelPrefab != null &&
                    !defaultModule.ContainsKey(typeof(InfoLabelElement)))
                {
                    GameObject infoLabelElementPrefab = GameObject.Instantiate(labelPrefab);
                    infoLabelElementPrefab.name = "InfoLabel";
                    infoLabelElementPrefab.SetActive(false);

                    LabelElement toDestroy = infoLabelElementPrefab.GetComponent<LabelElement>();
                    if (toDestroy)
                        Component.DestroyImmediate(toDestroy);

                    InfoLabelElement infoLabelElement = infoLabelElementPrefab.AddComponent<InfoLabelElement>();
                    defaultModule[typeof(InfoLabelElement)] = infoLabelElement;

                    Animator genericAnimator = infoLabelElementPrefab.GetComponent<Animator>();
                    if (genericAnimator)
                        f_GenericAnimator?.SetValue(infoLabelElement, genericAnimator);

                    TextMeshPro labelTMP = infoLabelElementPrefab.transform.Find("Title")?.GetComponent<TextMeshPro>();
                    if (labelTMP)
                    {
                        labelTMP.color = Color.white;
                        labelTMP.text = string.Empty;
                        f_Label?.SetValue(infoLabelElement, labelTMP);
                    }

                    RectTransform rectTransform = infoLabelElementPrefab.transform.Find("Title")?.GetComponent<RectTransform>();
                    if (rectTransform)
                    {
                        f_LabelTransform?.SetValue(infoLabelElement, rectTransform);
                    }

                    infoLabelElementPrefab.SetActive(true);
                }
            }
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
