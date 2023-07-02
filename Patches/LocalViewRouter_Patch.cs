using HarmonyLib;
using Kitchen;
using TMPro;
using UnityEngine;

namespace KitchenInGameTimer.Patches
{
    [HarmonyPatch]
    static class LocalViewRouter_Patch
    {
        const float SCALE = 0.008f;
        const float ELEMENT_VERTICAL_OFFSET = -0.345f;

        [HarmonyPatch(typeof(LocalViewRouter), "GetPrefab")]
        [HarmonyPostfix]
        static void GetPrefab_Postfix(ref GameObject __result, ViewType view_type)
        {
            if (view_type == ViewType.DayDisplay && __result != null && __result.GetComponent<InGameTimerView>() == null)
            {
                Transform containerTransform = __result.transform.Find("GameObject (1)");
                if (containerTransform == null)
                {
                    Main.LogError("Failed to find \"GameObject (1)\" in DayDisplay.");
                    return;
                }

                GameObject speedrunTimer = containerTransform.Find("Speedrun Duration")?.gameObject;
                if (speedrunTimer == null)
                {
                    Main.LogError("Failed to find \"GameObject (1)\" in DayDisplay/GameObject (1).");
                    return;
                }

                GameObject inGameTimeContainer = new GameObject("InGameTimer");
                inGameTimeContainer.transform.ParentTo(containerTransform);

                GameObject offsetContainer = new GameObject("Offset");
                offsetContainer.transform.ParentTo(inGameTimeContainer);
                offsetContainer.transform.localPosition = new Vector3(-0.86f, -1.796f, -0.035f);

                InGameTimerView groupCounterView = __result.AddComponent<InGameTimerView>();

                GameObject timerText = GameObject.Instantiate(speedrunTimer);
                timerText.name = "Timer";
                timerText.transform.ParentTo(offsetContainer);
                timerText.transform.localScale = Vector3.one * SCALE;
                if (timerText.TryGetComponent<TextMeshPro>(out TextMeshPro timerTMP))
                {
                    groupCounterView.Timer = timerTMP;
                }

                GameObject groupsServedText = GameObject.Instantiate(speedrunTimer);
                groupsServedText.name = "Groups Served";
                groupsServedText.transform.ParentTo(offsetContainer);
                groupsServedText.transform.localPosition = new Vector3(-0f, ELEMENT_VERTICAL_OFFSET, 0f);
                groupsServedText.transform.localScale = Vector3.one * SCALE;
                if (groupsServedText.TryGetComponent<TextMeshPro>(out TextMeshPro groupsServedTMP))
                {
                    groupCounterView.GroupsServed = groupsServedTMP;
                }

                // To refresh drawing and correct local position (?) of previous instantiated GO for some reason
                GameObject temp = GameObject.Instantiate(speedrunTimer);
                temp.transform.ParentTo(offsetContainer);
                GameObject.Destroy(temp);

                Main.LogInfo("Added InGameTimer GameObjects to DayDisplay.");
            }
        }
    }
}
