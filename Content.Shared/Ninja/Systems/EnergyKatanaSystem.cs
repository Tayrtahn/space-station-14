using Content.Shared.Inventory.Events;
using Content.Shared.Ninja.Components;

namespace Content.Shared.Ninja.Systems;

/// <summary>
/// System for katana binding and dash events. Recalling is handled by the suit.
/// </summary>
public sealed partial class EnergyKatanaSystem : EntitySystem
{
    [Dependency] private SharedSpaceNinjaSystem _ninja = default!;

    public override void Initialize()
    {
        base.Initialize();
    }

    /// <summary>
    /// When equipped by a ninja, try to bind it.
    /// </summary>
    [SubscribeLocalEvent]
    private void OnEquipped(Entity<EnergyKatanaComponent> ent, ref GotEquippedEvent args)
    {
        _ninja.BindKatana(args.EquipTarget, ent);
    }

    [SubscribeLocalEvent]
    private void OnCheckDash(Entity<EnergyKatanaComponent> ent, ref CheckDashEvent args)
    {
        // Just use a whitelist fam
        if (!_ninja.IsNinja(args.User))
            args.Cancelled = true;
    }
}
