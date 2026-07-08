using Content.Server.Objectives.Components;
using Content.Shared.Objectives.Components;

namespace Content.Server.Objectives.Systems;

public sealed partial class FreeObjectiveSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
    }

    // You automatically greentext, there's not much else to it
    [SubscribeLocalEvent]
    private void OnGetProgress(Entity<FreeObjectiveComponent> ent, ref ObjectiveGetProgressEvent args)
    {
        args.Progress = 1f;
    }
}
