using Content.Shared.Trigger.Components.Triggers;
using Robust.Shared.Timing;
using Content.Shared.Inventory.Events;

namespace Content.Shared.Trigger.Systems;

/// <summary>
/// System for creating triggers when entities are equipped or unequipped from inventory slots.
/// </summary>
public sealed partial class TriggerOnEquipmentSystem : TriggerOnXSystem
{
    [Dependency] private IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();
    }

    // Used by entities when equipping or unequipping other entities
    [SubscribeLocalEvent]
    private void OnDidEquip(Entity<TriggerOnDidEquipComponent> ent, ref DidEquipEvent args)
    {
        if (_timing.ApplyingState)
            return;

        if ((ent.Comp.SlotFlags & args.SlotFlags) == 0)
            return;

        Trigger.Trigger(ent.Owner, args.Equipment, ent.Comp.KeyOut);
    }

    [SubscribeLocalEvent]
    private void OnDidUnequip(Entity<TriggerOnDidUnequipComponent> ent, ref DidUnequipEvent args)
    {
        if (_timing.ApplyingState)
            return;

        if ((ent.Comp.SlotFlags & args.SlotFlags) == 0)
            return;

        Trigger.Trigger(ent.Owner, args.Equipment, ent.Comp.KeyOut);
    }

    // Used by entities when they get equipped or unequipped
    [SubscribeLocalEvent]
    private void OnGotEquipped(Entity<TriggerOnGotEquippedComponent> ent, ref GotEquippedEvent args)
    {
        if (_timing.ApplyingState)
            return;

        if ((ent.Comp.SlotFlags & args.SlotFlags) == 0)
            return;

        Trigger.Trigger(ent.Owner, args.EquipTarget, ent.Comp.KeyOut);
    }

    [SubscribeLocalEvent]
    private void OnGotUnequipped(Entity<TriggerOnGotUnequippedComponent> ent, ref GotUnequippedEvent args)
    {
        if (_timing.ApplyingState)
            return;

        if ((ent.Comp.SlotFlags & args.SlotFlags) == 0)
            return;

        Trigger.Trigger(ent.Owner, args.EquipTarget, ent.Comp.KeyOut);
    }
}
