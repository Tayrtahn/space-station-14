using Content.Shared.Body.Events;
using Content.Shared.StatusEffectNew;

namespace Content.Shared.Traits.Assorted;

public sealed partial class HemophiliaSystem : EntitySystem
{
    public override void Initialize()
    {
    }

    [SubscribeLocalEvent]
    private void OnBleedModifier(Entity<HemophiliaStatusEffectComponent> ent, ref StatusEffectRelayedEvent<BleedModifierEvent> args)
    {
        var ev = args.Args;
        ev.BleedReductionAmount *= ent.Comp.BleedReductionMultiplier;
        ev.BleedAmount *= ent.Comp.BleedAmountMultiplier;
        args.Args = ev;
    }
}
