using Kitchen;
using KitchenMods;
using Unity.Entities;
using UnityEngine;

namespace KitchenInGameTimer
{
    [UpdateInGroup(typeof(CreationGroup))]
    public class UpdateTimerMode : RestaurantSystem, IModSystem
    {
        protected override void OnUpdate()
        {
            if (!Main.PrefManager.Get<bool>(Main.TIMER_ENABLED_ID))
            {
                Clear<STimer>();
                return;
            }

            STimer timer = GetOrCreate<STimer>();

            string runDuring = Main.PrefManager.Get<string>(Main.TIMER_MODE_ID);
            timer.RunDay = runDuring == "ALWAYS_RUN" || runDuring == "DURING_DAY";
            timer.RunNight = runDuring == "ALWAYS_RUN" || runDuring == "DURING_NIGHT";

            string autoReset = Main.PrefManager.Get<string>(Main.TIMER_RESET_MODE_ID);
            timer.ResetStartOfDay = autoReset == "START_AND_END" || autoReset == "START_OF_DAY";
            timer.ResetStartOfNight = autoReset == "START_AND_END" || autoReset == "END_OF_DAY";

            timer.RunWhenPaused = !Main.PrefManager.Get<bool>(Main.TIMER_PAUSE_MODE_ID);

            Set(timer);
        }
    }
}
