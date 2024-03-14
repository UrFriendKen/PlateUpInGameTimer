using HarmonyLib;
using Kitchen;
using Kitchen.Modules;
using KitchenInGameTimer.Modules;
using Shapes;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace KitchenInGameTimer.Patches
{
    [HarmonyPatch]
    static class LocalViewRouter_Patch
    {
        static Transform _prefabContainer;

        static Dictionary<ViewType, GameObject> _viewPrefabs = new Dictionary<ViewType, GameObject>();

        [HarmonyPatch(typeof(LocalViewRouter), "GetPrefab")]
        [HarmonyPrefix]
        static bool GetPrefab_Prefix(ViewType view_type, ref GameObject __result)
        {
            if (_prefabContainer == null)
            {
                _prefabContainer = new GameObject("Timer Display Prefabs").transform;
                _prefabContainer.Reset();
                _prefabContainer.gameObject.SetActive(false);
            }

            if (_viewPrefabs.TryGetValue(view_type, out GameObject viewPrefab))
            {
                __result = viewPrefab;
                return false;
            }

            if (view_type == Main.TIMER_VIEW_TYPE)
            {
                viewPrefab = new GameObject("Timer");
                viewPrefab.transform.SetParent(_prefabContainer);
                viewPrefab.transform.Reset();

                Transform anchor = new GameObject("Anchor").transform;
                anchor.SetParent(viewPrefab.transform);
                anchor.transform.Reset();
                anchor.localPosition = new Vector3(-0.75f, 0f, 0f);

                InGameTimerView inGameTimerView = viewPrefab.AddComponent<InGameTimerView>();
                inGameTimerView.Anchor = anchor;
            }

            if (viewPrefab != null)
            {
                __result = viewPrefab;
                _viewPrefabs[view_type] = viewPrefab;
                return false;
            }
            return true;
        }

        [HarmonyPatch(typeof(LocalViewRouter), "GetPrefab")]
        [HarmonyPostfix]
        static void GetPrefab_Postfix(ViewType view_type, ref GameObject __result)
        {
            if (__result == null)
                return;

            if (view_type == ViewType.TimeDisplay)
            {
                if (!__result.GetComponent<NextCustomerTimeView>())
                {
                    LayerMask uiLayer = LayerMask.NameToLayer("UI");
                    Transform timeGroup = __result.transform.Find("Time");

                    GameObject anchor = new GameObject("Anchor");
                    anchor.layer = uiLayer;
                    anchor.transform.SetParent(timeGroup?.transform ?? __result.transform);
                    anchor.transform.Reset();
                    anchor.transform.localPosition = Vector3.forward * -0.055f;

                    GameObject container = new GameObject("Next Customer");
                    container.layer = uiLayer;
                    container.transform.SetParent(anchor.transform);
                    container.transform.Reset();

                    GameObject bar = new GameObject("Next Arrival");
                    bar.layer = uiLayer;
                    bar.transform.SetParent(container.transform);
                    bar.transform.Reset();
                    Rectangle rect = bar.AddComponent<Rectangle>();
                    rect.CornerRadius = 0.01f;
                    rect.Width = 0.03f;
                    rect.Height = 0.2f;
                    rect.Color = new Color(0.51f, 0.7f, 1f, 1f) * Mathf.Pow(1.2f, 2f);

                    InfoLabelElement label = null;
                    InfoLabelElement labelPrefab = ModuleDirectory.Main.GetPrefab<InfoLabelElement>();
                    if (labelPrefab)
                    {
                        label = Object.Instantiate(labelPrefab);
                        label.name = "Time Remaining";
                        label.gameObject.layer = uiLayer;
                        label.transform.SetParent(container.transform);
                        label.transform.localPosition = new Vector3(1.5f, 0.05f, 0f);
                        label.SetSize(3f, 0.4f);
                        label.SetStyle(ElementStyle.Default);
                        label.SetAlignment(TextAlignmentOptions.MidlineLeft);
                    }

                    NextCustomerTimeView nextCustomerTimeView = __result.AddComponent<NextCustomerTimeView>();
                    nextCustomerTimeView.Background = timeGroup?.Find("Grey")?.GetComponent<Rectangle>();
                    nextCustomerTimeView.Container = container;
                    nextCustomerTimeView.Label = label;
                }
                return;
            }
        }
    }
}
