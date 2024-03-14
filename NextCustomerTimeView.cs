using Kitchen;
using Kitchen.Modules;
using KitchenMods;
using MessagePack;
using Shapes;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace KitchenInGameTimer
{
    public class NextCustomerTimeView : UpdatableObjectView<NextCustomerTimeView.ViewData>
    {
        public class UpdateView : IncrementalViewSystemBase<ViewData>, IModSystem
        {
            EntityQuery Views;

            EntityQuery ScheduledGroups;

            protected override void Initialise()
            {
                base.Initialise();
                Views = GetEntityQuery(typeof(CTimeDisplay), typeof(CLinkedView));
                ScheduledGroups = GetEntityQuery(typeof(CScheduledCustomer));
            }

            protected override void OnUpdate()
            {
                ViewData data = new ViewData()
                {
                    IsNight = Has<SIsNightTime>()
                };

                if (!ScheduledGroups.IsEmpty)
                {
                    data.HasNextGroup = true;

                    float minArrivalTime = float.MaxValue;
                    using NativeArray<CScheduledCustomer> scheduledCustomers = ScheduledGroups.ToComponentDataArray<CScheduledCustomer>(Allocator.Temp);
                    for (int i = 0; i < scheduledCustomers.Length; i++)
                    {
                        float arrivalTime = scheduledCustomers[i].TimeOfDay;
                        if (arrivalTime >= minArrivalTime)
                            continue;
                        minArrivalTime = arrivalTime;
                    }

                    STime sTime = GetOrDefault<STime>();
                    float remainingTime = (minArrivalTime - sTime.TimeOfDay) * sTime.DayLength;

                    data.PercentDayArrivalTime = minArrivalTime;
                    data.TimeRemaining = remainingTime;
                }

                using NativeArray<CLinkedView> views = Views.ToComponentDataArray<CLinkedView>(Allocator.Temp);
                for (int i = 0; i < views.Length; i++)
                {
                    SendUpdate(views[i], data);
                }
            }
        }

        [MessagePackObject(false)]
        public class ViewData : ISpecificViewData, IViewData.ICheckForChanges<ViewData>
        {
            [Key(0)]
            public bool IsNight;

            [Key(1)]
            public bool HasNextGroup;

            [Key(2)]
            public float PercentDayArrivalTime;

            [Key(3)]
            public float TimeRemaining;

            public IUpdatableObject GetRelevantSubview(IObjectView view) => view.GetSubView<NextCustomerTimeView>();

            bool IViewData.ICheckForChanges<ViewData>.IsChangedFrom(ViewData check)
            {
                return HasNextGroup != check.HasNextGroup ||
                    PercentDayArrivalTime != check.PercentDayArrivalTime ||
                    TimeRemaining != check.TimeRemaining;
            }
        }

        public GameObject Container;

        public LabelElement Label;

        public Rectangle Background;

        ViewData Data;

        protected override void UpdateData(ViewData data)
        {
            Data = data;
        }

        void Update()
        {
            if (Data == null ||
                !Data.HasNextGroup ||
                Data.IsNight ||
                !Main.PrefManager.Get<bool>(Main.GROUPS_SHOW_NEXT_ARRIVAL_ID))
            {
                Container?.SetActive(false);
                return;
            }

            Container?.SetActive(true);
            SetIndicator(Data.PercentDayArrivalTime, Data.TimeRemaining);
        }

        private void SetIndicator(float fraction, float timeRemaining)
        {
            if (Container == null)
                return;

            float halfWidth = (Background?.Width ?? 0f) / 2f;
            float pos = Mathf.Lerp(-halfWidth, halfWidth, Mathf.Clamp01(fraction));
            Container.transform.localPosition = Vector3.right * pos;

            Label?.SetLabel($"{timeRemaining.ToString($"F{(timeRemaining < 3f ? 1 : 0)}")}s");
        }
    }
}
