using System;
using System.Collections.Generic;
using Necrom.Core.Domain;
using Necrom.Core.Persistence;
using NUnit.Framework;

namespace Necrom.Core.Tests
{
    public sealed class SnapshotContractTests
    {
        [Test]
        public void ValidationRejectsDuplicateUnitAcrossSlots()
        {
            var snapshot = new GameStateSnapshot("1", BattlePhase.Ready, 0, 0,
                new[]
                {
                    new FormationSlotSnapshot(0, "unit-a"),
                    new FormationSlotSnapshot(1, "unit-a")
                });

            Assert.Throws<InvalidOperationException>(() => GameStateSnapshotValidator.Validate(snapshot));
        }

        [Test]
        public void CaptureThenHydratePreservesBattleAndFormationRevisions()
        {
            var battle = new BattleStateMachine();
            battle.Start(0);
            var formation = new Formation();
            formation.Assign(2, new EntityId("unit-a"), 0);

            var snapshot = GameStateSnapshot.Capture("1", battle, formation);
            var restored = GameStateHydrator.Restore(snapshot);

            Assert.That(restored.Battle.Phase, Is.EqualTo(BattlePhase.Running));
            Assert.That(restored.Battle.Revision, Is.EqualTo(1));
            Assert.That(restored.Formation.Revision, Is.EqualTo(1));
            Assert.That(restored.Formation.GetSlot(2).Value.Value, Is.EqualTo("unit-a"));
        }

        [Test]
        public void MigrationRegistryAppliesExplicitPathAndPreservesState()
        {
            var source = new GameStateSnapshot("1", BattlePhase.Running, 3, 2,
                new[] { new FormationSlotSnapshot(0, "unit-a") });
            var registry = new SnapshotMigrationRegistry();
            registry.Register(new VersionOnlyMigration("1", "2"));

            var migrated = registry.MigrateTo(source, "2");

            Assert.That(migrated.SchemaVersion, Is.EqualTo("2"));
            Assert.That(migrated.BattlePhase, Is.EqualTo(BattlePhase.Running));
            Assert.That(migrated.BattleRevision, Is.EqualTo(3));
            Assert.That(migrated.FormationRevision, Is.EqualTo(2));
            Assert.That(migrated.Formation[0].UnitId, Is.EqualTo("unit-a"));
        }

        private sealed class VersionOnlyMigration : IGameStateSnapshotMigration
        {
            public string FromVersion { get; }
            public string ToVersion { get; }

            public VersionOnlyMigration(string fromVersion, string toVersion)
            {
                FromVersion = fromVersion;
                ToVersion = toVersion;
            }

            public GameStateSnapshot Migrate(GameStateSnapshot source)
                => new GameStateSnapshot(ToVersion, source.BattlePhase, source.BattleRevision,
                    source.FormationRevision, source.Formation);
        }
    }
}
