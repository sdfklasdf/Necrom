using System;
using Necrom.Core.Application;
using Necrom.Core.Domain;
using UnityEngine;

namespace Necrom.FirstPlayable.Runtime
{
    public sealed class FirstPlayableDefenseWaveHudState
    {
        public DefenseWavePhase Phase { get; }
        public int WaveNumber { get; }
        public int GateIntegrity { get; }
        public int GateMaxIntegrity { get; }
        public int RemainingEnemiesToSpawn { get; }
        public int ActiveEnemyCount { get; }

        public FirstPlayableDefenseWaveHudState(
            DefenseWavePhase phase,
            int waveNumber,
            int gateIntegrity,
            int gateMaxIntegrity,
            int remainingEnemiesToSpawn,
            int activeEnemyCount)
        {
            Phase = phase;
            WaveNumber = waveNumber;
            GateIntegrity = gateIntegrity;
            GateMaxIntegrity = gateMaxIntegrity;
            RemainingEnemiesToSpawn = remainingEnemiesToSpawn;
            ActiveEnemyCount = activeEnemyCount;
        }
    }

    /// <summary>
    /// Runtime bridge between the balance-neutral IdleDefenseEncounter domain
    /// and Unity threat objects. It deliberately does not own targeting,
    /// Raise, Formation, combat damage, animation or audio.
    /// </summary>
    public sealed class FirstPlayableDefenseWaveRuntimeController
        : MonoBehaviour
    {
        private FirstPlayableApplicationService _application;
        private EnemySpawnController _enemies;
        private IdleDefenseEncounter _defense;
        private Func<string> _resolveCommandIdProvider;

        public bool IsInitialized =>
            _application != null &&
            _enemies != null &&
            _defense != null &&
            _resolveCommandIdProvider != null;

        public DefenseWavePhase Phase
        {
            get
            {
                EnsureInitialized();
                return _defense.Phase;
            }
        }

        public int WaveNumber
        {
            get
            {
                EnsureInitialized();
                return _defense.WaveNumber;
            }
        }

        public int GateIntegrity
        {
            get
            {
                EnsureInitialized();
                return _defense.GateIntegrity;
            }
        }

        public int GateMaxIntegrity
        {
            get
            {
                EnsureInitialized();
                return _defense.GateMaxIntegrity;
            }
        }

        public int RemainingEnemiesToSpawn
        {
            get
            {
                EnsureInitialized();
                return _defense.RemainingEnemiesToSpawn;
            }
        }

        public int ActiveEnemyCount
        {
            get
            {
                EnsureInitialized();
                return _defense.ActiveEnemyCount;
            }
        }

        public void Initialize(
            FirstPlayableApplicationService application,
            EnemySpawnController enemies,
            int gateMaxIntegrity,
            Func<string> resolveCommandIdProvider)
        {
            if (IsInitialized)
                throw new InvalidOperationException(
                    "Defense wave runtime controller is already initialized.");

            _application = application ??
                throw new ArgumentNullException(nameof(application));
            _enemies = enemies ??
                throw new ArgumentNullException(nameof(enemies));
            _resolveCommandIdProvider = resolveCommandIdProvider ??
                throw new ArgumentNullException(
                    nameof(resolveCommandIdProvider));
            _defense = new IdleDefenseEncounter(gateMaxIntegrity);
        }

        public void StartWave(
            int waveNumber,
            int totalEnemyCount)
        {
            EnsureInitialized();
            _defense.StartWave(
                waveNumber,
                totalEnemyCount,
                _defense.Revision);
        }

        public void RegisterSpawnedThreat(
            EnemyRuntimeEntity target)
        {
            EnsureInitialized();
            if (target == null)
                throw new ArgumentNullException(nameof(target));
            if (target.Model == null ||
                target.Model.Faction != Faction.Enemy ||
                target.Model.LifeState != CombatantLifeState.Active)
            {
                throw new InvalidOperationException(
                    "Defense wave requires an active Enemy threat.");
            }
            if (!_enemies.IsTrackedThreat(target))
            {
                throw new InvalidOperationException(
                    "Spawned defense threat must be owned by EnemySpawnController.");
            }

            _defense.EnemySpawned(_defense.Revision);
        }

        public void RecordDefeatedThreat(
            EnemyRuntimeEntity target)
        {
            EnsureInitialized();
            if (target == null)
                throw new ArgumentNullException(nameof(target));
            if (target.Model == null ||
                target.Model.LifeState != CombatantLifeState.Defeated)
            {
                throw new InvalidOperationException(
                    "Defense defeat resolution requires a defeated Enemy.");
            }
            if (!_enemies.IsTrackedThreat(target))
            {
                throw new InvalidOperationException(
                    "Defeated Enemy is not an active tracked threat.");
            }

            _defense.EnemyDefeated(_defense.Revision);
            _enemies.ResolveDefeatedThreat(target);
            ResolveBattleIfTerminal();
        }

        public void RecordGateBreach(
            EnemyRuntimeEntity target,
            int integrityDamage)
        {
            EnsureInitialized();
            if (target == null)
                throw new ArgumentNullException(nameof(target));
            if (target.Model == null ||
                target.Model.LifeState != CombatantLifeState.Active)
            {
                throw new InvalidOperationException(
                    "Gate breach requires an active Enemy.");
            }
            if (!_enemies.IsTrackedThreat(target))
            {
                throw new InvalidOperationException(
                    "Gate-breaching Enemy is not an active tracked threat.");
            }

            _defense.EnemyReachedGate(
                integrityDamage,
                _defense.Revision);
            _enemies.ResolveEscapedThreat(target);
            ResolveBattleIfTerminal();
        }

        public void PrepareNextWave()
        {
            EnsureInitialized();
            _defense.PrepareNextWave(_defense.Revision);
        }

        public FirstPlayableDefenseWaveHudState CaptureHudState()
        {
            EnsureInitialized();
            return new FirstPlayableDefenseWaveHudState(
                _defense.Phase,
                _defense.WaveNumber,
                _defense.GateIntegrity,
                _defense.GateMaxIntegrity,
                _defense.RemainingEnemiesToSpawn,
                _defense.ActiveEnemyCount);
        }

        private void ResolveBattleIfTerminal()
        {
            if (_application.BattlePhase != BattlePhase.Running)
                return;

            bool? playerWon = null;
            if (_defense.Phase == DefenseWavePhase.Cleared)
                playerWon = true;
            else if (_defense.Phase == DefenseWavePhase.Failed)
                playerWon = false;

            if (!playerWon.HasValue)
                return;

            var commandId = _resolveCommandIdProvider();
            if (string.IsNullOrWhiteSpace(commandId))
            {
                throw new InvalidOperationException(
                    "Defense resolve command id provider returned no id.");
            }

            _application.Execute(
                new ResolveBattleCommand(
                    commandId,
                    playerWon.Value,
                    _application.BattleRevision));
        }

        private void EnsureInitialized()
        {
            if (!IsInitialized)
            {
                throw new InvalidOperationException(
                    "Defense wave runtime controller is not initialized.");
            }
        }
    }
}
