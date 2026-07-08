using Content.Shared.Throwing;
using Content.Shared.Trigger.Components.Triggers;

namespace Content.Shared.Trigger.Systems;

public sealed partial class TriggerOnLandSystem : TriggerOnXSystem
{
    public override void Initialize()
    {
        base.Initialize();
    }

    [SubscribeLocalEvent]
    private void OnLand(Entity<TriggerOnLandComponent> ent, ref LandEvent args)
    {
        Trigger.Trigger(ent.Owner, args.User, ent.Comp.KeyOut, predicted: false);
    }
}
