using Content.Shared.Actions;
using Content.Shared.Atmos.Components;

namespace Content.Shared.Atmos.EntitySystems;

public abstract partial class SharedFirestarterSystem : EntitySystem
{
    [Dependency] private SharedActionsSystem _actionsSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
    }

    /// <summary>
    /// Adds the firestarter action.
    /// </summary>
    [SubscribeLocalEvent]
    private void OnComponentInit(EntityUid uid, FirestarterComponent component, ComponentInit args)
    {
        _actionsSystem.AddAction(uid, ref component.FireStarterActionEntity, component.FireStarterAction, uid);
        Dirty(uid, component);
    }
}
