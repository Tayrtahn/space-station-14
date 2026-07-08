using Content.Server.Speech.Components;
using Content.Shared.Speech;

namespace Content.Server.Speech.EntitySystems;

public sealed partial class BlockListeningSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
    }

    [SubscribeLocalEvent]
    private void OnListenAttempt(EntityUid uid, BlockListeningComponent component, ListenAttemptEvent args)
    {
        args.Cancel();
    }
}
