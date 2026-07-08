using Content.Shared.ActionBlocker;
using Content.Shared.Hands;
using Content.Shared.Interaction.Events;
using Content.Shared.Item;
using Content.Shared.Emoting;
using Content.Shared.Movement.Events;

namespace Content.Shared.Puppet;

// TODO deduplicate with BlockMovementComponent
public abstract partial class SharedVentriloquistPuppetSystem : EntitySystem
{
    [Dependency] private ActionBlockerSystem _blocker = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<VentriloquistPuppetComponent, PickupAttemptEvent>(Cancel);
        SubscribeLocalEvent<VentriloquistPuppetComponent, UpdateCanMoveEvent>(Cancel);
        SubscribeLocalEvent<VentriloquistPuppetComponent, EmoteAttemptEvent>(Cancel);
        SubscribeLocalEvent<VentriloquistPuppetComponent, ChangeDirectionAttemptEvent>(Cancel);
    }

    [SubscribeLocalEvent]
    private void CancelInteract(Entity<VentriloquistPuppetComponent> ent, ref InteractionAttemptEvent args)
    {
        args.Cancelled = true;
    }

    [SubscribeLocalEvent]
    private void OnStartup(EntityUid uid, VentriloquistPuppetComponent component, ComponentStartup args)
    {
        _blocker.UpdateCanMove(uid);
    }

    [SubscribeLocalEvent]
    private void Cancel<T>(EntityUid uid, VentriloquistPuppetComponent component, T args) where T : CancellableEntityEventArgs
    {
        args.Cancel();
    }
}
