using Kitchen;
using KitchenMods;
using MessagePack;
using System;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Entities;

namespace KitchenInGameTimer
{
    public class InGameTimerView : UpdatableObjectView<InGameTimerView.ViewData>
    {
        public class UpdateView : IncrementalViewSystemBase<ViewData>, IModSystem
        {
            private EntityQuery Views;
            private EntityQuery ScheduledGroups;

            protected override void Initialise()
            {
                base.Initialise();

                Views = GetEntityQuery(new QueryHelper()
                    .All(typeof(CDayDisplay), typeof(CLinkedView)));

                ScheduledGroups = GetEntityQuery(new QueryHelper()
                    .All(typeof(CScheduledCustomer)));
            }

            protected override void OnUpdate()
            {
                using var views = Views.ToComponentDataArray<CLinkedView>(Allocator.Temp);

                TimeSpan duration = Require(out STimer timer) ? new TimeSpan((long)(timer.Seconds * 1E+07f)) : TimeSpan.Zero;
                int servedGroups = Require(out SServedGroups sServedGroups)? sServedGroups.Count : 0;
                int scheduledGroups = ScheduledGroups.CalculateEntityCount();

                for (var i = 0; i < views.Length; i++)
                {
                    var view = views[i];
                    SendUpdate(view, new ViewData()
                    {
                        Duration = duration,
                        ScheduledGroups = scheduledGroups,
                        ServedGroups = servedGroups
                    });
                }
            }
        }

        [MessagePackObject(false)]
        public class ViewData : ISpecificViewData, IViewData.ICheckForChanges<ViewData>
        {
            [Key(0)] public TimeSpan Duration;
            [Key(1)] public int ServedGroups;
            [Key(2)] public int ScheduledGroups;

            public IUpdatableObject GetRelevantSubview(IObjectView view) => view.GetSubView<InGameTimerView>();

            public bool IsChangedFrom(ViewData check) =>
                Duration.GetHashCode() != check.Duration.GetHashCode() ||
                ScheduledGroups != check.ScheduledGroups ||
                ServedGroups != check.ServedGroups;
        }

        private TimeSpan duration;
        private int servedGroups;
        private int remainingGroups;

        public TextMeshPro Timer;
        public TextMeshPro GroupsServed;

        protected override void UpdateData(ViewData data)
        {
            duration = data.Duration;
            servedGroups = data.ServedGroups;
            remainingGroups = data.ScheduledGroups;
        }

        void Update()
        {
            if (Timer)
            {
                string text = String.Empty;
                if (Main.PrefManager.Get<bool>(Main.TIMER_ENABLED_ID))
                    text = $"Time: {Math.Floor(duration.TotalHours):00}:{duration.Minutes:00}:{duration.Seconds:00}";
                Timer.text = text;
            }
            if (GroupsServed)
            {
                List<string> texts = new List<string>();
                if (Main.PrefManager.Get<bool>(Main.GROUPS_SERVED_ENABLED_ID))
                    texts.Add($"Served: {servedGroups}");
                if (Main.PrefManager.Get<bool>(Main.GROUPS_REMAINING_ENABLED_ID))
                    texts.Add($"Remaining: {remainingGroups}");
                GroupsServed.text = String.Join("\n", texts);
            }
        }
    }
}
