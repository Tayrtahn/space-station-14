using Content.Shared.Roles.Components;

namespace Content.Server.Roles;

public sealed partial class RoleBriefingSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
    }

    [SubscribeLocalEvent]
    private void OnGetBriefing(EntityUid uid, RoleBriefingComponent comp, ref GetBriefingEvent args)
    {
        args.Append(Loc.GetString(comp.Briefing));
    }
}
