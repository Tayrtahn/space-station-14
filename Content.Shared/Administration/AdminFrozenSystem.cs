using Content.Shared.ActionBlocker;
using Content.Shared.Chat;
using Content.Shared.Emoting;
using Content.Shared.Interaction.Events;
using Content.Shared.Item;
using Content.Shared.Movement.Events;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Movement.Pulling.Events;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Speech;
using Content.Shared.Throwing;

namespace Content.Shared.Administration;

// TODO deduplicate with BlockMovementComponent
public sealed partial class AdminFrozenSystem : EntitySystem
{
    [Dependency] private ActionBlockerSystem _blocker = default!;
    [Dependency] private PullingSystem _pulling = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<AdminFrozenComponent, ThrowAttemptEvent>(OnAttempt);
        SubscribeLocalEvent<AdminFrozenComponent, AttackAttemptEvent>(OnAttempt);
        SubscribeLocalEvent<AdminFrozenComponent, ChangeDirectionAttemptEvent>(OnAttempt);
    }

    /// <summary>
    /// Freezes and mutes the given entity.
    /// </summary>
    public void FreezeAndMute(EntityUid uid)
    {
        var comp = EnsureComp<AdminFrozenComponent>(uid);
        comp.Muted = true;
        Dirty(uid, comp);
    }

    [SubscribeLocalEvent]
    private void OnInteractAttempt(Entity<AdminFrozenComponent> ent, ref InteractionAttemptEvent args)
    {
        args.Cancelled = true;
    }

    [SubscribeLocalEvent]
    private void OnSpeakAttempt(EntityUid uid, AdminFrozenComponent component, SpeakAttemptEvent args)
    {
        if (!component.Muted)
            return;

        args.Cancel();
    }

    [SubscribeLocalEvent]
    private void OnInGameOocMessageAttempt(Entity<AdminFrozenComponent> ent, ref InGameOocMessageAttemptEvent args)
    {
        if (!ent.Comp.Muted)
            return;

        // Despite Type being available, Admin Mute does not care to differentiate. If you are out, you are out.
        args.Cancelled = true;
    }

    [SubscribeLocalEvent]
    private void OnInGameOocMessageAttemptBroadcast(ref InGameOocMessageAttemptEvent args)
    {
        //TODO Player LOOC mute/ban. Session is in the  args, but where to store/check the muted state?
    }

    [SubscribeLocalEvent]
    private void OnAttempt(EntityUid uid, AdminFrozenComponent component, CancellableEntityEventArgs args)
    {
        args.Cancel();
    }

    [SubscribeLocalEvent]
    private void OnPullAttempt(EntityUid uid, AdminFrozenComponent component, PullAttemptEvent args)
    {
        args.Cancelled = true;
    }

    [SubscribeLocalEvent]
    private void OnStartup(EntityUid uid, AdminFrozenComponent component, ComponentStartup args)
    {
        if (TryComp<PullableComponent>(uid, out var pullable))
        {
            _pulling.TryStopPull(uid, pullable);
        }

        UpdateCanMove(uid, component, args);
    }

    [SubscribeLocalEvent]
    private void OnUpdateCanMove(EntityUid uid, AdminFrozenComponent component, UpdateCanMoveEvent args)
    {
        if (component.LifeStage > ComponentLifeStage.Running)
            return;

        args.Cancel();
    }

    [SubscribeLocalEvent]
    private void UpdateCanMove(EntityUid uid, AdminFrozenComponent component, EntityEventArgs args)
    {
        _blocker.UpdateCanMove(uid);
    }

    [SubscribeLocalEvent]
    private void OnEmoteAttempt(EntityUid uid, AdminFrozenComponent component, EmoteAttemptEvent args)
    {
        if (component.Muted)
            args.Cancel();
    }
}
