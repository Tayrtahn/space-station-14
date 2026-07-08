using Content.Server.DeviceLinking.Systems;
using Content.Server.Shuttles.Components;
using Content.Server.Shuttles.Events;

namespace Content.Server.Shuttles.Systems;

public sealed partial class DockingSignalControlSystem : EntitySystem
{
    [Dependency] private DeviceLinkSystem _deviceLinkSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
    }

    [SubscribeLocalEvent]
    private void OnDocked(Entity<DockingSignalControlComponent> ent, ref DockEvent args)
    {
        _deviceLinkSystem.SendSignal(ent, ent.Comp.DockStatusSignalPort, signal: true);
    }

    [SubscribeLocalEvent]
    private void OnUndocked(Entity<DockingSignalControlComponent> ent, ref UndockEvent args)
    {
        _deviceLinkSystem.SendSignal(ent, ent.Comp.DockStatusSignalPort, signal: false);
    }
}
