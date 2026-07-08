using Content.Shared.ActionBlocker;
using Content.Shared.Movement.Events;
using Content.Shared.Shuttles.BUIStates;
using Content.Shared.Shuttles.Components;
using Robust.Shared.Serialization;

namespace Content.Shared.Shuttles.Systems
{
    public abstract partial class SharedShuttleConsoleSystem : EntitySystem
    {
        [Dependency] protected ActionBlockerSystem ActionBlockerSystem = default!;

        public override void Initialize()
        {
            base.Initialize();
        }

        [Serializable, NetSerializable]
        protected sealed class PilotComponentState : ComponentState
        {
            public NetEntity? Console { get; }

            public PilotComponentState(NetEntity? uid)
            {
                Console = uid;
            }
        }

        [SubscribeLocalEvent]
        protected virtual void HandlePilotShutdown(EntityUid uid, PilotComponent component, ComponentShutdown args)
        {
            ActionBlockerSystem.UpdateCanMove(uid);
        }

        [SubscribeLocalEvent]
        private void OnStartup(EntityUid uid, PilotComponent component, ComponentStartup args)
        {
            ActionBlockerSystem.UpdateCanMove(uid);
        }

        [SubscribeLocalEvent]
        private void HandleMovementBlock(EntityUid uid, PilotComponent component, UpdateCanMoveEvent args)
        {
            if (component.LifeStage > ComponentLifeStage.Running)
                return;
            if (component.Console == null)
                return;

            args.Cancel();
        }
    }
}
