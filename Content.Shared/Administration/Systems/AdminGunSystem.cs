using Content.Shared.Administration.Components;
using Content.Shared.Weapons.Ranged.Events;

namespace Content.Shared.Administration.Systems;

public sealed partial class AdminGunSystem : EntitySystem
{
    public override void Initialize()
    {
    }

    [SubscribeLocalEvent]
    private void OnGunRefreshModifiers(Entity<AdminMinigunComponent> ent, ref GunRefreshModifiersEvent args)
    {
        args.FireRate = 15;
    }
}
