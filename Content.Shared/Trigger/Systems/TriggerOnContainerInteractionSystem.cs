using Content.Shared.Trigger.Components.Triggers;
using Robust.Shared.Containers;
using Robust.Shared.Timing;

namespace Content.Shared.Trigger.Systems;

/// <summary>
/// System for creating triggers when entities are inserted into or removed from containers.
/// </summary>
public sealed partial class TriggerOnContainerInteractionSystem : TriggerOnXSystem
{
    [Dependency] private IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();
    }

    // Used by containers to trigger when entities are inserted into or removed from them
    [SubscribeLocalEvent]
    private void OnInsertedIntoContainer(Entity<TriggerOnInsertedIntoContainerComponent> ent, ref EntInsertedIntoContainerMessage args)
    {
        if (_timing.ApplyingState)
            return;

        if (ent.Comp.ContainerId != null && ent.Comp.ContainerId != args.Container.ID)
            return;

        Trigger.Trigger(ent.Owner, args.Entity, ent.Comp.KeyOut);
    }

    [SubscribeLocalEvent]
    private void OnRemovedFromContainer(Entity<TriggerOnRemovedFromContainerComponent> ent, ref EntRemovedFromContainerMessage args)
    {
        if (_timing.ApplyingState)
            return;

        if (ent.Comp.ContainerId != null && ent.Comp.ContainerId != args.Container.ID)
            return;

        Trigger.Trigger(ent.Owner, args.Entity, ent.Comp.KeyOut);
    }

    // Used by entities to trigger when they are inserted into or removed from a container
    [SubscribeLocalEvent]
    private void OnGotInsertedIntoContainer(Entity<TriggerOnGotInsertedIntoContainerComponent> ent, ref EntGotInsertedIntoContainerMessage args)
    {
        if (_timing.ApplyingState)
            return;

        if (ent.Comp.ContainerId != null && ent.Comp.ContainerId != args.Container.ID)
            return;

        Trigger.Trigger(ent.Owner, args.Container.Owner, ent.Comp.KeyOut);
    }

    [SubscribeLocalEvent]
    private void OnGotRemovedFromContainer(Entity<TriggerOnGotRemovedFromContainerComponent> ent, ref EntGotRemovedFromContainerMessage args)
    {
        if (_timing.ApplyingState)
            return;

        if (ent.Comp.ContainerId != null && ent.Comp.ContainerId != args.Container.ID)
            return;

        Trigger.Trigger(ent.Owner, args.Container.Owner, ent.Comp.KeyOut);
    }
}
