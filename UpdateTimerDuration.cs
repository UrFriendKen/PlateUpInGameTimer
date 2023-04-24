using Kitchen;
using Unity.Entities;

namespace KitchenInGameTimer
{
    public struct STimer : IComponentData
    {
        public float Seconds;

        public bool RunDay;
        public bool RunNight;

        public bool ResetStartOfDay;
        public bool ResetStartOfNight;

        public bool RunWhenPaused;
    }

    [UpdateInGroup(typeof(TimeManagementGroup))]
    public class UpdateSpeedrunDuration : RestaurantSystem
    {
        private EntityQuery Popups;

        protected override void Initialise()
        {
            base.Initialise();
            Popups = GetEntityQuery(typeof(CPopup));
        }

        protected override void OnUpdate()
        {
            if (!Main.PrefManager.Get<bool>("Enabled"))
            {
                if (TryGetSingletonEntity<STimer>(out Entity timerEntity))
                {
                    EntityManager.DestroyEntity(timerEntity);
                }
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
