using Content.Shared.Power.Components;
using Content.Shared.UserInterface;

namespace Content.Shared.Power.EntitySystems;

public abstract partial class SharedActivatableUIRequiresPowerSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
    }

    [SubscribeLocalEvent]
    protected abstract void OnActivate(Entity<ActivatableUIRequiresPowerComponent> ent, ref ActivatableUIOpenAttemptEvent args);
}
