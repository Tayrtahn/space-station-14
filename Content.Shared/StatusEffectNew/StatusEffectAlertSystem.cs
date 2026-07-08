using Content.Shared.Alert;
using Content.Shared.StatusEffectNew.Components;

namespace Content.Shared.StatusEffectNew;

/// <summary>
/// Handles displaying status effects that should show an alert, optionally with a duration.
/// </summary>
public sealed partial class StatusEffectAlertSystem : EntitySystem
{
    [Dependency] private AlertsSystem _alerts = default!;

    [Dependency] private EntityQuery<StatusEffectComponent> _effectQuery = default!;

    public override void Initialize()
    {
        base.Initialize();
    }

    [SubscribeLocalEvent]
    private void OnStatusEffectApplied(Entity<StatusEffectAlertComponent> ent, ref StatusEffectAppliedEvent args)
    {
        if (!_effectQuery.TryComp(ent, out var effectComp))
            return;

        _alerts.UpdateAlert(args.Target, ent.Comp.Alert, cooldown: ent.Comp.ShowDuration ? effectComp.EndEffectTime : null);
    }

    [SubscribeLocalEvent]
    private void OnStatusEffectRemoved(Entity<StatusEffectAlertComponent> ent, ref StatusEffectRemovedEvent args)
    {
        _alerts.ClearAlert(args.Target, ent.Comp.Alert);
    }

    [SubscribeLocalEvent]
    private void OnEndTimeUpdated(Entity<StatusEffectAlertComponent> ent, ref StatusEffectEndTimeUpdatedEvent args)
    {
        _alerts.UpdateAlert(args.Target, ent.Comp.Alert, cooldown: ent.Comp.ShowDuration ? args.EndTime : null);
    }
}
