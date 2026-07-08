using System.Linq;
using Content.Server.Doors.Electronics;
using Content.Shared.Access;
using Content.Shared.Access.Components;
using Content.Shared.Access.Systems;
using Content.Shared.DeviceNetwork.Components;
using Content.Shared.Doors.Electronics;
using Content.Shared.Doors;
using Content.Shared.Interaction;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.Server.Doors.Electronics;

public sealed partial class DoorElectronicsSystem : EntitySystem
{
    [Dependency] private UserInterfaceSystem _uiSystem = default!;
    [Dependency] private AccessReaderSystem _accessReader = default!;

    public override void Initialize()
    {
        base.Initialize();
    }

    public void UpdateUserInterface(EntityUid uid, DoorElectronicsComponent component)
    {
        var accesses = new List<ProtoId<AccessLevelPrototype>>();

        if (TryComp<AccessReaderComponent>(uid, out var accessReader))
        {
            foreach (var accessList in accessReader.AccessLists)
            {
                var access = accessList.FirstOrDefault();
                accesses.Add(access);
            }
        }

        var state = new DoorElectronicsConfigurationState(accesses);
        _uiSystem.SetUiState(uid, DoorElectronicsConfigurationUiKey.Key, state);
    }

    [SubscribeLocalEvent]
    private void OnChangeConfiguration(
        EntityUid uid,
        DoorElectronicsComponent component,
        DoorElectronicsUpdateConfigurationMessage args)
    {
        var accessReader = EnsureComp<AccessReaderComponent>(uid);
        _accessReader.TrySetAccesses((uid, accessReader), args.AccessList);
    }

    [SubscribeLocalEvent]
    private void OnAccessReaderChanged(
        EntityUid uid,
        DoorElectronicsComponent component,
        AccessReaderConfigurationChangedEvent args)
    {
        UpdateUserInterface(uid, component);
    }

    [SubscribeLocalEvent]
    private void OnBoundUIOpened(
        EntityUid uid,
        DoorElectronicsComponent component,
        BoundUIOpenedEvent args)
    {
        UpdateUserInterface(uid, component);
    }
}
