using Content.Shared.GameTicking;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Robust.Client.Player;
using Robust.Shared.Player;

namespace Content.Client.Overlays;

/// <summary>
/// This is a base system to make it easier to enable or disabling UI elements based on whether or not the player has
/// some component, either on their controlled entity on some worn piece of equipment.
/// </summary>
public abstract partial class EquipmentHudSystem<T> : EntitySystem where T : IComponent
{
    [Dependency] private IPlayerManager _player = default!;

    [ViewVariables]
    public bool IsActive { get; private set; }
    protected virtual SlotFlags TargetSlots => ~SlotFlags.POCKET;

    public override void Initialize()
    {
        base.Initialize();
    }

    private void Update(RefreshEquipmentHudEvent<T> ev)
    {
        IsActive = true;
        UpdateInternal(ev);
    }

    public void Deactivate()
    {
        if (!IsActive)
            return;

        IsActive = false;
        DeactivateInternal();
    }

    protected virtual void UpdateInternal(RefreshEquipmentHudEvent<T> args) { }

    protected virtual void DeactivateInternal() { }

    [SubscribeLocalEvent]
    private void OnStartup(Entity<T> ent, ref ComponentStartup args)
    {
        RefreshOverlay();
    }

    [SubscribeLocalEvent]
    private void OnRemove(Entity<T> ent, ref ComponentRemove args)
    {
        RefreshOverlay();
    }

    [SubscribeLocalEvent]
    private void OnPlayerAttached(LocalPlayerAttachedEvent args)
    {
        RefreshOverlay();
    }

    [SubscribeLocalEvent]
    private void OnPlayerDetached(LocalPlayerDetachedEvent args)
    {
        if (_player.LocalSession?.AttachedEntity is null)
            Deactivate();
    }

    [SubscribeLocalEvent]
    private void OnCompEquip(Entity<T> ent, ref GotEquippedEvent args)
    {
        RefreshOverlay();
    }

    [SubscribeLocalEvent]
    private void OnCompUnequip(Entity<T> ent, ref GotUnequippedEvent args)
    {
        RefreshOverlay();
    }

    [SubscribeLocalEvent]
    private void OnRoundRestart(RoundRestartCleanupEvent args)
    {
        Deactivate();
    }

    [SubscribeLocalEvent]
    protected virtual void OnRefreshEquipmentHud(Entity<T> ent, ref InventoryRelayedEvent<RefreshEquipmentHudEvent<T>> args)
    {
        OnRefreshComponentHud(ent, ref args.Args);
    }

    [SubscribeLocalEvent]
    protected virtual void OnRefreshComponentHud(Entity<T> ent, ref RefreshEquipmentHudEvent<T> args)
    {
        args.Active = true;
        args.Components.Add(ent.Comp);
    }

    protected void RefreshOverlay()
    {
        if (_player.LocalSession?.AttachedEntity is not { } entity)
            return;

        var ev = new RefreshEquipmentHudEvent<T>(TargetSlots);
        RaiseLocalEvent(entity, ref ev);

        if (ev.Active)
            Update(ev);
        else
            Deactivate();
    }
}
