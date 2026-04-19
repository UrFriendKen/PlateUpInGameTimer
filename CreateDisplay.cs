using Kitchen;
using KitchenMods;
using System.Runtime.InteropServices;
using Unity.Entities;
using UnityEngine;

namespace KitchenInGameTimer
{
    [StructLayout(LayoutKind.Sequential, Size = 1)]
    public struct SDisplay : IComponentData, IModComponent { }

    public class CreateDisplay : RestaurantSystem, IModSystem
    {
        protected override void OnUpdate()
        {
            if (Has<SDisplay>())
                return;

            Entity singleton = EntityManager.CreateEntity(typeof(SDisplay), typeof(CDoNotPersist), typeof(CPosition), typeof(CRequiresView));
            Set(singleton, new CRequiresView()
            {
                Type = Main.TIMER_VIEW_TYPE,
                ViewMode = ViewMode.Screen,
                PhysicsDriven = false
            });
            Set(singleton, new CPosition()
            {
                Position = new Vector3(1f, 0.72f, 0f)
            });
        }
    }
}
