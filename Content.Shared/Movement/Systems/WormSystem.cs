using Content.Shared.Alert;
using Content.Shared.Movement.Components;
using Content.Shared.Popups;
using Content.Shared.Rejuvenate;
using Content.Shared.Stunnable;

namespace Content.Shared.Movement.Systems;

/// <summary>
/// This handles the worm component
/// </summary>
public sealed partial class WormSystem : EntitySystem
{
    [Dependency] private AlertsSystem _alerts = default!;
    [Dependency] private SharedStunSystem _stun = default!;

    public override void Initialize()
    {
    }

    [SubscribeLocalEvent]
    private void OnMapInit(Entity<WormComponent> ent, ref MapInitEvent args)
    {
        EnsureComp<KnockedDownComponent>(ent, out var knocked);
        _alerts.ShowAlert(ent.Owner, SharedStunSystem.KnockdownAlert);
        _stun.SetAutoStand((ent, knocked));
    }

    [SubscribeLocalEvent]
    private void OnRejuvenate(Entity<WormComponent> ent, ref RejuvenateEvent args)
    {
        RemComp<WormComponent>(ent);
    }

    [SubscribeLocalEvent]
    private void OnStandAttempt(Entity<WormComponent> ent, ref StandUpAttemptEvent args)
    {
        if (args.Cancelled)
            return;

        args.Cancelled = true;
        args.Message = (Loc.GetString("worm-component-stand-attempt"), PopupType.SmallCaution);
        args.Autostand = false;
    }

    [SubscribeLocalEvent]
    private void OnKnockedDownRefresh(Entity<WormComponent> ent, ref KnockedDownRefreshEvent args)
    {
        args.FrictionModifier *= ent.Comp.FrictionModifier;
        args.SpeedModifier *= ent.Comp.SpeedModifier;
    }
}
