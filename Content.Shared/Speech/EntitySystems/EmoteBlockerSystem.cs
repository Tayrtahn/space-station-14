using Content.Shared.Chat;
using Content.Shared.Inventory;
using Content.Shared.Speech.Components;

namespace Content.Shared.Speech.EntitySystems;

public sealed partial class EmoteBlockerSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
    }

    [SubscribeLocalEvent]
    private static void OnRelayedEmoteEvent(Entity<EmoteBlockerComponent> entity, ref InventoryRelayedEvent<BeforeEmoteEvent> args)
    {
        OnEmoteEvent(entity, ref args.Args);
    }

    [SubscribeLocalEvent]
    private static void OnEmoteEvent(Entity<EmoteBlockerComponent> entity, ref BeforeEmoteEvent args)
    {
        if (entity.Comp.BlocksEmotes.Contains(args.Emote))
        {
            args.Cancel();
            args.Blocker = entity;
            return;
        }

        foreach (var blockedCat in entity.Comp.BlocksCategories)
        {
            if (blockedCat == args.Emote.Category)
            {
                args.Cancel();
                args.Blocker = entity;
                return;
            }
        }
    }
}
