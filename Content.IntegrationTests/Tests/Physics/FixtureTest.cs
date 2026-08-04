using Content.IntegrationTests.Fixtures;
using Content.IntegrationTests.Fixtures.Attributes;
using Robust.Shared.Physics;

namespace Content.IntegrationTests.Tests.Physics;

public sealed class FixtureTest : GameTest
{
    [Test]
    [RunOnSide(Side.Server)]
    public async Task ValidateFixtures()
    {
        var protos = Pair.GetPrototypesWithComponent<FixturesComponent>();

        using (Assert.EnterMultipleScope())
        {
            foreach (var (proto, fixturesComp) in protos)
            {
                foreach (var (fixtureId, fixture) in fixturesComp.Fixtures)
                {
                    if (!fixture.Hard)
                    {
                        Assert.That(
                            fixture.Density,
                            Is.Zero,
                            $"Entity {proto.ID} has a non-Hard fixture '{fixtureId}' with non-zero density");
                    }
                }
            }
        }
    }
}
