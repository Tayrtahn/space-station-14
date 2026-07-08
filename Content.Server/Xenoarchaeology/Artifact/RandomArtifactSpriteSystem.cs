using Content.Shared.Item;
using Content.Shared.Xenoarchaeology.Artifact;
using Content.Shared.Xenoarchaeology.XenoArtifacts;
using Robust.Server.GameObjects;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server.Xenoarchaeology.Artifact;

public sealed partial class RandomArtifactSpriteSystem : EntitySystem
{
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private IGameTiming _time = default!;
    [Dependency] private AppearanceSystem _appearance = default!;
    [Dependency] private SharedItemSystem _item = default!;

    public override void Initialize()
    {
        base.Initialize();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<RandomArtifactSpriteComponent, AppearanceComponent>();
        while (query.MoveNext(out var uid, out var component, out var appearance))
        {
            if (component.ActivationStart == null)
                continue;

            var timeDif = _time.CurTime - component.ActivationStart.Value;
            if (timeDif.Seconds >= component.ActivationTime)
            {
                _appearance.SetData(uid, SharedArtifactsVisuals.IsActivated, false, appearance);
                component.ActivationStart = null;
            }
        }
    }

    [SubscribeLocalEvent]
    private void OnMapInit(EntityUid uid, RandomArtifactSpriteComponent component, MapInitEvent args)
    {
        var randomSprite = _random.Next(component.MinSprite, component.MaxSprite + 1);
        _appearance.SetData(uid, SharedArtifactsVisuals.SpriteIndex, randomSprite);
        _item.SetHeldPrefix(uid, "ano" + randomSprite.ToString("D2")); //set item artifact inhands
    }

    [SubscribeLocalEvent]
    private void UnlockingStageStarted(Entity<RandomArtifactSpriteComponent> ent, ref ArtifactUnlockingStartedEvent args)
    {
        _appearance.SetData(ent, SharedArtifactsVisuals.IsUnlocking, true);
    }

    [SubscribeLocalEvent]
    private void UnlockingStageFinished(Entity<RandomArtifactSpriteComponent> ent, ref ArtifactUnlockingFinishedEvent args)
    {
        _appearance.SetData(ent, SharedArtifactsVisuals.IsUnlocking, false);
    }

    [SubscribeLocalEvent]
    private void ArtifactActivated(Entity<RandomArtifactSpriteComponent> ent, ref XenoArtifactActivatedEvent args)
    {
        _appearance.SetData(ent, SharedArtifactsVisuals.IsActivated, true);
        ent.Comp.ActivationStart = _time.CurTime;
    }
}
