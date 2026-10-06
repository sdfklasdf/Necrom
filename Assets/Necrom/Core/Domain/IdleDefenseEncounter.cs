using System;

namespace Necrom.Core.Domain
{
    public enum DefenseWavePhase
    {
        Ready = 0,
        Running = 1,
        Cleared = 2,
        Failed = 3
    }

    /// <summary>
    /// Balance-neutral domain seam for the idle-RPG + light-defense direction.
    /// It owns only wave/gate truth. Existing combatants, Formation, Raise,
    /// auto-combat and presentation systems remain separate concerns.
    /// </summary>
    public sealed class IdleDefenseEncounter
    {
        public int GateMaxIntegrity { get; }
        public int GateIntegrity { get; private set; }
        public int WaveNumber { get; private set; }
        public int RemainingEnemiesToSpawn { get; private set; }
        public int ActiveEnemyCount { get; private set; }
        public DefenseWavePhase Phase { get; private set; }
        public long Revision { get; private set; }

        public IdleDefenseEncounter(int gateMaxIntegrity)
        {
            if (gateMaxIntegrity <= 0)
                throw new ArgumentOutOfRangeException(nameof(gateMaxIntegrity));

            GateMaxIntegrity = gateMaxIntegrity;
            GateIntegrity = gateMaxIntegrity;
            Phase = DefenseWavePhase.Ready;
        }

        public void StartWave(int waveNumber, int totalEnemyCount, long expectedRevision)
        {
            RequireRevision(expectedRevision);
            if (Phase != DefenseWavePhase.Ready)
                throw new InvalidOperationException("A defense wave can start only from Ready.");
            if (waveNumber <= 0)
                throw new ArgumentOutOfRangeException(nameof(waveNumber));
            if (WaveNumber > 0 && waveNumber != WaveNumber + 1)
                throw new InvalidOperationException("Defense wave numbers must advance sequentially.");
            if (totalEnemyCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(totalEnemyCount));

            WaveNumber = waveNumber;
            RemainingEnemiesToSpawn = totalEnemyCount;
            ActiveEnemyCount = 0;
            Phase = DefenseWavePhase.Running;
            Revision++;
        }

        public void EnemySpawned(long expectedRevision)
        {
            RequireRunning(expectedRevision);
            if (RemainingEnemiesToSpawn <= 0)
                throw new InvalidOperationException("No queued defense enemy remains to spawn.");

            RemainingEnemiesToSpawn--;
            ActiveEnemyCount++;
            Revision++;
        }

        public void EnemyDefeated(long expectedRevision)
        {
            RequireRunning(expectedRevision);
            RequireActiveEnemy();

            ActiveEnemyCount--;
            Revision++;
            EvaluateWaveCompletion();
        }

        public void EnemyReachedGate(int integrityDamage, long expectedRevision)
        {
            RequireRunning(expectedRevision);
            RequireActiveEnemy();
            if (integrityDamage <= 0)
                throw new ArgumentOutOfRangeException(nameof(integrityDamage));

            ActiveEnemyCount--;
            GateIntegrity = Math.Max(0, GateIntegrity - integrityDamage);
            Revision++;

            if (GateIntegrity == 0)
            {
                Phase = DefenseWavePhase.Failed;
                return;
            }

            EvaluateWaveCompletion();
        }

        public void PrepareNextWave(long expectedRevision)
        {
            RequireRevision(expectedRevision);
            if (Phase != DefenseWavePhase.Cleared)
                throw new InvalidOperationException(
                    "The next defense wave can be prepared only after a clear.");

            RemainingEnemiesToSpawn = 0;
            ActiveEnemyCount = 0;
            Phase = DefenseWavePhase.Ready;
            Revision++;
        }

        private void EvaluateWaveCompletion()
        {
            if (Phase == DefenseWavePhase.Running &&
                RemainingEnemiesToSpawn == 0 &&
                ActiveEnemyCount == 0)
            {
                Phase = DefenseWavePhase.Cleared;
            }
        }

        private void RequireRunning(long expectedRevision)
        {
            RequireRevision(expectedRevision);
            if (Phase != DefenseWavePhase.Running)
                throw new InvalidOperationException(
                    "Defense enemy transitions require a running wave.");
        }

        private void RequireActiveEnemy()
        {
            if (ActiveEnemyCount <= 0)
                throw new InvalidOperationException(
                    "No active defense enemy can be resolved.");
        }

        private void RequireRevision(long expectedRevision)
        {
            if (expectedRevision != Revision)
                throw new InvalidOperationException("Revision conflict.");
        }
    }
}
