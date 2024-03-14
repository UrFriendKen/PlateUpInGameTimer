using Kitchen;
using Kitchen.Modules;
using KitchenInGameTimer.Modules;
using KitchenMods;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace KitchenInGameTimer
{
    public class InGameTimerView : UpdatableObjectView<InGameTimerView.ViewData>
    {
        public class UpdateView : IncrementalViewSystemBase<ViewData>, IModSystem
        {
            private EntityQuery Views;
            private EntityQuery QueuedGroups;
            private EntityQuery ScheduledGroups;

            protected override void Initialise()
            {
                base.Initialise();

                Views = GetEntityQuery(new QueryHelper()
                    .All(typeof(SDisplay), typeof(CLinkedView)));

                QueuedGroups = GetEntityQuery(new QueryHelper()
                    .All(typeof(CCustomerGroup), typeof(CGroupPhaseQueue)));

                ScheduledGroups = GetEntityQuery(new QueryHelper()
                    .All(typeof(CScheduledCustomer)));
            }

            protected override void OnUpdate()
            {
                using var views = Views.ToComponentDataArray<CLinkedView>(Allocator.Temp);

                TimeSpan duration = Require(out STimer timer) ? new TimeSpan((long)(timer.Seconds * 1E+07f)) : TimeSpan.Zero;
                int servedGroups = Require(out SServedGroups sServedGroups)? sServedGroups.Count : 0;
                int queuedGroups = QueuedGroups.CalculateEntityCount();
                int scheduledGroups = ScheduledGroups.CalculateEntityCount();

                for (var i = 0; i < views.Length; i++)
                {
                    Main.LogInfo(scheduledGroups);
                    var view = views[i];
                    SendUpdate(view, new ViewData()
                    {
                        Duration = duration,
                        ScheduledGroups = scheduledGroups,
                        QueueGroups = queuedGroups,
                        ServedGroups = servedGroups
                    });
                }
            }
        }

        [MessagePackObject(false)]
        public class ViewData : IViewData, IViewData.ICheckForChanges<ViewData>
        {
            [Key(0)] public TimeSpan Duration;
            [Key(1)] public int ServedGroups;
            [Key(2)] public int QueueGroups;
            [Key(3)] public int ScheduledGroups;

            public bool IsChangedFrom(ViewData check) =>
                Duration.GetHashCode() != check.Duration.GetHashCode() ||
                ScheduledGroups != check.ScheduledGroups ||
                QueueGroups != check.QueueGroups ||
                ServedGroups != check.ServedGroups;
        }

        private ViewData Data;

        public Transform Anchor;

        private Transform Container;

        private ModuleList ModuleList = new ModuleList();

        private List<Row> Rows = new List<Row>();

        public virtual ElementStyle Style { get; set; }

        protected Vector2 DefaultElementSize = new Vector2(3f, 0.45f);

        private class Row
        {
            private Func<string> _valueFunc;
            private InfoLabelElement _infoLabel;

            public InfoLabelElement InfoLabel => _infoLabel;

            public string Value => _valueFunc == null ? string.Empty : _valueFunc();

            public Row(InfoLabelElement infoLabel, Func<string> getValue)
            {
                _infoLabel = infoLabel;
                _valueFunc = getValue;
            }

            public void SetTitle(string title)
            {
                _infoLabel.SetTitle(title);
            }

            public void Update()
            {
                _infoLabel?.SetValue(Value);
            }
        }

        protected override void UpdateData(ViewData data)
        {
            Data = data;
        }

        List<bool> _oldPrefStates = new List<bool>();
        readonly List<string> _prefKeys = new List<string>()
        {
            Main.TIMER_ENABLED_ID,
            Main.GROUPS_SERVED_ENABLED_ID,
            Main.GROUPS_QUEUE_ENABLED_ID,
            Main.GROUPS_REMAINING_ENABLED_ID
        };

        void Update()
        {
            if (Data == null)
            {
                Container?.gameObject.SetActive(false);
                return;
            }

            bool ShouldRefreshModules()
            {
                if (Container == null)
                    return true;

                List<bool> prefStates = _prefKeys.Select(Main.PrefManager.Get<bool>).ToList();
                if (prefStates.Count != _oldPrefStates.Count)
                {
                    _oldPrefStates = prefStates;
                    return true;
                }

                for (int i = 0; i < prefStates.Count; i++)
                {
                    if (prefStates[i] == _oldPrefStates[i])
                        continue;
                    _oldPrefStates = prefStates;
                    return true;
                }

                return false;
            }

            if (ShouldRefreshModules())
            {
                if (Container == null)
                {
                    Container = new GameObject("Container").transform;
                    Container.SetParent(transform);
                    Container.Reset();
                    Container.localPosition = Anchor?.localPosition ?? Vector3.zero;
                }

                ModuleList.Clear();
                Rows.Clear();

                AddRowConditional("Time", Main.TIMER_ENABLED_ID, () =>
                {
                    TimeSpan duration = Data.Duration;
                    return $"{Math.Floor(duration.TotalHours):00}:{duration.Minutes:00}:{duration.Seconds:00}";
                });
                AddRowConditional("Served", Main.GROUPS_SERVED_ENABLED_ID, () => Data.ServedGroups.ToString());
                AddRowConditional("Queue", Main.GROUPS_QUEUE_ENABLED_ID, () => Data.QueueGroups.ToString());
                AddRowConditional("To Spawn", Main.GROUPS_REMAINING_ENABLED_ID, () => Data.ScheduledGroups.ToString());
            }

            foreach (Row row in Rows)
            {
                row.Update();
            }
        }

        Row AddRow(string title, Func<string> getValue)
        {
            Row row = new Row(AddInfoLabel(title), getValue);
            Rows.Add(row);
            return row;
        }

        void AddRowConditional(string title, string showPreferenceKey, Func<string> getValue)
        {
            if (!Main.PrefManager.Get<bool>(showPreferenceKey))
                return;
            AddRow(title, getValue);
        }

        protected virtual InfoLabelElement AddInfoLabel(string title)
        {
            InfoLabelElement infoLabelElement = New<InfoLabelElement>();
            infoLabelElement.SetSize(DefaultElementSize.x, DefaultElementSize.y);
            infoLabelElement.SetTitle(title);
            infoLabelElement.SetStyle(Style);
            infoLabelElement.SetAlignment(TMPro.TextAlignmentOptions.Right);
            return infoLabelElement;
        }

        protected virtual InfoLabelElement AddInfoLabel(string title, string value)
        {
            return AddInfoLabel(title).SetValue(value);
        }

        protected virtual InfoLabelElement AddInfoLabel(string title, string value, Color color)
        {
            InfoLabelElement infoLabelElement = AddInfoLabel(title, value);
            infoLabelElement.SetTextColor(color);
            return infoLabelElement;
        }

        protected virtual TElement New<TElement>(bool add_to_module_list = true) where TElement : Element
        {
            TElement val = ModuleDirectory.Add<TElement>(Container);
            if (add_to_module_list)
            {
                ModuleList.AddModule(val);
            }
            return val;
        }
    }
}
