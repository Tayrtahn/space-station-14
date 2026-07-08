using Content.Server.Atmos.EntitySystems;
using Content.Shared.Atmos.Components;
using Content.Shared.Trigger;
using Content.Shared.Trigger.Components.Effects;

namespace Content.Server.Trigger.Systems;

/// <summary>
/// Trigger system for adding or removing fire stacks from an entity with <see cref="FlammableComponent"/>.
/// </summary>
/// <seealso cref="IgniteOnTriggerSystem"/>
public sealed partial class FireStackOnTriggerSystem : EntitySystem
{
    [Dependency] private FlammableSystem _flame = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
    }

    [SubscribeLocalEvent]
    private void OnTriggerFlame(Entity<FireStackOnTriggerComponent> ent, ref TriggerEvent args)
    {
        if (args.Key != null && !ent.Comp.KeysIn.Contains(args.Key))
            return;

        var target = ent.Comp.TargetUser ? args.User : ent.Owner;

        if (target == null)
            return;

        if (!TryComp<FlammableComponent>(target.Value, out var flammable))
            return;

        _flame.AdjustFireStacks(target.Value, ent.Comp.FireStacks, ignite: ent.Comp.DoIgnite, flammable: flammable);

        args.Handled = true;
    }

    [SubscribeLocalEvent]
    private void OnTriggerExtinguish(Entity<ExtinguishOnTriggerComponent> ent, ref TriggerEvent args)
    {
        if (args.Key != null && !ent.Comp.KeysIn.Contains(args.Key))
            return;

        var target = ent.Comp.TargetUser ? args.User : ent.Owner;

        if (target == null)
            return;

        if (!TryComp<FlammableComponent>(target.Value, out var flammable))
            return;

        _flame.Extinguish(target.Value, flammable: flammable);

        args.Handled = true;
    }
}
