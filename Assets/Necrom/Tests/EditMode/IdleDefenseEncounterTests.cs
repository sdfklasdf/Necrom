using Necrom.Core.Domain;
using NUnit.Framework;

namespace Necrom.Core.UnityTests
{
    public sealed class IdleDefenseEncounterTests
    {
        [Test]
        public void WaveClearsOnlyAfterQueuedAndActiveEnemiesAreResolved()
        {
            var encounter = new IdleDefenseEncounter(100);

            encounter.StartWave(1, 2, 0);
            Assert.That(encounter.Phase, Is.EqualTo(DefenseWavePhase.Running));
            Assert.That(encounter.RemainingEnemiesToSpawn, Is.EqualTo(2));

            encounter.EnemySpawned(1);
            encounter.EnemySpawned(2);
            encounter.EnemyDefeated(3);

            Assert.That(encounter.Phase, Is.EqualTo(DefenseWavePhase.Running));
            Assert.That(encounter.ActiveEnemyCount, Is.EqualTo(1));

            encounter.EnemyDefeated(4);

            Assert.That(encounter.Phase, Is.EqualTo(DefenseWavePhase.Cleared));
            Assert.That(encounter.ActiveEnemyCount, Is.Zero);
            Assert.That(encounter.RemainingEnemiesToSpawn, Is.Zero);
            Assert.That(encounter.GateIntegrity, Is.EqualTo(100));
        }

        [Test]
        public void EnemyReachingGateConsumesIntegrityAndCanFailWave()
        {
            var encounter = new IdleDefenseEncounter(10);

            encounter.StartWave(1, 2, 0);
            encounter.EnemySpawned(1);
            encounter.EnemySpawned(2);
            encounter.EnemyReachedGate(4, 3);

            Assert.That(encounter.GateIntegrity, Is.EqualTo(6));
            Assert.That(encounter.Phase, Is.EqualTo(DefenseWavePhase.Running));

            encounter.EnemyReachedGate(8, 4);

            Assert.That(encounter.GateIntegrity, Is.Zero);
            Assert.That(encounter.Phase, Is.EqualTo(DefenseWavePhase.Failed));
        }

        [Test]
        public void NextWavePreservesGateIntegrityAndRequiresSequentialWaveNumber()
        {
            var encounter = new IdleDefenseEncounter(20);

            encounter.StartWave(1, 1, 0);
            encounter.EnemySpawned(1);
            encounter.EnemyReachedGate(3, 2);

            Assert.That(encounter.Phase, Is.EqualTo(DefenseWavePhase.Cleared));
            Assert.That(encounter.GateIntegrity, Is.EqualTo(17));

            encounter.PrepareNextWave(3);
            Assert.That(encounter.Phase, Is.EqualTo(DefenseWavePhase.Ready));
            Assert.That(encounter.GateIntegrity, Is.EqualTo(17));

            Assert.Throws<System.InvalidOperationException>(
                () => encounter.StartWave(3, 1, 4));

            encounter.StartWave(2, 1, 4);
            Assert.That(encounter.WaveNumber, Is.EqualTo(2));
        }

        [Test]
        public void InvalidRevisionOrTransitionDoesNotMutateState()
        {
            var encounter = new IdleDefenseEncounter(12);

            Assert.Throws<System.InvalidOperationException>(
                () => encounter.EnemySpawned(0));
            Assert.That(encounter.Phase, Is.EqualTo(DefenseWavePhase.Ready));
            Assert.That(encounter.Revision, Is.Zero);

            encounter.StartWave(1, 1, 0);

            Assert.Throws<System.InvalidOperationException>(
                () => encounter.EnemySpawned(0));
            Assert.That(encounter.RemainingEnemiesToSpawn, Is.EqualTo(1));
            Assert.That(encounter.ActiveEnemyCount, Is.Zero);
            Assert.That(encounter.Revision, Is.EqualTo(1));
        }
    }
}
