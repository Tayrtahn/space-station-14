using Content.Client.PDA;
using Content.Shared.Clothing.Components;
using Content.Shared.Clothing.EntitySystems;
using Robust.Client.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.Client.Clothing.Systems;

// All valid items for chameleon are calculated on client startup and stored in dictionary.
public sealed partial class ChameleonClothingSystem : SharedChameleonClothingSystem
{
    public override void Initialize()
    {
        base.Initialize();

        PrepareAllVariants();
    }

    [SubscribeLocalEvent]
    private void OnProtoReloaded(PrototypesReloadedEventArgs args)
    {
        if (args.WasModified<EntityPrototype>())
            PrepareAllVariants();
    }

    [SubscribeLocalEvent]
    private void HandleState(EntityUid uid, ChameleonClothingComponent component, ref AfterAutoHandleStateEvent args)
    {
        UpdateVisuals(uid, component);
    }

    protected override void UpdateSprite(EntityUid uid, EntityPrototype proto)
    {
        base.UpdateSprite(uid, proto);
        if (TryComp(uid, out SpriteComponent? sprite)
            && proto.TryComp(out SpriteComponent? otherSprite, Factory))
        {
            sprite.CopyFrom(otherSprite);
        }

        // Edgecase for PDAs to include visuals when UI is open
        if (TryComp(uid, out PdaBorderColorComponent? borderColor)
            && proto.TryComp(out PdaBorderColorComponent? otherBorderColor, Factory))
        {
            borderColor.BorderColor = otherBorderColor.BorderColor;
            borderColor.AccentHColor = otherBorderColor.AccentHColor;
            borderColor.AccentVColor = otherBorderColor.AccentVColor;
        }
    }
}
