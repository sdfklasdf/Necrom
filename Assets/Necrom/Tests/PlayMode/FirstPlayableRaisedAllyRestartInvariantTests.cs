using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Necrom.FirstPlayable.Tests
{
    public sealed class FirstPlayableRaisedAllyRestartInvariantTests
    {
        [UnityTest]
        public IEnumerator ApprovedRestartInvariantSeamExists()
        {
            var rosterType = RequireType("Necrom.FirstPlayable.Runtime.FirstPlayableAlliedRosterController");
            var runtimeType = RequireType("Necrom.FirstPlayable.Runtime.FirstPlayableBattleRuntimeController");

            Assert.That(
                rosterType.GetMethod("EnsureRestartInvariant", BindingFlags.Instance | BindingFlags.Public),
                Is.Not.Null);
            Assert.That(
                runtimeType.GetMethod("ConfigureAlliedRestartInvariant", BindingFlags.Instance | BindingFlags.Public),
                Is.Not.Null);
            yield return null;
        }

        [UnityTest]
        public IEnumerator ResolvedRestartPreservesExactFormationOwnershipAndSingleRuntimeAlly()
        {
            var f = NewFixture("preserve", activateMatchingAlly: true);
            MoveBattleToResolved(f);
            var allyBefore = GetRosterSlot(f.Roster, 0);
            var formationRevisionBefore = ReadLong(f.Formation, "Revision");

            Restart(f, "restart-preserve", ReadLong(f.Battle, "Revision"));

            AssertBattle(f.Battle, "Ready", 4L);
            Assert.That(ReadLong(f.Formation, "Revision"), Is.EqualTo(formationRevisionBefore));
            Assert.That(GetFormationSlot(f.Formation, 0).ToString(), Is.EqualTo("undead-preserve"));
            Assert.That(ReadInt(f.Roster, "ActiveCount"), Is.EqualTo(1));
            Assert.That(GetRosterSlot(f.Roster, 0), Is.SameAs(allyBefore));
            Assert.That(f.AlliedZone.childCount, Is.EqualTo(1));

            UnityEngine.Object.Destroy(f.Root);
            yield return null;
        }

        [UnityTest]
        public IEnumerator DefeatedAllyOwnershipSurvivesRestartWithoutHealthReset()
        {
            var f = NewFixture("defeated", activateMatchingAlly: true);
            var ally = GetRosterSlot(f.Roster, 0);
            var model = Read(ally, "Model");
            Invoke(model, "ApplyDamage", ReadInt(model, "Health"));
            Assert.That(Read(model, "LifeState").ToString(), Is.EqualTo("Defeated"));

            MoveBattleToResolved(f);
            var formationRevisionBefore = ReadLong(f.Formation, "Revision");
            Restart(f, "restart-defeated", ReadLong(f.Battle, "Revision"));

            Assert.That(Read(model, "LifeState").ToString(), Is.EqualTo("Defeated"));
            Assert.That(GetFormationSlot(f.Formation, 0).ToString(), Is.EqualTo("undead-defeated"));
            Assert.That(ReadLong(f.Formation, "Revision"), Is.EqualTo(formationRevisionBefore));
            Assert.That(GetRosterSlot(f.Roster, 0), Is.SameAs(ally));
            Assert.That(f.AlliedZone.childCount, Is.EqualTo(1));

            UnityEngine.Object.Destroy(f.Root);
            yield return null;
        }

        [UnityTest]
        public IEnumerator StaleRestartDoesNotMutateOwnershipOrDuplicateRuntimeAlly()
        {
            var f = NewFixture("stale", activateMatchingAlly: true);
            MoveBattleToResolved(f);
            var allyBefore = GetRosterSlot(f.Roster, 0);
            var formationRevisionBefore = ReadLong(f.Formation, "Revision");
            var childCountBefore = f.AlliedZone.childCount;

            var error = Assert.Throws<TargetInvocationException>(
                () => Restart(f, "restart-stale", ReadLong(f.Battle, "Revision") - 1));

            Assert.That(error.InnerException, Is.TypeOf<InvalidOperationException>());
            AssertBattle(f.Battle, "Resolved", 3L);
            Assert.That(ReadLong(f.Formation, "Revision"), Is.EqualTo(formationRevisionBefore));
            Assert.That(GetFormationSlot(f.Formation, 0).ToString(), Is.EqualTo("undead-stale"));
            Assert.That(GetRosterSlot(f.Roster, 0), Is.SameAs(allyBefore));
            Assert.That(ReadInt(f.Roster, "ActiveCount"), Is.EqualTo(1));
            Assert.That(f.AlliedZone.childCount, Is.EqualTo(childCountBefore));

            UnityEngine.Object.Destroy(f.Root);
            yield return null;
        }

        [UnityTest]
        public IEnumerator MissingRuntimeEntryRejectsBeforeRestartMutation()
        {
            var f = NewFixture("missing", activateMatchingAlly: false);
            MoveBattleToResolved(f);
            var battleRevisionBefore = ReadLong(f.Battle, "Revision");
            var formationRevisionBefore = ReadLong(f.Formation, "Revision");

            var error = Assert.Throws<TargetInvocationException>(
                () => Restart(f, "restart-missing", battleRevisionBefore));

            Assert.That(error.InnerException, Is.TypeOf<InvalidOperationException>());
            AssertBattle(f.Battle, "Resolved", battleRevisionBefore);
            Assert.That(ReadLong(f.Formation, "Revision"), Is.EqualTo(formationRevisionBefore));
            Assert.That(GetFormationSlot(f.Formation, 0).ToString(), Is.EqualTo("undead-missing"));
            Assert.That(ReadInt(f.Roster, "ActiveCount"), Is.EqualTo(0));
            Assert.That(f.AlliedZone.childCount, Is.EqualTo(0));

            UnityEngine.Object.Destroy(f.Root);
            yield return null;
        }

        [UnityTest]
        public IEnumerator MismatchedExistingRosterEntryRejectsBeforeRestartMutation()
        {
            var f = NewFixture("mismatch", activateMatchingAlly: false);
            InjectMismatchedRuntimeAlly(f, "wrong-undead-mismatch");
            MoveBattleToResolved(f);
            var battleRevisionBefore = ReadLong(f.Battle, "Revision");
            var formationRevisionBefore = ReadLong(f.Formation, "Revision");

            var error = Assert.Throws<TargetInvocationException>(
                () => Restart(f, "restart-mismatch", battleRevisionBefore));

            Assert.That(error.InnerException, Is.TypeOf<InvalidOperationException>());
            AssertBattle(f.Battle, "Resolved", battleRevisionBefore);
            Assert.That(ReadLong(f.Formation, "Revision"), Is.EqualTo(formationRevisionBefore));
            Assert.That(GetFormationSlot(f.Formation, 0).ToString(), Is.EqualTo("undead-mismatch"));
            Assert.That(ReadInt(f.Roster, "ActiveCount"), Is.EqualTo(1));
            Assert.That(Read(Read(GetRosterSlot(f.Roster, 0), "Model"), "Id").ToString(),
                Is.EqualTo("wrong-undead-mismatch"));
            Assert.That(f.AlliedZone.childCount, Is.EqualTo(1));

            UnityEngine.Object.Destroy(f.Root);
            yield return null;
        }

        private static Fixture NewFixture(string suffix, bool activateMatchingAlly)
        {
            var root = new GameObject("RestartInvariant-" + suffix, typeof(RectTransform));
            var boundary = root.AddComponent(RequireType("Necrom.FirstPlayable.Runtime.EncounterBoundaryController"));
            var config = Activator.CreateInstance(
                RequireType("Necrom.FirstPlayable.Runtime.EncounterLayoutConfig"),
                new object[]
                {
                    new Vector2(390f, 844f),
                    new Rect(0f, 34f, 390f, 776f),
                    0.22f,
                    new Rect(0.12f, 0.08f, 0.30f, 0.24f),
                    new Rect(0.58f, 0.58f, 0.30f, 0.28f),
                    new Rect(0.12f, 0.34f, 0.44f, 0.18f)
                });
            Invoke(boundary, "StartBoundary", config);

            var safeArea = root.transform.Find("SafeArea");
            var combat = safeArea.Find("CombatViewport");
            var necromancerZone = combat.Find("NecromancerSpawnZone") as RectTransform;
            var enemyZone = combat.Find("EnemySpawnZone") as RectTransform;
            var alliedZone = new GameObject("AlliedSpawnZone", typeof(RectTransform)).GetComponent<RectTransform>();
            alliedZone.SetParent(combat, false);

            var necromancer = combat.gameObject.AddComponent(
                RequireType("Necrom.FirstPlayable.Runtime.NecromancerAnchorController"));
            Invoke(
                necromancer,
                "BindNecromancer",
                NewCombatant("player-" + suffix, "necromancer.prototype", "Player", 100),
                necromancerZone);

            var enemies = combat.gameObject.AddComponent(
                RequireType("Necrom.FirstPlayable.Runtime.EnemySpawnController"));
            var enemyDefinition = Activator.CreateInstance(
                RequireType("Necrom.FirstPlayable.Runtime.EnemyArchetypeDefinition"),
                "enemy.skeleton.guard",
                "frontline.guard",
                25);
            Invoke(enemies, "SpawnEnemy", "enemy-" + suffix, enemyDefinition, enemyZone);

            var formation = Activator.CreateInstance(RequireType("Necrom.Core.Domain.Formation"));
            var undeadId = NewEntityId("undead-" + suffix);
            Invoke(formation, "Assign", 0, undeadId, (long)0);

            var battle = Activator.CreateInstance(RequireType("Necrom.Core.Domain.BattleStateMachine"));
            var encounter = Activator.CreateInstance(
                RequireType("Necrom.Core.Domain.FirstPlayableEncounter"),
                Activator.CreateInstance(RequireType("Necrom.Core.Domain.RaiseService")),
                battle,
                formation);
            var app = Activator.CreateInstance(
                RequireType("Necrom.Core.Application.FirstPlayableApplicationService"),
                encounter);

            var roster = combat.gameObject.AddComponent(
                RequireType("Necrom.FirstPlayable.Runtime.FirstPlayableAlliedRosterController"));
            Invoke(roster, "Initialize", formation, alliedZone);

            if (activateMatchingAlly)
            {
                var source = NewConsumedSource(suffix);
                var command = NewRaiseCommand(source, undeadId, suffix);
                Invoke(roster, "ActivateCommitted", command, NewBehaviorSpec(4, 100));
            }

            var runtime = combat.gameObject.AddComponent(
                RequireType("Necrom.FirstPlayable.Runtime.FirstPlayableBattleRuntimeController"));
            Invoke(runtime, "Initialize", app, boundary, config, necromancer, enemies);
            ConfigureRestartInvariant(runtime, roster);

            return new Fixture
            {
                Root = root,
                AlliedZone = alliedZone,
                Formation = formation,
                Battle = battle,
                App = app,
                Roster = roster,
                Runtime = runtime
            };
        }

        private static void ConfigureRestartInvariant(object runtime, object roster)
        {
            var method = runtime.GetType().GetMethod(
                "ConfigureAlliedRestartInvariant",
                BindingFlags.Instance | BindingFlags.Public);
            Assert.That(method, Is.Not.Null,
                "FirstPlayableBattleRuntimeController.ConfigureAlliedRestartInvariant must exist.");
            method.Invoke(runtime, new[] { roster });
        }

        private static void MoveBattleToResolved(Fixture f)
        {
            Invoke(
                f.App,
                "Execute",
                NewCommand("Necrom.Core.Application.StartBattleCommand", "start-" + f.Root.name, 0L));
            Invoke(
                f.App,
                "Execute",
                NewResolveCommand("resolve-" + f.Root.name, false, 1L));
            Invoke(
                f.App,
                "Execute",
                NewCommand("Necrom.Core.Application.FinalizeBattleCommand", "finalize-" + f.Root.name, 2L));
            AssertBattle(f.Battle, "Resolved", 3L);
        }

        private static object Restart(Fixture f, string commandId, long expectedRevision)
            => Invoke(
                f.Runtime,
                "RestartBattle",
                NewCommand(
                    "Necrom.Core.Application.RestartBattleCommand",
                    commandId,
                    expectedRevision));

        private static void InjectMismatchedRuntimeAlly(Fixture f, string unitId)
        {
            var allyObject = new GameObject(
                "Ally:" + unitId,
                typeof(RectTransform),
                RequireType("Necrom.FirstPlayable.Runtime.RaisedAllyRuntimeEntity"));
            allyObject.transform.SetParent(f.AlliedZone, false);
            var ally = allyObject.GetComponent(
                RequireType("Necrom.FirstPlayable.Runtime.RaisedAllyRuntimeEntity"));
            Invoke(
                ally,
                "Initialize",
                NewCombatant(unitId, "enemy.skeleton.guard", "Player", 7),
                0,
                NewBehaviorSpec(4, 100));

            var field = f.Roster.GetType().GetField(
                "_slots",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null);
            var slots = (Array)field.GetValue(f.Roster);
            slots.SetValue(ally, 0);
        }

        private static object NewConsumedSource(string suffix)
        {
            var defeated = NewCombatant(
                "enemy-source-" + suffix,
                "enemy.skeleton.guard",
                "Enemy",
                5);
            Invoke(defeated, "ApplyDamage", 5);
            var source = Activator.CreateInstance(
                RequireType("Necrom.Core.Domain.RaiseSource"),
                NewEntityId("source-" + suffix),
                defeated,
                "frontline.guard");
            Invoke(source, "Consume", (long)0);
            return source;
        }

        private static object NewRaiseCommand(object source, object undeadId, string suffix)
            => Activator.CreateInstance(
                RequireType("Necrom.Core.Application.RaiseIntoFormationCommand"),
                "raise-restart-" + suffix,
                source,
                ReadLong(source, "Revision"),
                undeadId,
                7,
                0,
                (long)1);

        private static object NewBehaviorSpec(int damage, int intervalMs)
            => Activator.CreateInstance(
                RequireType("Necrom.Core.Domain.BasicAutoBehaviorSpec"),
                damage,
                intervalMs);

        private static object NewCommand(string fullName, string commandId, long revision)
            => Activator.CreateInstance(
                RequireType(fullName),
                commandId,
                revision);

        private static object NewResolveCommand(string commandId, bool playerWon, long revision)
            => Activator.CreateInstance(
                RequireType("Necrom.Core.Application.ResolveBattleCommand"),
                commandId,
                playerWon,
                revision);

        private static object NewCombatant(
            string entityId,
            string archetypeId,
            string factionName,
            int health)
        {
            var faction = Enum.Parse(
                RequireType("Necrom.Core.Domain.Faction"),
                factionName);
            return Activator.CreateInstance(
                RequireType("Necrom.Core.Domain.Combatant"),
                NewEntityId(entityId),
                archetypeId,
                faction,
                health);
        }

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
            Assert.That(
                candidates.Length,
                Is.EqualTo(1),
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
            => target.GetType().GetProperty(
                property,
                BindingFlags.Instance | BindingFlags.Public).GetValue(target);

        private static int ReadInt(object target, string property)
            => Convert.ToInt32(Read(target, property));

        private static long ReadLong(object target, string property)
            => Convert.ToInt64(Read(target, property));

        private static void AssertBattle(object battle, string phase, long revision)
        {
            Assert.That(Read(battle, "Phase").ToString(), Is.EqualTo(phase));
            Assert.That(ReadLong(battle, "Revision"), Is.EqualTo(revision));
        }

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

        private sealed class Fixture
        {
            public GameObject Root;
            public RectTransform AlliedZone;
            public object Formation;
            public object Battle;
            public object App;
            public Component Roster;
            public Component Runtime;
        }
    }
}