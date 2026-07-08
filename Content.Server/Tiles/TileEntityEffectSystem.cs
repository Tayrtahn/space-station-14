using Content.Shared.StepTrigger.Systems;
using Content.Shared.EntityEffects;

namespace Content.Server.Tiles;

public sealed partial class TileEntityEffectSystem : EntitySystem
{
    [Dependency] private SharedEntityEffectsSystem _entityEffects = default!;

    public override void Initialize()
    {
        base.Initialize();
    }

    [SubscribeLocalEvent]
    private void OnTileStepTriggerAttempt(Entity<TileEntityEffectComponent> ent, ref StepTriggerAttemptEvent args)
    {
        args.Continue = true;
    }

    [SubscribeLocalEvent]
    private void OnTileStepTriggered(Entity<TileEntityEffectComponent> ent, ref StepTriggeredOffEvent args)
    {
        var otherUid = args.Tripper;

        _entityEffects.ApplyEffects(otherUid, ent.Comp.Effects.ToArray(), user: otherUid);
    }
}
