using Content.Shared.Item.ItemToggle;
using Content.Shared.Item.ItemToggle.Components;
using Content.Shared.PowerCell.Components;

namespace Content.Shared.PowerCell;

/// <summary>
/// Handles events to integrate PowerCellDraw with ItemToggle
/// </summary>
public sealed partial class ToggleCellDrawSystem : EntitySystem
{
    [Dependency] private ItemToggleSystem _toggle = default!;
    [Dependency] private PowerCellSystem _cell = default!;

    public override void Initialize()
    {
        base.Initialize();
    }

    [SubscribeLocalEvent]
    private void OnMapInit(Entity<ToggleCellDrawComponent> ent, ref MapInitEvent args)
    {
        _cell.SetDrawEnabled(ent.Owner, _toggle.IsActivated(ent.Owner));
    }

    [SubscribeLocalEvent]
    private void OnActivateAttempt(Entity<ToggleCellDrawComponent> ent, ref ItemToggleActivateAttemptEvent args)
    {
        if (!_cell.HasDrawCharge(ent.Owner, user: args.User, predicted: true)
            || !_cell.HasActivatableCharge(ent.Owner, user: args.User, predicted: true))
            args.Cancelled = true;
    }

    [SubscribeLocalEvent]
    private void OnToggled(Entity<ToggleCellDrawComponent> ent, ref ItemToggledEvent args)
    {
        _cell.SetDrawEnabled(ent.Owner, args.Activated);
    }

    [SubscribeLocalEvent]
    private void OnEmpty(Entity<ToggleCellDrawComponent> ent, ref PowerCellSlotEmptyEvent args)
    {
        _toggle.TryDeactivate(ent.Owner);
    }
}
