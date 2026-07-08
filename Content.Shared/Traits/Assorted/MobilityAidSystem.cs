using Content.Shared.Hands;
using Content.Shared.Movement.Systems;
using Content.Shared.Wieldable;

namespace Content.Shared.Traits.Assorted;

/// <summary>
/// Handles <see cref="MobilityAidComponent"/>
/// </summary>
public sealed partial class MobilityAidSystem : EntitySystem
{
    [Dependency] private MovementSpeedModifierSystem _movementSpeedModifier = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
    }

    [SubscribeLocalEvent]
    private void OnGotEquippedHand(Entity<MobilityAidComponent> ent, ref GotEquippedHandEvent args)
    {
        _movementSpeedModifier.RefreshMovementSpeedModifiers(args.User);
    }

    [SubscribeLocalEvent]
    private void OnGotUnequippedHand(Entity<MobilityAidComponent> ent, ref GotUnequippedHandEvent args)
    {
        _movementSpeedModifier.RefreshMovementSpeedModifiers(args.User);
    }

    [SubscribeLocalEvent]
    private void OnMobilityAidWielded(Entity<MobilityAidComponent> ent, ref ItemWieldedEvent args)
    {
        _movementSpeedModifier.RefreshMovementSpeedModifiers(args.User);
    }

    [SubscribeLocalEvent]
    private void OnMobilityAidUnwielded(Entity<MobilityAidComponent> ent, ref ItemUnwieldedEvent args)
    {
        _movementSpeedModifier.RefreshMovementSpeedModifiers(args.User);
    }
}
