using System;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Necrom.FirstPlayable.Tests
{
    public sealed class FirstPlayableHybridDefenseRuntimeTests
    {
        private static int _id;

        [UnityTest]
        public IEnumerator DefeatRaiseAndExactAllyContributionClearOneWave()
        {
            var f = BuildFixture();

            var lethal = Invoke(
                f.Pipeline,
                "Apply",
                f.Enemy1,
                4,
                NewEntityId("source:hybrid:first"));
            Assert.That(
                Read(lethal, "BecameDefeated"),
                Is.EqualTo(true));

            Invoke(f.Defense, "RecordDefeatedThreat", f.Enemy1);

            Assert.That(
                Read(f.App, "BattlePhase").ToString(),
                Is.EqualTo("Running"),
                "The first defeated threat must not resolve the whole wave.");
            Assert.That(ReadInt(f.Defense, "ActiveEnemyCount"), Is.EqualTo(1));
            Assert.That(Read(f.Enemies, "CurrentTarget"), Is.SameAs(f.Enemy1));

            Assert.That(TryAcquire(f.Targeting, out var next), Is.True);
            Assert.That(next, Is.SameAs(f.Enemy2));

            Invoke(f.RaiseAction, "Execute");
            Assert.That(ReadInt(f.Roster, "ActiveCount"), Is.EqualTo(1));

            var ally = Invoke(f.Roster, "GetSlot", 0);
            var allyId = Read(Read(ally, "Model"), "Id");

            Invoke(f.AlliedLoop, "Advance", 0.100f);
            yield return null;

            Assert.That(
                Read(f.Defense, "Phase").ToString(),
                Is.EqualTo("Cleared"));
            Assert.That(
                Read(f.App, "BattlePhase").ToString(),
                Is.EqualTo("Victory"));
            Assert.That(ReadInt(f.Enemies, "ActiveThreatCount"), Is.Zero);

            var observed = Read(f.Session, "ObservedContribution");
            Assert.That(observed, Is.Not.Null);
            Assert.That(
                Read(observed, "ActingUnitId").ToString(),
                Is.EqualTo(allyId.ToString()),
                "The actual raised ally must own the contribution receipt.");

            var hud = Invoke(f.Defense, "CaptureHudState");
            Assert.That(Read(hud, "Phase").ToString(), Is.EqualTo("Cleared"));
            Assert.That(ReadInt(hud, "WaveNumber"), Is.EqualTo(1));
            Assert.That(ReadInt(hud, "GateIntegrity"), Is.EqualTo(10));
            Assert.That(ReadInt(hud, "ActiveEnemyCount"), Is.Zero);
            Assert.That(ReadInt(hud, "RemainingEnemiesToSpawn"), Is.Zero);

            UnityEngine.Object.Destroy(f.Root);
            yield return null;
        }

        [UnityTest]
        public IEnumerator GateBreachesDriveRunningThenFailureTruth()
        {
            var f = BuildFixture();

            Invoke(f.Defense, "RecordGateBreach", f.Enemy1, 4);

            Assert.That(
                Read(f.Defense, "Phase").ToString(),
                Is.EqualTo("Running"));
            Assert.That(ReadInt(f.Defense, "GateIntegrity"), Is.EqualTo(6));
            Assert.That(ReadInt(f.Defense, "ActiveEnemyCount"), Is.EqualTo(1));
            Assert.That(
                Read(f.App, "BattlePhase").ToString(),
                Is.EqualTo("Running"));
            Assert.That(TryAcquire(f.Targeting, out var next), Is.True);
            Assert.That(next, Is.SameAs(f.Enemy2));

            Invoke(f.Defense, "RecordGateBreach", f.Enemy2, 6);

            Assert.That(
                Read(f.Defense, "Phase").ToString(),
                Is.EqualTo("Failed"));
            Assert.That(ReadInt(f.Defense, "GateIntegrity"), Is.Zero);
            Assert.That(ReadInt(f.Defense, "ActiveEnemyCount"), Is.Zero);
            Assert.That(
                Read(f.App, "BattlePhase").ToString(),
                Is.EqualTo("Defeat"));

            var hud = Invoke(f.Defense, "CaptureHudState");
            Assert.That(Read(hud, "Phase").ToString(), Is.EqualTo("Failed"));
            Assert.That(ReadInt(hud, "GateIntegrity"), Is.Zero);
            Assert.That(ReadInt(hud, "GateMaxIntegrity"), Is.EqualTo(10));

            UnityEngine.Object.Destroy(f.Root);
            yield return null;
        }

        private static Fixture BuildFixture()
        {
            _id = 0;
            var root = new GameObject(
                "HybridDefenseRuntime",
                typeof(RectTransform));
            var enemyZone = NewZone(root.transform, "EnemySpawnZone");
            var alliedZone = NewZone(root.transform, "AlliedSpawnZone");

            var enemies = root.AddComponent(
                RequireType(
                    "Necrom.FirstPlayable.Runtime.EnemySpawnController"));

            var formation = Activator.CreateInstance(
                RequireType("Necrom.Core.Domain.Formation"));
            var battle = Activator.CreateInstance(
                RequireType("Necrom.Core.Domain.BattleStateMachine"));
            var encounter = Activator.CreateInstance(
                RequireType("Necrom.Core.Domain.FirstPlayableEncounter"),
                Activator.CreateInstance(
                    RequireType("Necrom.Core.Domain.RaiseService")),
                battle,
                formation);
            var app = Activator.CreateInstance(
                RequireType(
                    "Necrom.Core.Application.FirstPlayableApplicationService"),
                encounter);
            var progression = Activator.CreateInstance(
                RequireType(
                    "Necrom.Core.Application.FirstPlayableProgression"),
                app,
                encounter);

            var input = root.AddComponent(
                RequireType(
                    "Necrom.FirstPlayable.Runtime.FirstPlayableRaiseCommandInputHook"));
            Invoke(input, "Initialize", progression);

            var roster = root.AddComponent(
                RequireType(
                    "Necrom.FirstPlayable.Runtime.FirstPlayableAlliedRosterController"));
            Invoke(roster, "Initialize", formation, alliedZone);

            var raiseAction = root.AddComponent(
                RequireType(
                    "Necrom.FirstPlayable.Runtime.FirstPlayableRaiseActionController"));
            var raiseInit = raiseAction.GetType().GetMethod("Initialize");
            var rip = raiseInit.GetParameters();
            raiseInit.Invoke(
                raiseAction,
                new object[]
                {
                    enemies,
                    input,
                    BuildRaiseCommandFactory(
                        rip[2].ParameterType,
                        formation),
                    (Func<string>)(() => NextId("raised")),
                    (Func<string>)(() => NextId("assigned"))
                });

            var activation =
                raiseAction.GetType().GetMethod(
                    "ConfigureAlliedActivation");
            activation.Invoke(
                raiseAction,
                new object[]
                {
                    roster,
                    BuildBehaviorSpecProvider(
                        activation.GetParameters()[1].ParameterType,
                        damage: 4,
                        intervalMs: 100)
                });

            var session = Activator.CreateInstance(
                RequireType(
                    "Necrom.FirstPlayable.Runtime.FirstPlayableCombatHudSession"));
            Invoke(raiseAction, "ConfigureHudSession", session);

            var necroObject = new GameObject(
                "HybridNecromancer",
                typeof(RectTransform));
            necroObject.transform.SetParent(root.transform, false);
            var necro = necroObject.AddComponent(
                RequireType(
                    "Necrom.FirstPlayable.Runtime.NecromancerRuntimeEntity"));
            Invoke(
                necro,
                "Initialize",
                NewCombatant(
                    "player:hybrid",
                    "necromancer.prototype",
                    "Player",
                    100));

            var targeting = root.AddComponent(
                RequireType(
                    "Necrom.FirstPlayable.Runtime.FirstPlayableTargetingController"));
            Invoke(targeting, "Initialize", necro, enemies);

            var pipeline = Activator.CreateInstance(
                RequireType(
                    "Necrom.FirstPlayable.Runtime.FirstPlayableDamageDeathPipeline"),
                (Func<string>)(() => NextId("damage")),
                (Func<string>)(() => NextId("defeat")));

            var defense = root.AddComponent(
                RequireType(
                    "Necrom.FirstPlayable.Runtime.FirstPlayableDefenseWaveRuntimeController"));
            Invoke(
                defense,
                "Initialize",
                app,
                enemies,
                10,
                (Func<string>)(() => NextId("resolve:wave")));
            Invoke(defense, "StartWave", 1, 2);

            var enemyDefinition = Activator.CreateInstance(
                RequireType(
                    "Necrom.FirstPlayable.Runtime.EnemyArchetypeDefinition"),
                "enemy.guard",
                "frontline.guard",
                4);

            var enemy1 = Invoke(
                enemies,
                "SpawnEnemy",
                "enemy:hybrid:1",
                enemyDefinition,
                enemyZone);
            var enemy2 = Invoke(
                enemies,
                "SpawnEnemy",
                "enemy:hybrid:2",
                enemyDefinition,
                enemyZone);
            Invoke(defense, "RegisterSpawnedThreat", enemy1);
            Invoke(defense, "RegisterSpawnedThreat", enemy2);

            Invoke(
                app,
                "Execute",
                Activator.CreateInstance(
                    RequireType(
                        "Necrom.Core.Application.StartBattleCommand"),
                    "start:hybrid",
                    ReadLong(battle, "Revision")));

            var alliedLoop = root.AddComponent(
                RequireType(
                    "Necrom.FirstPlayable.Runtime.FirstPlayableAlliedAutoCombatLoop"));
            var alliedInit =
                alliedLoop.GetType().GetMethod("Initialize");
            alliedInit.Invoke(
                alliedLoop,
                new object[]
                {
                    app,
                    roster,
                    targeting,
                    pipeline,
                    BuildEntityIdProvider(
                        alliedInit.GetParameters()[4].ParameterType),
                    (Func<string>)(() => NextId("legacy:resolve"))
                });
            Invoke(alliedLoop, "ConfigureDefenseWave", defense);
            Invoke(alliedLoop, "ConfigureHudSession", session);

            return new Fixture
            {
                Root = root,
                Enemies = enemies,
                Formation = formation,
                Battle = battle,
                App = app,
                Roster = roster,
                RaiseAction = raiseAction,
                Session = session,
                Targeting = targeting,
                Pipeline = pipeline,
                Defense = defense,
                Enemy1 = enemy1,
                Enemy2 = enemy2,
                AlliedLoop = alliedLoop
            };
        }

        private static Delegate BuildRaiseCommandFactory(
            Type delegateType,
            object formation)
        {
            var invoke = delegateType.GetMethod("Invoke");
            var source = Expression.Parameter(
                invoke.GetParameters()[0].ParameterType,
                "source");
            var formationConstant =
                Expression.Constant(formation, typeof(object));
            var helper =
                typeof(FirstPlayableHybridDefenseRuntimeTests)
                    .GetMethod(
                        nameof(BuildRaiseCommandObject),
                        BindingFlags.Static | BindingFlags.NonPublic);
            var call = Expression.Call(
                helper,
                Expression.Convert(source, typeof(object)),
                formationConstant);
            return Expression.Lambda(
                delegateType,
                Expression.Convert(call, invoke.ReturnType),
                source).Compile();
        }

        private static object BuildRaiseCommandObject(
            object source,
            object formation)
            => Activator.CreateInstance(
                RequireType(
                    "Necrom.Core.Application.RaiseIntoFormationCommand"),
                NextId("raise"),
                source,
                ReadLong(source, "Revision"),
                NewEntityId(NextId("ally")),
                7,
                0,
                ReadLong(formation, "Revision"));

        private static Delegate BuildBehaviorSpecProvider(
            Type delegateType,
            int damage,
            int intervalMs)
        {
            var invoke = delegateType.GetMethod("Invoke");
            var command = Expression.Parameter(
                invoke.GetParameters()[0].ParameterType,
                "command");
            var helper =
                typeof(FirstPlayableHybridDefenseRuntimeTests)
                    .GetMethod(
                        nameof(NewBehaviorSpec),
                        BindingFlags.Static | BindingFlags.NonPublic);
            var call = Expression.Call(
                helper,
                Expression.Constant(damage),
                Expression.Constant(intervalMs));
            return Expression.Lambda(
                delegateType,
                Expression.Convert(call, invoke.ReturnType),
                command).Compile();
        }

        private static Delegate BuildEntityIdProvider(
            Type delegateType)
        {
            var invoke = delegateType.GetMethod("Invoke");
            var enemyId = Expression.Parameter(
                invoke.GetParameters()[0].ParameterType,
                "enemyId");
            var helper =
                typeof(FirstPlayableHybridDefenseRuntimeTests)
                    .GetMethod(
                        nameof(BuildRaiseSourceId),
                        BindingFlags.Static | BindingFlags.NonPublic);
            var call = Expression.Call(
                helper,
                Expression.Convert(enemyId, typeof(object)));
            return Expression.Lambda(
                delegateType,
                Expression.Convert(call, invoke.ReturnType),
                enemyId).Compile();
        }

        private static object BuildRaiseSourceId(object enemyId)
            => NewEntityId("source:ally:" + enemyId);

        private static object NewBehaviorSpec(
            int damage,
            int intervalMs)
            => Activator.CreateInstance(
                RequireType(
                    "Necrom.Core.Domain.BasicAutoBehaviorSpec"),
                damage,
                intervalMs);

        private static bool TryAcquire(
            Component targeting,
            out object target)
        {
            var args = new object[] { null };
            var result = (bool)targeting
                .GetType()
                .GetMethod("TryAcquireTarget")
                .Invoke(targeting, args);
            target = args[0];
            return result;
        }

        private static object NewCombatant(
            string entityId,
            string archetype,
            string factionName,
            int health)
        {
            var faction = Enum.Parse(
                RequireType("Necrom.Core.Domain.Faction"),
                factionName);
            return Activator.CreateInstance(
                RequireType("Necrom.Core.Domain.Combatant"),
                NewEntityId(entityId),
                archetype,
                faction,
                health);
        }

        private static object NewEntityId(string value)
            => Activator.CreateInstance(
                RequireType("Necrom.Core.Domain.EntityId"),
                value);

        private static RectTransform NewZone(
            Transform parent,
            string name)
        {
            var zone = new GameObject(
                name,
                typeof(RectTransform))
                .GetComponent<RectTransform>();
            zone.SetParent(parent, false);
            return zone;
        }

        private static object Invoke(
            object target,
            string method,
            params object[] args)
        {
            var candidates = target.GetType().GetMethods()
                .Where(m => m.Name == method)
                .Where(m => m.GetParameters().Length == args.Length)
                .Where(
                    m => ParametersAccept(
                        m.GetParameters(),
                        args))
                .ToArray();

            Assert.That(
                candidates.Length,
                Is.EqualTo(1),
                $"Expected one matching {target.GetType().Name}.{method} overload.");
            return candidates[0].Invoke(target, args);
        }

        private static bool ParametersAccept(
            ParameterInfo[] parameters,
            object[] args)
        {
            for (var i = 0; i < parameters.Length; i++)
            {
                if (args[i] == null)
                {
                    if (parameters[i].ParameterType.IsValueType)
                        return false;
                    continue;
                }

                if (!parameters[i].ParameterType.IsInstanceOfType(args[i]))
                    return false;
            }

            return true;
        }

        private static object Read(
            object target,
            string property)
            => target.GetType()
                .GetProperty(
                    property,
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic)
                .GetValue(target);

        private static int ReadInt(
            object target,
            string property)
            => Convert.ToInt32(Read(target, property));

        private static long ReadLong(
            object target,
            string property)
            => Convert.ToInt64(Read(target, property));

        private static Type RequireType(string fullName)
        {
            var type = AppDomain.CurrentDomain
                .GetAssemblies()
                .Select(a => a.GetType(fullName))
                .FirstOrDefault(t => t != null);
            Assert.That(
                type,
                Is.Not.Null,
                fullName + " must exist.");
            return type;
        }

        private static string NextId(string prefix)
            => prefix + ":" + (++_id);

        private sealed class Fixture
        {
            public GameObject Root;
            public Component Enemies;
            public object Formation;
            public object Battle;
            public object App;
            public Component Roster;
            public Component RaiseAction;
            public object Session;
            public Component Targeting;
            public object Pipeline;
            public Component Defense;
            public object Enemy1;
            public object Enemy2;
            public Component AlliedLoop;
        }
    }
}
