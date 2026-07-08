using Content.Shared.GPS.Components;

namespace Content.Shared.CartridgeLoader.Cartridges;

public sealed partial class AstroNavCartridgeSystem : EntitySystem
{
    [Dependency] private CartridgeLoaderSystem _cartridgeLoaderSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
    }

    [SubscribeLocalEvent]
    private void OnCartridgeAdded(Entity<AstroNavCartridgeComponent> ent, ref CartridgeAddedEvent args)
    {
        EnsureComp<HandheldGPSComponent>(args.Loader);
    }

    [SubscribeLocalEvent]
    private void OnCartridgeRemoved(Entity<AstroNavCartridgeComponent> ent, ref CartridgeRemovedEvent args)
    {
        // only remove when the program itself is removed
        if (!_cartridgeLoaderSystem.HasProgram<AstroNavCartridgeComponent>(args.Loader.AsNullable()))
        {
            RemComp<HandheldGPSComponent>(args.Loader);
        }
    }
}
