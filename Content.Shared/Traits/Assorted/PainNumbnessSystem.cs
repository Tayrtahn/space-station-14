using Content.Shared.Damage.Events;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Events;
using Content.Shared.Mobs.Systems;
using Content.Shared.StatusEffectNew;

namespace Content.Shared.Traits.Assorted;

public sealed partial class PainNumbnessSystem : EntitySystem
{
    [Dependency] private MobThresholdSystem _mobThresholdSystem = default!;

    public override void Initialize()
    {
    }

    [SubscribeLocalEvent]
    private void OnEffectApplied(Entity<PainNumbnessStatusEffectComponent> ent, ref StatusEffectAppliedEvent args)
    {
        if (!HasComp<MobThresholdsComponent>(args.Target))
            return;

        _mobThresholdSystem.VerifyThresholds(args.Target);
    }

    [SubscribeLocalEvent]
    private void OnEffectRemoved(Entity<PainNumbnessStatusEffectComponent> ent, ref StatusEffectRemovedEvent args)
    {
        if (!HasComp<MobThresholdsComponent>(args.Target))
            return;

        _mobThresholdSystem.VerifyThresholds(args.Target);
    }

    [SubscribeLocalEvent]
    private void OnChangeForceSay(Entity<PainNumbnessStatusEffectComponent> ent, ref StatusEffectRelayedEvent<BeforeForceSayEvent> args)
    {
        if (ent.Comp.ForceSayNumbDataset != null)
            args.Args.Prefix = ent.Comp.ForceSayNumbDataset.Value;
    }

    [SubscribeLocalEvent]
    private void OnAlertSeverityCheck(Entity<PainNumbnessStatusEffectComponent> ent, ref StatusEffectRelayedEvent<BeforeAlertSeverityCheckEvent> args)
    {
        if (args.Args.CurrentAlert == "HumanHealth")
            args.Args.CancelUpdate = true;
    }
}
