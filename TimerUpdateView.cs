using Kitchen;
using Unity.Collections;
using Unity.Entities;
using UnityEngine.SocialPlatforms.Impl;

namespace KitchenInGameTimer
{
    public class TimerUpdateView : IncrementalViewSystemBase<DayDisplayView.ViewData>
    {
        EntityQuery DayDisplay;

        static bool IsCustomTimer = false;

        int _day;
        bool _isPractice;
        bool _isNight;
        int _tier;
        int _setting;
        SFixedSeed _fixedSeed;
        bool _isSpeedrun;
        SpeedrunScore _score;
        NativeArray<CLinkedView> _linkedViews;

        protected override void Initialise()
        {
            base.Initialise();
            DayDisplay = GetEntityQuery(typeof(CLinkedView), typeof(CDayDisplay));
            RequireSingletonForUpdate<SKitchenStatus>();
        }

        protected override void OnUpdate()
        {
            if (!Main.PrefManager.Get<bool>("Enabled"))
            {
                if (IsCustomTimer)
                {
                    World.GetExistingSystem<DayDisplayView.UpdateView>().Enabled = true;
                    IsCustomTimer = false;
                }
                return;
            }
            World.GetExistingSystem<DayDisplayView.UpdateView>().Enabled = false;
            IsCustomTimer = true;

            _linkedViews = DayDisplay.ToComponentDataArray<CLinkedView>(Allocator.Temp);

            _day = GetOrDefault<SDay>().Day;
            _isPractice = Has<SPracticeMode>();
            _isNight = Has<SIsNightTime>();
            _tier = 0;
            if (Require(out CFranchiseTier franchiseTier))
            {
                _tier = franchiseTier.Tier;
            }
            _setting = Require(out CSetting cSetting) ? cSetting.RestaurantSetting : 0;
            Require(out _fixedSeed);
            _isSpeedrun = Main.PrefManager.Get<bool>("Enabled") && Has<STimer>();
            _score = (_isSpeedrun && Require(out STimer timer)) ? SpeedrunScore.FromSeconds(timer.Seconds) : new SpeedrunScore();

            SendViewUpdate();

            _linkedViews.Dispose();
        }

        private void SendViewUpdate()
        {
            for (int i = 0; i < _linkedViews.Length; i++)
            {
                CLinkedView linkedView = _linkedViews[i];
                SendUpdate(linkedView, new DayDisplayView.ViewData
                {
                    Day = _day,
                    IsPractice = _isPractice,
                    IsNight = _isNight,
                    Tier = _tier,
                    Seed = _fixedSeed.Seed,
                    CurrentSetting = _setting,
                    IsSpeedrun = _isSpeedrun,
                    SpeedrunScore = _score
                });
            }
        }
    }
}
