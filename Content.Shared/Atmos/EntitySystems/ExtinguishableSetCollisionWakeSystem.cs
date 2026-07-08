using Content.Shared.Atmos.Components;

namespace Content.Shared.Atmos.EntitySystems;

/// <summary>
/// Implements <see cref="ExtinguishableSetCollisionWakeComponent"/>.
/// </summary>
public sealed partial class ExtinguishableSetCollisionWakeSystem : EntitySystem
{
    [Dependency]
    private CollisionWakeSystem _collisionWake = null!;

    public override void Initialize()
    {
        base.Initialize();
    }

    [SubscribeLocalEvent]
    private void HandleExtinguished(Entity<ExtinguishableSetCollisionWakeComponent> ent, ref ExtinguishedEvent args)
    {
        _collisionWake.SetEnabled(ent, true);
    }

    [SubscribeLocalEvent]
    private void HandleIgnited(Entity<ExtinguishableSetCollisionWakeComponent> ent, ref IgnitedEvent args)
    {
        _collisionWake.SetEnabled(ent, false);
    }
}
