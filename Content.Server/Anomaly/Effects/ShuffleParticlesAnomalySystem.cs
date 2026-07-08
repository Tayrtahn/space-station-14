using Content.Server.Anomaly.Components;
using Content.Shared.Anomaly.Components;
using Robust.Shared.Random;

namespace Content.Server.Anomaly.Effects;

public sealed partial class ShuffleParticlesAnomalySystem : EntitySystem
{
    [Dependency] private AnomalySystem _anomaly = default!;
    [Dependency] private IRobustRandom _random = default!;

    public override void Initialize()
    {
    }

    [SubscribeLocalEvent]
    private void OnAffectedByParticle(Entity<ShuffleParticlesAnomalyComponent> ent, ref AnomalyAffectedByParticleEvent args)
    {
        if (!TryComp<AnomalyComponent>(ent, out var anomalyComp))
            return;

        if (ent.Comp.ShuffleOnParticleHit && _random.Prob(ent.Comp.Prob))
            _anomaly.ShuffleParticlesEffect((args.Anomaly, anomalyComp));
    }

    [SubscribeLocalEvent]
    private void OnPulse(Entity<ShuffleParticlesAnomalyComponent> ent, ref AnomalyPulseEvent args)
    {
        if (!TryComp<AnomalyComponent>(ent, out var anomaly))
            return;

        if (ent.Comp.ShuffleOnPulse && _random.Prob(ent.Comp.Prob))
        {
            _anomaly.ShuffleParticlesEffect((ent, anomaly));
        }
    }
}

