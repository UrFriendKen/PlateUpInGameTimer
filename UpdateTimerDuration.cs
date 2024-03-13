using Kitchen;
using KitchenMods;
using Unity.Entities;

namespace KitchenInGameTimer
{
    public struct STimer : IComponentData, IModComponent
    {
        public float Seconds;

        public bool RunDay;
        public bool RunNight;

        public bool ResetStartOfDay;
        public bool ResetStartOfNight;

        public bool RunWhenPaused;
    }

    [UpdateInGroup(typeof(TimeManagementGroup))]
    public class UpdateTimerDuration : RestaurantSystem, IModSystem
    {
        private EntityQuery Popups;

        protected override void Initialise()
        {
            base.Initialise();
            Popups = GetEntityQuery(typeof(CPopup));
            RequireSingletonForUpdate<STimer>();
        }

        protected override void OnUpdate()
        {
            STimer timer = GetSingleton<STimer>();

            if (Main.RequestReset)
            {
                timer.Seconds = 0;
                Main.RequestReset = false;
            }

            if (Has<SIsNightFirstUpdate>())
            {
                if (timer.ResetStartOfNight)
                    timer.Seconds = 0;
            }

            if (Has<SIsDayFirstUpdate>())
            {
                if (timer.ResetStartOfDay)
                    timer.Seconds = 0;
            }

            if (!Has<SGameOver>()
                && (!base.Time.IsPaused || timer.RunWhenPaused)
                && (Popups.IsEmpty || timer.RunWhenPaused))
            {
                if (((Has<SIsNightTime>() || Has<SPracticeMode>()) == timer.RunNight) ||
                    ((Has<SIsDayTime>() && !Has<SPracticeMode>()) == timer.RunDay))
                {
                    timer.Seconds += base.Time.RealDeltaTime;
                }
            }

            Set(timer);
        }
    }
}
