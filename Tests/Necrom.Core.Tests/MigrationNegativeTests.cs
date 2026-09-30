using System;
using Necrom.Core.Domain;
using Necrom.Core.Persistence;
using NUnit.Framework;

namespace Necrom.Core.Tests
{
    public sealed class MigrationNegativeTests
    {
        [Test]
        public void MissingMigrationPathIsRejected()
        {
            var registry = new SnapshotMigrationRegistry();
            var source = Snapshot("1");

            Assert.Throws<InvalidOperationException>(() => registry.MigrateTo(source, "2"));
        }

        [Test]
        public void DuplicateMigrationSourceRegistrationIsRejected()
        {
            var registry = new SnapshotMigrationRegistry();
            registry.Register(new FixedMigration("1", "2", s => Snapshot("2")));

            Assert.Throws<InvalidOperationException>(() =>
                registry.Register(new FixedMigration("1", "3", s => Snapshot("3"))));
        }

        [Test]
        public void MigrationReturningUnexpectedVersionIsRejected()
        {
            var registry = new SnapshotMigrationRegistry();
            registry.Register(new FixedMigration("1", "2", s => Snapshot("3")));

            Assert.Throws<InvalidOperationException>(() => registry.MigrateTo(Snapshot("1"), "2"));
        }

        [Test]
        public void MigrationProducingInvalidSnapshotIsRejectedBeforeExposure()
        {
            var registry = new SnapshotMigrationRegistry();
            registry.Register(new FixedMigration("1", "2", s =>
                new GameStateSnapshot("2", BattlePhase.Ready, -1, 0,
                    Array.Empty<FormationSlotSnapshot>())));

            Assert.Throws<InvalidOperationException>(() => registry.MigrateTo(Snapshot("1"), "2"));
        }

        [Test]
        public void MigrationCycleIsRejected()
        {
            var registry = new SnapshotMigrationRegistry();
            registry.Register(new FixedMigration("1", "2", s => Snapshot("2")));
            registry.Register(new FixedMigration("2", "1", s => Snapshot("1")));

            Assert.Throws<InvalidOperationException>(() => registry.MigrateTo(Snapshot("1"), "3"));
        }

        private static GameStateSnapshot Snapshot(string version)
            => new GameStateSnapshot(
                version,
                BattlePhase.Ready,
                0,
                0,
                Array.Empty<FormationSlotSnapshot>());

        private sealed class FixedMigration : IGameStateSnapshotMigration
        {
            private readonly Func<GameStateSnapshot, GameStateSnapshot> _migrate;
            public string FromVersion { get; }
            public string ToVersion { get; }

            public FixedMigration(
                string fromVersion,
                string toVersion,
                Func<GameStateSnapshot, GameStateSnapshot> migrate)
            {
                FromVersion = fromVersion;
                ToVersion = toVersion;
                _migrate = migrate;
            }

            public GameStateSnapshot Migrate(GameStateSnapshot source)
                => _migrate(source);
        }
    }
}
