using Content.IntegrationTests.Tests.Interaction;
using Content.Shared.Movement.Components;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;

namespace Content.IntegrationTests.Tests.Movement;

public sealed class InputMoverTest
{
    private const string TestEntityProtoId = "InputMoverTestEntity";

    [TestPrototypes]
    private const string Prototypes = $"""
- type: entity
  id: {TestEntityProtoId}
  components:
  - type: InputMover
""";

    [Test]
    public async Task DeleteGridTileTest()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings { Connected = true });
        var server = pair.Server;
        var entMan = server.EntMan;
        var mapSys = server.System<SharedMapSystem>();

        var mapData = await pair.CreateTestMap();

        await server.WaitPost(() =>
        {
            var moverEnt = entMan.SpawnEntity(TestEntityProtoId, mapData.GridCoords);
            var moverComp = entMan.GetComponent<InputMoverComponent>(moverEnt);

            mapSys.SetTile(mapData.Grid, mapData.GridCoords, Tile.Empty);

            entMan.Dirty(moverEnt, moverComp);
        });

        await pair.RunTicksSync(5);

        await pair.CleanReturnAsync();
    }
}
