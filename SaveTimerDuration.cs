using Kitchen;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;

namespace KitchenInGameTimer
{
    public class SaveTimerDuration : GameSystemBase
    {
        private float _duration;

        protected override void Initialise()
        {
            base.Initialise();
        }

        protected override void OnUpdate()
        {
            if (!Has<SPracticeMode>())
            {
                return;
            }
            _duration = GetOrDefault<STimer>().Seconds;
        }

        public override void AfterLoading(SaveSystemType system_type)
        {
            base.AfterLoading(system_type);
            
            if (Require<STimer>(out STimer timer))
            {
                timer.Seconds = _duration;
                Set(timer);
            }
        }
    }
}
