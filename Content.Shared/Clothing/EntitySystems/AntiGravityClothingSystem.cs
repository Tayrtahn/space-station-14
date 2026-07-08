using Content.Shared.Clothing.Components;
using Content.Shared.Gravity;
using Content.Shared.Inventory;
using Content.Shared.Standing;

namespace Content.Shared.Clothing.EntitySystems;

/// <remarks>
/// We check standing state on all clothing because we don't want you to have anti-gravity unless you're standing.
/// This is for balance reasons as it prevents you from wearing anti-grav clothing to cheese being stun cuffed, as
/// well as other worse things.
/// </remarks>
public sealed partial class AntiGravityClothingSystem : EntitySystem
{
    [Dependency] private StandingStateSystem _standing = default!;
    [Dependency] private SharedGravitySystem _gravity = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
    }

    [SubscribeLocalEvent]
    private void OnIsWeightless(Entity<AntiGravityClothingComponent> ent, ref InventoryRelayedEvent<IsWeightlessEvent> args)
    {
        if (args.Args.Handled || _standing.IsDown(args.Owner))
            return;

        args.Args.Handled = true;
        args.Args.IsWeightless = true;
    }

    [SubscribeLocalEvent]
    private void OnEquipped(Entity<AntiGravityClothingComponent> entity, ref ClothingGotEquippedEvent args)
    {
        // This clothing item does nothing if we're not standing
        if (_standing.IsDown(args.Wearer))
            return;

        _gravity.RefreshWeightless(args.Wearer, true);
    }

    [SubscribeLocalEvent]
    private void OnUnequipped(Entity<AntiGravityClothingComponent> entity, ref ClothingGotUnequippedEvent args)
    {
        // This clothing item does nothing if we're not standing
        if (_standing.IsDown(args.Wearer))
            return;

        _gravity.RefreshWeightless(args.Wearer, false);
    }

    [SubscribeLocalEvent]
    private void OnDowned(Entity<AntiGravityClothingComponent> entity, ref InventoryRelayedEvent<DownedEvent> args)
    {
        _gravity.RefreshWeightless(args.Owner, false);
    }

    [SubscribeLocalEvent]
    private void OnStood(Entity<AntiGravityClothingComponent> entity, ref InventoryRelayedEvent<StoodEvent> args)
    {
        _gravity.RefreshWeightless(args.Owner, true);
    }
}
