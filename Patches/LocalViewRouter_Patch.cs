using HarmonyLib;
using Kitchen;
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
    }
}
