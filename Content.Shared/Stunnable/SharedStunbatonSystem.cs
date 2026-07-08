using Content.Shared.ActionBlocker;
using Content.Shared.Item.ItemToggle.Components;

namespace Content.Shared.Stunnable;

public abstract partial class SharedStunbatonSystem : EntitySystem
{
    [Dependency] private ActionBlockerSystem _actionBlocker = default!;

    public override void Initialize()
    {
        base.Initialize();
    }

    [SubscribeLocalEvent]
    protected virtual void TryTurnOn(Entity<StunbatonComponent> entity, ref ItemToggleActivateAttemptEvent args)
    {
        if (args.User != null && !_actionBlocker.CanComplexInteract(args.User.Value)) {
            args.Cancelled = true;
            return;
        }
    }

    [SubscribeLocalEvent]
    protected virtual void TryTurnOff(Entity<StunbatonComponent> entity, ref ItemToggleDeactivateAttemptEvent args)
    {
        if (args.User != null && !_actionBlocker.CanComplexInteract(args.User.Value)) {
            args.Cancelled = true;
            return;
        }
    }
}
