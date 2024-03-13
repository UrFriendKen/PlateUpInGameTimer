using Kitchen;
using KitchenMods;
using Unity.Entities;

namespace KitchenInGameTimer
{
    public class MarkTemporaryEntities : GameSystemBase, IModSystem
    {
        EntityQuery Temps;

        protected override void Initialise()
        {
            base.Initialise();
            Temps = GetEntityQuery(new QueryHelper()
                .Any(typeof(SServedGroups))
                .None(typeof(CDoNotPersist)));
            RequireForUpdate(Temps);
        }

        protected override void OnUpdate()
        {
            EntityManager.AddComponent<CDoNotPersist>(Temps);
        }
    }
}
