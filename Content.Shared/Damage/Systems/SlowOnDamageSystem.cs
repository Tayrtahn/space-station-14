using Content.Shared.Clothing;
using Content.Shared.Damage.Components;
using Content.Shared.Examine;
using Content.Shared.FixedPoint;
using Content.Shared.Inventory;
using Content.Shared.Movement.Systems;

namespace Content.Shared.Damage.Systems;

public sealed partial class SlowOnDamageSystem : EntitySystem
{
    [Dependency] private MovementSpeedModifierSystem _movementSpeedModifierSystem = default!;
    [Dependency] private DamageableSystem _damage = default!;

    public override void Initialize()
    {
        base.Initialize();
    }

    [SubscribeLocalEvent]
    private void OnRefreshMovespeed(EntityUid uid, SlowOnDamageComponent component, RefreshMovementSpeedModifiersEvent args)
    {
        if (!TryComp<DamageableComponent>(uid, out var damage))
            return;

        var totalDamage = _damage.GetTotalDamage((uid, damage));

        if (totalDamage == FixedPoint2.Zero)
            return;

        // Get closest threshold
        FixedPoint2 closest = FixedPoint2.Zero;
        var total = totalDamage;
        foreach (var thres in component.SpeedModifierThresholds)
        {
            if (total >= thres.Key && thres.Key > closest)
                closest = thres.Key;
        }

        if (closest != FixedPoint2.Zero)
        {
            var speed = component.SpeedModifierThresholds[closest];

            var ev = new ModifySlowOnDamageSpeedEvent(speed);
            RaiseLocalEvent(uid, ref ev);
            args.ModifySpeed(ev.Speed, ev.Speed);
        }
    }

    [SubscribeLocalEvent]
    private void OnDamageChanged(EntityUid uid, SlowOnDamageComponent component, DamageChangedEvent args)
    {
        // We -could- only refresh if it crossed a threshold but that would kind of be a lot of duplicated
        // code and this isn't a super hot path anyway since basically only humans have this

        _movementSpeedModifierSystem.RefreshMovementSpeedModifiers(uid);
    }

    [SubscribeLocalEvent]
    private void OnModifySpeed(Entity<ClothingSlowOnDamageModifierComponent> ent, ref InventoryRelayedEvent<ModifySlowOnDamageSpeedEvent> args)
    {
        var dif = 1 - args.Args.Speed;
        if (dif <= 0)
            return;

        // reduces the slowness modifier by the given coefficient
        args.Args.Speed += dif * ent.Comp.Modifier;
    }

    [SubscribeLocalEvent]
    private void OnExamined(Entity<ClothingSlowOnDamageModifierComponent> ent, ref ExaminedEvent args)
    {
        var msg = Loc.GetString("slow-on-damage-modifier-examine", ("mod", (1 - ent.Comp.Modifier) * 100));
        args.PushMarkup(msg);
    }

    [SubscribeLocalEvent]
    private void OnGotEquipped(Entity<ClothingSlowOnDamageModifierComponent> ent, ref ClothingGotEquippedEvent args)
    {
        _movementSpeedModifierSystem.RefreshMovementSpeedModifiers(args.Wearer);
    }

    [SubscribeLocalEvent]
    private void OnGotUnequipped(Entity<ClothingSlowOnDamageModifierComponent> ent, ref ClothingGotUnequippedEvent args)
    {
        _movementSpeedModifierSystem.RefreshMovementSpeedModifiers(args.Wearer);
    }

    [SubscribeLocalEvent]
    private void OnIgnoreStartup(Entity<IgnoreSlowOnDamageComponent> ent, ref ComponentStartup args)
    {
        _movementSpeedModifierSystem.RefreshMovementSpeedModifiers(ent);
    }

    [SubscribeLocalEvent]
    private void OnIgnoreShutdown(Entity<IgnoreSlowOnDamageComponent> ent, ref ComponentShutdown args)
    {
        _movementSpeedModifierSystem.RefreshMovementSpeedModifiers(ent);
    }

    [SubscribeLocalEvent]
    private void OnIgnoreModifySpeed(Entity<IgnoreSlowOnDamageComponent> ent, ref ModifySlowOnDamageSpeedEvent args)
    {
        args.Speed = 1f;
    }
}

[ByRefEvent]
public record struct ModifySlowOnDamageSpeedEvent(float Speed) : IInventoryRelayEvent
{
    public SlotFlags TargetSlots => SlotFlags.WITHOUT_POCKET;
}
