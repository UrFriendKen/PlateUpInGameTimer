using Kitchen;
using Unity.Collections;
using Unity.Entities;

namespace KitchenInGameTimer
{
    public struct SServedGroups : IComponentData
    {
        public int Count;
    }

    public struct CGroupLeavingRecorded : IComponentData { }

    public class UpdateServedGroups : RestaurantSystem
    {
        private EntityQuery LeavingGroups;

        protected override void Initialise()
        {
            base.Initialise();
            LeavingGroups = GetEntityQuery(new QueryHelper()
                .All(typeof(CGroupLeaving))
                .None(typeof(CGroupLeavingRecorded)));
        }

        protected override void OnUpdate()
        {
            NativeArray<Entity> entities = LeavingGroups.ToEntityArray(Allocator.Temp);
            SServedGroups servedGroups = GetOrCreate<SServedGroups>();
            if (Has<SIsNightTime>())
            {
                servedGroups.Count = 0;
                Set(servedGroups);
                return;
            }
            servedGroups.Count += entities.Length;
            Set(servedGroups);

            foreach (Entity entity in entities)
            {
                Set<CGroupLeavingRecorded>(entity);
            }
        }
    }
}
