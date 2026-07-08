using Content.Client.Items;
using Content.Client.Radiation.UI;
using Content.Shared.Radiation.Components;
using Content.Shared.Radiation.Systems;

namespace Content.Client.Radiation.Systems;

public sealed partial class GeigerSystem : SharedGeigerSystem
{
    public override void Initialize()
    {
        base.Initialize();
        Subs.ItemStatus<GeigerComponent>(ent => ent.Comp.ShowControl ? new GeigerItemControl(ent) : null);
    }

    [SubscribeLocalEvent]
    private void OnHandleState(EntityUid uid, GeigerComponent component, ref AfterAutoHandleStateEvent args)
    {
        component.UiUpdateNeeded = true;
    }
}
