#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using Content.IntegrationTests.Fixtures;
using Content.IntegrationTests.Fixtures.Attributes;
using Robust.Shared;
using Robust.Shared.Audio.Components;
using Robust.Shared.GameObjects;
using Robust.Shared.Log;
using Robust.Shared.Map;
using Robust.Shared.Maths;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.Manager.Attributes;
using Robust.Shared.Spawners;

namespace Content.IntegrationTests.Tests;

[TestOf(typeof(EntityUid))]
public sealed class EntityTest : GameTest
{
    private static readonly HashSet<ProtoId<EntityCategoryPrototype>> IgnoredCategories = ["Spawner", "Debug"];

    public override PoolSettings PoolSettings => new()
    {
        Connected = true,
        Dirty = true
    };

    public static PoolSettings Disconnected => new()
    {
        Dirty = true,
    };

    [Test]
    [PairConfig(nameof(Disconnected))]
    [Description("Spawns each EntityPrototype in isolation on its own map.")]
    public async Task SpawnAndDeleteAllEntitiesOnDifferentMaps()
    {
        // This test dirties the pair as it simply deletes ALL entities when done. Overhead of restarting the round
        // is minimal relative to the rest of the test.
        var mapSystem = Server.System<SharedMapSystem>();

        await Server.WaitPost(() =>
        {
            foreach (var proto in SProtoMan.EnumeratePrototypes<EntityPrototype>())
            {
                // Preconditions.
                if (proto.Abstract)
                    continue;

                if (Pair.IsTestPrototype(proto))
                    continue;

                // This will smash stuff otherwise.
                if (proto.Components.ContainsKey("MapGrid"))
                    continue;

                // This comp can delete all entities, and spawn others
                if (proto.Components.ContainsKey("RoomFill"))
                    continue;

                mapSystem.CreateMap(out var mapId);
                var grid = mapSystem.CreateGridEntity(mapId);
                // TODO: Fix this better in engine.
                mapSystem.SetTile(grid.Owner, grid.Comp, Vector2i.Zero, new Tile(1));
                var coord = new EntityCoordinates(grid.Owner, 0, 0);
                SEntMan.SpawnEntity(proto.ID, coord);
            }
        });

        await Server.WaitRunTicks(450); // 15 seconds, enough to trigger most update loops

        await Server.WaitPost(() =>
        {
            DeleteAllEntities(SEntMan);

            Assert.That(SEntMan.EntityCount, Is.Zero);
        });
    }

    [Test]
    [PairConfig(nameof(Disconnected))]
    [Description("Spawns each EntityPrototype on top of each other, tests interactions between entities for strange behavior.")]
    public async Task SpawnAndDeleteAllEntitiesInTheSameSpot()
    {
        Assume.That(Client.Session, Is.Null);
        var map = await Pair.CreateTestMap();

        await Server.WaitPost(() =>
        {
            foreach (var proto in SProtoMan.EnumeratePrototypes<EntityPrototype>())
            {
                // Preconditions.
                if (proto.Abstract)
                    continue;

                if (Pair.IsTestPrototype(proto))
                    continue;

                // This will smash stuff otherwise.
                if (proto.Components.ContainsKey("MapGrid"))
                    continue;

                // This comp can delete all entities, and spawn others
                if (proto.Components.ContainsKey("RoomFill"))
                    continue;

                SEntMan.SpawnEntity(proto.ID, map.GridCoords);
            }

            Server.RunTicks(450); // 15 seconds, enough to trigger most update loops
        });
        await Server.WaitPost(() =>
        {
            DeleteAllEntities(SEntMan);

            Assert.That(SEntMan.EntityCount, Is.Zero);
        });
    }

    /// <summary>
    /// Variant of <see cref="SpawnAndDeleteAllEntitiesOnDifferentMaps"/> that also launches a client and dirties
    /// all components on every entity.
    /// </summary>
    [Test]
    [Description("Spawns each entity on individual maps, dirties each component, and checks that the the client .")]
    public async Task SpawnAndDirtyAllEntities()
    {
        var sMapSys = Server.System<SharedMapSystem>();

        Assert.That(Server.CfgMan.GetCVar(CVars.NetPVS), Is.False);

        await Server.WaitPost(() =>
        {
            foreach (var proto in SProtoMan.EnumeratePrototypes<EntityPrototype>())
            {
                // Preconditions.
                if (proto.Abstract)
                    continue;

                if (Pair.IsTestPrototype(proto))
                    continue;

                // This will smash stuff otherwise.
                if (proto.Components.ContainsKey("MapGrid"))
                    continue;

                sMapSys.CreateMap(out var mapId);
                var grid = sMapSys.CreateGridEntity(mapId);
                var ent = SEntMan.SpawnEntity(proto.ID, new EntityCoordinates(grid.Owner, 0.5f, 0.5f));
                foreach (var (_, component) in SEntMan.GetNetComponents(ent))
                {
                    SEntMan.Dirty(ent, component);
                }
            }
        });

        await Pair.RunUntilSynced();

        // Make sure the client actually received the entities
        // 500 is completely arbitrary. Note that the client & sever entity counts aren't expected to match.
        Assert.That(CEntMan.EntityCount, Is.GreaterThan(500));

        await Server.WaitPost(() =>
        {
            DeleteAllEntities(SEntMan);

            Assert.That(SEntMan.EntityCount, Is.Zero);
        });
    }

    /// <summary>
    /// This test checks that spawning and deleting an entity doesn't somehow create other unrelated entities.
    /// </summary>
    /// <remarks>
    /// Unless an entity is intentionally designed to spawn other entities (e.g., mob spawners), they should
    /// generally not spawn unrelated / detached entities. Any entities that do get spawned should be parented to
    /// the spawned entity (e.g., in a container). If an entity needs to spawn an entity somewhere in null-space,
    /// it should delete that entity when it is no longer required. This test mainly exists to prevent "entity leak"
    /// bugs, where spawning some entity starts spawning unrelated entities in null space that stick around after
    /// the original entity is gone.
    ///
    /// Note that this isn't really a strict requirement, and there are probably quite a few edge cases. Its a pretty
    /// crude test to try catch issues like this, and possibly should just be disabled.
    /// </remarks>
    [Test]
    public async Task SpawnAndDeleteEntityCountTest()
    {
        var sMapSys = Server.System<SharedMapSystem>();
        var sAudioQuery = SEntMan.GetEntityQuery<AudioComponent>();
        var cAudioQuery = CEntMan.GetEntityQuery<AudioComponent>();

        var excluded = new[]
        {
            "MapGrid",
            "StationEvent",
            "TimedDespawn",

            // makes an announcement on mapInit.
            "AnnounceOnSpawn",
        };

        Assert.That(Server.CfgMan.GetCVar(CVars.NetPVS), Is.False);

        List<EntProtoId> protoIds = new(SProtoMan.Count<EntityPrototype>());
        foreach (var proto in SProtoMan.EnumeratePrototypes<EntityPrototype>())
        {
            // Preconditions
            if (proto.Abstract)
                continue;

            if (Pair.IsTestPrototype(proto))
                continue;

            var skip = false;
            foreach (var exclude in excluded)
            {
                if (proto.Components.ContainsKey(exclude))
                {
                    skip = true;
                    break;
                }
            }
            if (skip)
                continue;

            foreach (var category in proto.Categories)
            {
                if (IgnoredCategories.Contains(category.ID))
                {
                    skip = true;
                    break;
                }
            }
            if (skip)
                continue;

            protoIds.Add(proto.ID);
        }

        protoIds.Sort();
        var mapId = MapId.Nullspace;

        await Server.WaitPost(() =>
        {
            sMapSys.CreateMap(out mapId);
        });

        var coords = new MapCoordinates(Vector2.Zero, mapId);

        await Pair.RunTicksSync(3);

        // We consider only non-audio entities, as some entities will just play sounds when they spawn.
        int Count(IEntityManager ent) => ent.EntityCount - ent.Count<AudioComponent>();

        await Assert.MultipleAsync(async () =>
        {
            foreach (var protoId in protoIds)
            {
                var count = Count(SEntMan);
                var clientCount = Count(CEntMan);
                var serverEntities = GetEntitySet(SEntMan, sAudioQuery);
                var clientEntities = GetEntitySet(CEntMan, cAudioQuery);
                EntityUid uid = default;
                await Server.WaitPost(() => uid = SEntMan.SpawnEntity(protoId, coords));
                await Pair.RunTicksSync(3);

                // If the entity deleted itself, check that it didn't spawn other entities
                if (!SEntMan.EntityExists(uid))
                {
                    await CleanupTransientEntities(Pair, serverEntities);

                    Assert.That(Count(SEntMan), Is.EqualTo(count), $"Server prototype {protoId} failed on deleting itself\n" +
                        BuildDiffString(serverEntities, GetEntitySet(SEntMan, sAudioQuery), SEntMan));
                    Assert.That(Count(CEntMan), Is.EqualTo(clientCount), $"Client prototype {protoId} failed on deleting itself\n" +
                        $"Expected {clientCount} and found {CEntMan.EntityCount}.\n" +
                        $"Server count was {count}.\n" +
                        BuildDiffString(clientEntities, GetEntitySet(CEntMan, cAudioQuery), CEntMan));
                    continue;
                }

                // Check that the number of entities has increased.
                Assert.That(Count(SEntMan), Is.GreaterThan(count), $"Server prototype {protoId} failed on spawning as entity count didn't increase\n" +
                    BuildDiffString(serverEntities, GetEntitySet(SEntMan, sAudioQuery), SEntMan));
                Assert.That(Count(CEntMan), Is.GreaterThan(clientCount), $"Client prototype {protoId} failed on spawning as entity count didn't increase\n" +
                    $"Expected at least {clientCount} and found {CEntMan.EntityCount}. " +
                    $"Server count was {count}.\n" +
                    BuildDiffString(clientEntities, GetEntitySet(CEntMan, cAudioQuery), CEntMan));

                await Server.WaitPost(() => SEntMan.DeleteEntity(uid));
                await Pair.RunTicksSync(3);
                await CleanupTransientEntities(Pair, serverEntities);

                // Check that the number of entities has gone back to the original value.
                Assert.That(Count(SEntMan), Is.EqualTo(count), $"Server prototype {protoId} failed on deletion: count didn't reset properly\n" +
                    BuildDiffString(serverEntities, GetEntitySet(SEntMan, sAudioQuery), SEntMan));
                Assert.That(Count(CEntMan), Is.EqualTo(clientCount), $"Client prototype {protoId} failed on deletion: count didn't reset properly:\n" +
                    $"Expected {clientCount} and found {Count(CEntMan)}.\n" +
                    $"Server count was {count}.\n" +
                    BuildDiffString(clientEntities, GetEntitySet(CEntMan, cAudioQuery), CEntMan));
            }
        });
    }

    /// <summary>
    /// Returns a <see cref="HashSet{EntityUid}"/> of all entities a given entity manager tracks that don't have the AudioComponent.
    /// </summary>
    private static HashSet<EntityUid> GetEntitySet(IEntityManager entMan, EntityQuery<AudioComponent> query)
    {
        HashSet<EntityUid> entities = new(entMan.EntityCount);

        foreach (var ent in entMan.GetEntities())
        {
            if (!query.HasComp(ent))
                entities.Add(ent);
        }
        return entities;
    }

    /// <summary>
    /// Deletes all existing entities with the MetaDataComponent.
    /// </summary>
    private static void DeleteAllEntities(IEntityManager entMan)
    {
        List<(EntityUid, MetaDataComponent)> list = new(entMan.Count<MetaDataComponent>());
        var query = entMan.AllEntityQueryEnumerator<MetaDataComponent>();
        while (query.MoveNext(out var uid, out var meta))
        {
            list.Add((uid, meta));
        }

        foreach (var (uid, meta) in list)
        {
            if (!meta.EntityDeleted)
                entMan.DeleteEntity(uid);
        }
    }

    /// <summary>
    /// Deletes any entities with <see cref="TimedDespawnComponent"/> that were not present in the baseline snapshot.
    /// Some entities spawn transient side-effects on deletion (e.g. explosion visuals). These side-effect entities
    /// use TimedDespawn and would persist across test iterations, corrupting baseline entity counts and causing
    /// cascading assertion failures.
    /// </summary>
    private static async Task CleanupTransientEntities(Pair.TestPair pair, HashSet<EntityUid> baselineEntities)
    {
        var server = pair.Server;
        await server.WaitPost(() =>
        {
            var toRemove = new List<EntityUid>();
            var query = server.EntMan.AllEntityQueryEnumerator<TimedDespawnComponent>();
            while (query.MoveNext(out var uid, out _))
            {
                if (!baselineEntities.Contains(uid))
                    toRemove.Add(uid);
            }

            foreach (var uid in toRemove)
            {
                server.EntMan.DeleteEntity(uid);
            }
        });
        await pair.RunTicksSync(3);
    }

    private static string BuildDiffString(HashSet<EntityUid> oldEnts, HashSet<EntityUid> newEnts, IEntityManager entMan)
    {
        var sb = new StringBuilder();
        var addedEnts = newEnts.Except(oldEnts);
        var removedEnts = oldEnts.Except(newEnts);
        if (addedEnts.Any())
            sb.AppendLine("Listing new entities:");
        foreach (var addedEnt in addedEnts)
        {
            sb.AppendLine(entMan.ToPrettyString(addedEnt));
        }
        if (removedEnts.Any())
            sb.AppendLine("Listing removed entities:");
        foreach (var removedEnt in removedEnts)
        {
            sb.AppendLine("\t" + entMan.ToPrettyString(removedEnt));
        }
        return sb.ToString();
    }

    private static bool HasRequiredDataField(Component component)
    {
        foreach (var field in component.GetType().GetFields())
        {
            foreach (var attribute in field.GetCustomAttributes(true))
            {
                if (attribute is not DataFieldAttribute dataField)
                    continue;

                if (dataField.Required)
                    return true;
            }
        }
        foreach (var property in component.GetType().GetProperties())
        {
            foreach (var attribute in property.GetCustomAttributes(true))
            {
                if (attribute is not DataFieldAttribute dataField)
                    continue;

                if (dataField.Required)
                    return true;
            }
        }
        return false;
    }

    [Test]
    [Description("Tests removing and restoring components to a null entity.")]
    public async Task AllComponentsOneToOneDeleteTest()
    {
        var skipComponents = new[]
        {
            "DebugExceptionOnAdd", // Debug components that explicitly throw exceptions
            "DebugExceptionExposeData",
            "DebugExceptionInitialize",
            "DebugExceptionStartup",
            "GridFill",
            "RoomFill",
            "Map", // We aren't testing a map entity in this test
            "MapGrid",
            "Broadphase",
            "StationData", // errors when removed mid-round
            "StationJobs",
            "Actor", // We aren't testing actor components, those need their player session set.
            "BiomeSelection", // Whaddya know, requires config.
            "ActivatableUI", // Requires enum key
        };

        var componentFactory = Server.ResolveDependency<IComponentFactory>();
        var logmill = Server.ResolveDependency<ILogManager>().GetSawmill("EntityTest");

        await Pair.CreateTestMap();
        await Server.WaitRunTicks(5);
        var testLocation = TestMap!.GridCoords;

        await Server.WaitAssertion(() =>
        {
            Assert.Multiple(() =>
            {
                foreach (var type in componentFactory.AllRegisteredTypes)
                {
                    var component = (Component)componentFactory.GetComponent(type);
                    var name = componentFactory.GetComponentName(type);

                    if (HasRequiredDataField(component))
                        continue;

                    // If this component is ignored
                    if (skipComponents.Contains(name))
                    {
                        continue;
                    }

                    var entity = SEntMan.SpawnEntity(null, testLocation);

                    Assert.That(SComp<MetaDataComponent>(entity).EntityInitialized);

                    // The component may already exist if it is a mandatory component
                    // such as MetaData or Transform
                    if (SEntMan.HasComponent(entity, type))
                    {
                        SEntMan.DeleteEntity(entity);
                        continue;
                    }

                    logmill.Debug($"Adding component: {name}");

                    Assert.DoesNotThrow(() =>
                        {
                            SEntMan.AddComponent(entity, component);
                        }, "Component '{0}' threw an exception.",
                        name);

                    SEntMan.DeleteEntity(entity);
                }
            });
        });
    }
}
