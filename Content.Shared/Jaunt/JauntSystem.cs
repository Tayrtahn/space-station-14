using Content.Shared.Actions;

namespace Content.Shared.Jaunt;
public sealed partial class JauntSystem : EntitySystem
{
    [Dependency] private SharedActionsSystem _actions = default!;

    public override void Initialize()
    {
        base.Initialize();
    }

    [SubscribeLocalEvent]
    private void OnInit(Entity<JauntComponent> ent, ref MapInitEvent args)
    {
        _actions.AddAction(ent.Owner, ref ent.Comp.Action, ent.Comp.JauntAction);
    }

    [SubscribeLocalEvent]
    private void OnShutdown(Entity<JauntComponent> ent, ref ComponentShutdown args)
    {
        _actions.RemoveAction(ent.Owner, ent.Comp.Action);
    }

}

