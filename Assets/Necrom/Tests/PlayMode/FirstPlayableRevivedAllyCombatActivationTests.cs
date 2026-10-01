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
    public sealed class FirstPlayableRevivedAllyCombatActivationTests
    {
        private static string _suffix;
        private static object _lastCommand;
        private static int _damageEventCalls;
        private static int _defeatEventCalls;
        private static int _resolveCommandCalls;

        [UnityTest]
        public IEnumerator ApprovedActivationTypesExist()
        {
            Assert.That(FindType("Necrom.FirstPlayable.Runtime.RaisedAllyRuntimeEntity"), Is.Not.Null);
            Assert.That(FindType("Necrom.FirstPlayable.Runtime.FirstPlayableAlliedRosterController"), Is.Not.Null);
            Assert.That(FindType("Necrom.FirstPlayable.Runtime.FirstPlayableAlliedAutoCombatLoop"), Is.Not.Null);
            yield return null;
        }

        [UnityTest]
        public IEnumerator SuccessfulRaiseActivatesExactFormationUnitOnce()
        {
            var f = NewRaiseFixture("activate");
            var result = Execute(f.Action);

            Assert.That(Read(result, "CommandId").ToString(), Is.EqualTo("raise-ally-activate"));
            Assert.That(Read(f.Source, "State").ToString(), Is.EqualTo("Consumed"));
            Assert.That(GetFormationSlot(f.Formation, 0).ToString(), Is.EqualTo("undead-activate"));
            Assert.That(ReadInt(f.Roster, "ActiveCount"), Is.EqualTo(1));

            var ally = GetRosterSlot(f.Roster, 0);
            Assert.That(ally, Is.Not.Null);
            Assert.That(Read(Read(ally, "Model"), "Id").ToString(), Is.EqualTo("undead-activate"));
            Assert.That(((Component)ally).transform.parent, Is.EqualTo(f.AlliedZone));

            var spec = NewBehaviorSpec(4, 100);
            var error = Assert.Throws<TargetInvocationException>(
                () => Invoke(f.Roster, "EnsureCanActivate", _lastCommand, spec));
            Assert.That(error.InnerException, Is.TypeOf<InvalidOperationException>());
            Assert.That(ReadInt(f.Roster, "ActiveCount"), Is.EqualTo(1));
            Assert.That(ReadLong(f.Formation, "Revision"), Is.EqualTo(1));

            UnityEngine.Object.Destroy(f.Root);
            yield return null;
        }

        [UnityTest]
        public IEnumerator MismatchedOccupiedFormationRejectsBeforeRuntimeMutation()
        {
            Configure("mismatch");
            var root = new GameObject("AlliedMismatch", typeof(RectTransform));
            var alliedZone = NewZone(root.transform, "AlliedSpawnZone");
            var formation = Activator.CreateInstance(RequireType("Necrom.Core.Domain.Formation"));
            Invoke(formation, "Assign", 0, NewEntityId("other-unit"), (long)0);

            var roster = root.AddComponent(RequireType("Necrom.FirstPlayable.Runtime.FirstPlayableAlliedRosterController"));
            Invoke(roster, "Initialize", formation, alliedZone);
            var source = NewConsumedSource("mismatch");
            var command = NewRaiseCommand(source, "undead-mismatch", 0, ReadLong(formation, "Revision"));
            var spec = NewBehaviorSpec(4, 100);

            var error = Assert.Throws<TargetInvocationException>(
                () => Invoke(roster, "EnsureCanActivate", command, spec));
            Assert.That(error.InnerException, Is.TypeOf<InvalidOperationException>());
            Assert.That(ReadInt(roster, "ActiveCount"), Is.EqualTo(0));
            Assert.That(alliedZone.childCount, Is.EqualTo(0));

            UnityEngine.Object.Destroy(root);
            yield return null;
        }

        [UnityTest]
        public IEnumerator ProjectionRequiresCommittedFormationState()
        {
            Configure("uncommitted");
            var root = new GameObject("AlliedUncommitted", typeof(RectTransform));
            var alliedZone = NewZone(root.transform, "AlliedSpawnZone");
            var formation = Activator.CreateInstance(RequireType("Necrom.Core.Domain.Formation"));
            var roster = root.AddComponent(RequireType("Necrom.FirstPlayable.Runtime.FirstPlayableAlliedRosterController"));
            Invoke(roster, "Initialize", formation, alliedZone);
            var source = NewConsumedSource("uncommitted");
            var command = NewRaiseCommand(source, "undead-uncommitted", 0, 0);
            var spec = NewBehaviorSpec(4, 100);

            var error = Assert.Throws<TargetInvocationException>(
                () => Invoke(roster, "ActivateCommitted", command, spec));
            Assert.That(error.InnerException, Is.TypeOf<InvalidOperationException>());
            Assert.That(ReadInt(roster, "ActiveCount"), Is.EqualTo(0));
            Assert.That(alliedZone.childCount, Is.EqualTo(0));

            UnityEngine.Object.Destroy(root);
            yield return null;
        }

        [UnityTest]
        public IEnumerator ActiveRaisedAllyDamagesNextEnemyUsingInjectedSpec()
        {
            var f = NewRaiseFixture("combat");
            Execute(f.Action);
            var next = StartNextCombat(f, enemyHealth: 10, allyDamage: 4, intervalMs: 100);

            Invoke(next.Loop, "Advance", 0.100f);

            Assert.That(ReadInt(next.EnemyModel, "Health"), Is.EqualTo(6));
            Assert.That(_damageEventCalls, Is.EqualTo(1));
            Assert.That(_defeatEventCalls, Is.EqualTo(0));
            Assert.That(GetFormationSlot(f.Formation, 0).ToString(), Is.EqualTo("undead-combat"));
            Assert.That(ReadLong(f.Formation, "Revision"), Is.EqualTo(1));

            UnityEngine.Object.Destroy(f.Root);
            yield return null;
        }

        [UnityTest]
        public IEnumerator DefeatedRaisedAllyCannotContribute()
        {
            var f = NewRaiseFixture("defeated");
            Execute(f.Action);
            var ally = GetRosterSlot(f.Roster, 0);
            var allyModel = Read(ally, "Model");
            Invoke(allyModel, "ApplyDamage", 7);
            var next = StartNextCombat(f, enemyHealth: 10, allyDamage: 4, intervalMs: 100);

            Invoke(next.Loop, "Advance", 0.500f);

            Assert.That(Read(allyModel, "LifeState").ToString(), Is.EqualTo("Defeated"));
            Assert.That(ReadInt(next.EnemyModel, "Health"), Is.EqualTo(10));
            Assert.That(_damageEventCalls, Is.EqualTo(0));

            UnityEngine.Object.Destroy(f.Root);
            yield return null;
        }

        private static RaiseFixture NewRaiseFixture(string suffix)
        {
            Configure(suffix);
            var root = new GameObject("RevivedAlly-" + suffix, typeof(RectTransform));
            var enemyZone = NewZone(root.transform, "EnemySpawnZone");
            var alliedZone = NewZone(root.transform, "AlliedSpawnZone");
            var spawn = root.AddComponent(RequireType("Necrom.FirstPlayable.Runtime.EnemySpawnController"));
            var formation = Activator.CreateInstance(RequireType("Necrom.Core.Domain.Formation"));
            var battle = Activator.CreateInstance(RequireType("Necrom.Core.Domain.BattleStateMachine"));
            var encounter = Activator.CreateInstance(
                RequireType("Necrom.Core.Domain.FirstPlayableEncounter"),
                Activator.CreateInstance(RequireType("Necrom.Core.Domain.RaiseService")),
                battle,
                formation);
            var app = Activator.CreateInstance(
                RequireType("Necrom.Core.Application.FirstPlayableApplicationService"),
                encounter);
            var progression = Activator.CreateInstance(
                RequireType("Necrom.Core.Application.FirstPlayableProgression"),
                app,
                encounter);
            var hook = root.AddComponent(RequireType("Necrom.FirstPlayable.Runtime.FirstPlayableRaiseCommandInputHook"));
            Invoke(hook, "Initialize", progression);

            var definition = Activator.CreateInstance(
                RequireType("Necrom.FirstPlayable.Runtime.EnemyArchetypeDefinition"),
                "enemy.skeleton.guard",
                "frontline.guard",
                5);
            var enemy = Invoke(spawn, "SpawnEnemy", "enemy-" + suffix, definition, enemyZone);
            Invoke(enemy, "ApplyDamage", 5, NewEntityId("source-" + suffix));
            var source = Read(enemy, "RaiseSource");

            var roster = root.AddComponent(RequireType("Necrom.FirstPlayable.Runtime.FirstPlayableAlliedRosterController"));
            Invoke(roster, "Initialize", formation, alliedZone);

            var action = root.AddComponent(RequireType("Necrom.FirstPlayable.Runtime.FirstPlayableRaiseActionController"));
            var init = action.GetType().GetMethod("Initialize");
            var p = init.GetParameters();
            init.Invoke(action, new object[]
            {
                spawn,
                hook,
                BuildCommandFactoryDelegate(p[2].ParameterType),
                (Func<string>)(() => "raised:" + suffix),
                (Func<string>)(() => "assigned:" + suffix)
            });

            var configure = action.GetType().GetMethod("ConfigureAlliedActivation");
            Assert.That(configure, Is.Not.Null);
            var cp = configure.GetParameters();
            configure.Invoke(action, new object[]
            {
                roster,
                BuildBehaviorSpecProvider(cp[1].ParameterType)
            });

            return new RaiseFixture
            {
                Root = root,
                EnemyZone = enemyZone,
                AlliedZone = alliedZone,
                Spawn = spawn,
                Formation = formation,
                Battle = battle,
                App = app,
                Source = source,
                Roster = roster,
                Action = action
            };
        }

        private static CombatFixture StartNextCombat(
            RaiseFixture f,
            int enemyHealth,
            int allyDamage,
            int intervalMs)
        {
            _damageEventCalls = 0;
            _defeatEventCalls = 0;
            _resolveCommandCalls = 0;

            var definition = Activator.CreateInstance(
                RequireType("Necrom.FirstPlayable.Runtime.EnemyArchetypeDefinition"),
                "enemy.next.guard",
                "frontline.guard",
                enemyHealth);
            var enemy = Invoke(f.Spawn, "SpawnEnemy", "enemy-next-" + _suffix, definition, f.EnemyZone);
            var enemyModel = Read(enemy, "Model");

            var necroObject = new GameObject("NecromancerRuntime", typeof(RectTransform));
            necroObject.transform.SetParent(f.Root.transform, false);
            var necro = necroObject.AddComponent(RequireType("Necrom.FirstPlayable.Runtime.NecromancerRuntimeEntity"));
            var necroModel = NewCombatant("player-" + _suffix, "necromancer.prototype", "Player", 100);
            Invoke(necro, "Initialize", necroModel);

            var targeting = f.Root.AddComponent(RequireType("Necrom.FirstPlayable.Runtime.FirstPlayableTargetingController"));
            Invoke(targeting, "Initialize", necro, f.Spawn);

            var pipeline = Activator.CreateInstance(
                RequireType("Necrom.FirstPlayable.Runtime.FirstPlayableDamageDeathPipeline"),
                (Func<string>)ProvideDamageEventId,
                (Func<string>)ProvideDefeatEventId);

            var startCommand = Activator.CreateInstance(
                RequireType("Necrom.Core.Application.StartBattleCommand"),
                "start-next-" + _suffix,
                ReadLong(f.Battle, "Revision"));
            Invoke(f.App, "Execute", startCommand);

            var loop = f.Root.AddComponent(RequireType("Necrom.FirstPlayable.Runtime.FirstPlayableAlliedAutoCombatLoop"));
            var initialize = loop.GetType().GetMethod("Initialize");
            var ip = initialize.GetParameters();
            initialize.Invoke(loop, new object[]
            {
                f.App,
                f.Roster,
                targeting,
                pipeline,
                BuildEntityIdProvider(ip[4].ParameterType),
                (Func<string>)ProvideResolveCommandId
            });

            return new CombatFixture { Loop = loop, EnemyModel = enemyModel };
        }

        private static void Configure(string suffix)
        {
            _suffix = suffix;
            _lastCommand = null;
            _damageEventCalls = 0;
            _defeatEventCalls = 0;
            _resolveCommandCalls = 0;
        }

        private static Delegate BuildCommandFactoryDelegate(Type delegateType)
        {
            var invoke = delegateType.GetMethod("Invoke");
            var sourceType = invoke.GetParameters()[0].ParameterType;
            var source = Expression.Parameter(sourceType, "source");
            var helper = typeof(FirstPlayableRevivedAllyCombatActivationTests)
                .GetMethod(nameof(BuildRaiseCommandObject), BindingFlags.Static | BindingFlags.NonPublic);
            var call = Expression.Call(helper, Expression.Convert(source, typeof(object)));
            return Expression.Lambda(
                delegateType,
                Expression.Convert(call, invoke.ReturnType),
                source).Compile();
        }

        private static object BuildRaiseCommandObject(object source)
        {
            _lastCommand = NewRaiseCommand(
                source,
                "undead-" + _suffix,
                0,
                0);
            return _lastCommand;
        }

        private static Delegate BuildBehaviorSpecProvider(Type delegateType)
        {
            var invoke = delegateType.GetMethod("Invoke");
            var inputType = invoke.GetParameters()[0].ParameterType;
            var input = Expression.Parameter(inputType, "command");
            var helper = typeof(FirstPlayableRevivedAllyCombatActivationTests)
                .GetMethod(nameof(ProvideBehaviorSpecObject), BindingFlags.Static | BindingFlags.NonPublic);
            var call = Expression.Call(helper, Expression.Convert(input, typeof(object)));
            return Expression.Lambda(
                delegateType,
                Expression.Convert(call, invoke.ReturnType),
                input).Compile();
        }

        private static object ProvideBehaviorSpecObject(object command)
            => NewBehaviorSpec(4, 100);

        private static Delegate BuildEntityIdProvider(Type delegateType)
        {
            var invoke = delegateType.GetMethod("Invoke");
            var inputType = invoke.GetParameters()[0].ParameterType;
            var input = Expression.Parameter(inputType, "enemyId");
            var helper = typeof(FirstPlayableRevivedAllyCombatActivationTests)
                .GetMethod(nameof(ProvideRaiseSourceIdObject), BindingFlags.Static | BindingFlags.NonPublic);
            var call = Expression.Call(helper, Expression.Convert(input, typeof(object)));
            return Expression.Lambda(
                delegateType,
                Expression.Convert(call, invoke.ReturnType),
                input).Compile();
        }

        private static object ProvideRaiseSourceIdObject(object enemyId)
            => NewEntityId("raise:ally:" + enemyId);

        private static string ProvideDamageEventId()
        {
            _damageEventCalls++;
            return "damage:ally:" + _damageEventCalls;
        }

        private static string ProvideDefeatEventId()
        {
            _defeatEventCalls++;
            return "defeat:ally:" + _defeatEventCalls;
        }

        private static string ProvideResolveCommandId()
        {
            _resolveCommandCalls++;
            return "resolve:ally:" + _resolveCommandCalls;
        }

        private static object NewConsumedSource(string suffix)
        {
            var defeated = NewCombatant("enemy-source-" + suffix, "enemy.skeleton.guard", "Enemy", 5);
            Invoke(defeated, "ApplyDamage", 5);
            var source = Activator.CreateInstance(
                RequireType("Necrom.Core.Domain.RaiseSource"),
                NewEntityId("source-" + suffix),
                defeated,
                "frontline.guard");
            Invoke(source, "Consume", (long)0);
            return source;
        }

        private static object NewRaiseCommand(
            object source,
            string undeadId,
            int slot,
            long formationRevision)
            => Activator.CreateInstance(
                RequireType("Necrom.Core.Application.RaiseIntoFormationCommand"),
                "raise-ally-" + _suffix,
                source,
                ReadLong(source, "Revision"),
                NewEntityId(undeadId),
                7,
                slot,
                formationRevision);

        private static object NewBehaviorSpec(int damage, int intervalMs)
            => Activator.CreateInstance(
                RequireType("Necrom.Core.Domain.BasicAutoBehaviorSpec"),
                damage,
                intervalMs);

        private static object NewCombatant(
            string entityId,
            string archetype,
            string factionName,
            int health)
        {
            var faction = Enum.Parse(RequireType("Necrom.Core.Domain.Faction"), factionName);
            return Activator.CreateInstance(
                RequireType("Necrom.Core.Domain.Combatant"),
                NewEntityId(entityId),
                archetype,
                faction,
                health);
        }

        private static RectTransform NewZone(Transform parent, string name)
        {
            var zone = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            zone.SetParent(parent, false);
            return zone;
        }

        private static object Execute(object action)
            => Invoke(action, "Execute");

        private static object GetFormationSlot(object formation, int slot)
            => Invoke(formation, "GetSlot", slot);

        private static object GetRosterSlot(object roster, int slot)
            => Invoke(roster, "GetSlot", slot);

        private static object NewEntityId(string value)
            => Activator.CreateInstance(
                RequireType("Necrom.Core.Domain.EntityId"),
                value);

        private static object Invoke(object target, string method, params object[] args)
        {
            var candidates = target.GetType().GetMethods()
                .Where(m => m.Name == method)
                .Where(m => m.GetParameters().Length == args.Length)
                .Where(m => ParametersAccept(m.GetParameters(), args))
                .ToArray();
            Assert.That(candidates.Length, Is.EqualTo(1),
                $"Expected one matching {target.GetType().Name}.{method} overload.");
            return candidates[0].Invoke(target, args);
        }

        private static bool ParametersAccept(ParameterInfo[] parameters, object[] args)
        {
            for (var i = 0; i < parameters.Length; i++)
            {
                if (args[i] == null)
                {
                    if (parameters[i].ParameterType.IsValueType) return false;
                    continue;
                }
                if (!parameters[i].ParameterType.IsInstanceOfType(args[i])) return false;
            }
            return true;
        }

        private static object Read(object target, string property)
            => target.GetType().GetProperty(property).GetValue(target);

        private static int ReadInt(object target, string property)
            => Convert.ToInt32(Read(target, property));

        private static long ReadLong(object target, string property)
            => Convert.ToInt64(Read(target, property));

        private static Type RequireType(string fullName)
        {
            var type = FindType(fullName);
            Assert.That(type, Is.Not.Null, fullName + " must exist.");
            return type;
        }

        private static Type FindType(string fullName)
            => AppDomain.CurrentDomain.GetAssemblies()
                .Select(a => a.GetType(fullName))
                .FirstOrDefault(t => t != null);

        private sealed class RaiseFixture
        {
            public GameObject Root;
            public RectTransform EnemyZone;
            public RectTransform AlliedZone;
            public Component Spawn;
            public object Formation;
            public object Battle;
            public object App;
            public object Source;
            public Component Roster;
            public Component Action;
        }

        private sealed class CombatFixture
        {
            public Component Loop;
            public object EnemyModel;
        }
    }
}