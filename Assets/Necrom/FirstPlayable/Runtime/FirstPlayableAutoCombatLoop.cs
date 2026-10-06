using System;
using Necrom.Core.Application;
using Necrom.Core.Domain;
using DomainEntityId = Necrom.Core.Domain.EntityId;
using UnityEngine;

namespace Necrom.FirstPlayable.Runtime
{
    public sealed class FirstPlayableAutoCombatLoop : MonoBehaviour
    {
        public event Action<DomainEntityId, DamageDeathResult> AttackApplied;
        private FirstPlayableBattleRuntimeController _battle;
        private NecromancerRuntimeEntity _necromancer;
        private FirstPlayableTargetingController _targeting;
        private FirstPlayableDamageDeathPipeline _damageDeathPipeline;
        private Func<DomainEntityId, DomainEntityId> _raiseSourceIdForEnemy;
        private Func<string> _resolveCommandIdProvider;
        private FirstPlayableDefenseWaveRuntimeController _defenseWave;
        private double _elapsedMilliseconds;
        private bool _hasCachedRaiseSourceId;
        private DomainEntityId _cachedEnemyId;
        private DomainEntityId _cachedRaiseSourceId;
        private bool _initialized;
        private const double TimingEpsilonMilliseconds = 0.001d;

        public void Initialize(
            FirstPlayableBattleRuntimeController battle,
            NecromancerRuntimeEntity necromancer,
            FirstPlayableTargetingController targeting,
            FirstPlayableDamageDeathPipeline damageDeathPipeline,
            Func<DomainEntityId, DomainEntityId> raiseSourceIdForEnemy,
            Func<string> resolveCommandIdProvider)
        {
            _battle = battle ?? throw new ArgumentNullException(nameof(battle));
            _necromancer = necromancer ?? throw new ArgumentNullException(nameof(necromancer));
            _targeting = targeting ?? throw new ArgumentNullException(nameof(targeting));
            _damageDeathPipeline = damageDeathPipeline
                ?? throw new ArgumentNullException(nameof(damageDeathPipeline));
            _raiseSourceIdForEnemy = raiseSourceIdForEnemy
                ?? throw new ArgumentNullException(nameof(raiseSourceIdForEnemy));
            _resolveCommandIdProvider = resolveCommandIdProvider
                ?? throw new ArgumentNullException(nameof(resolveCommandIdProvider));
            _elapsedMilliseconds = 0d;
            _hasCachedRaiseSourceId = false;
            _initialized = true;
        }

        public void ConfigureDefenseWave(
            FirstPlayableDefenseWaveRuntimeController defenseWave)
        {
            EnsureInitialized();
            if (defenseWave == null)
                throw new ArgumentNullException(nameof(defenseWave));
            if (_defenseWave != null)
                throw new InvalidOperationException(
                    "Defense wave runtime is already configured.");

            _defenseWave = defenseWave;
        }

        public void Advance(float deltaTimeSeconds)
        {
            EnsureInitialized();

            if (float.IsNaN(deltaTimeSeconds) ||
                float.IsInfinity(deltaTimeSeconds) ||
                deltaTimeSeconds < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(deltaTimeSeconds));
            }

            if (_battle.Phase != BattlePhase.Running)
            {
                _elapsedMilliseconds = 0d;
                return;
            }
            var behavior = _necromancer.AutoBehavior;
            if (_necromancer.Model == null ||
                behavior == null ||
                !ReferenceEquals(_necromancer.Model, behavior.Actor))
            {
                throw new InvalidOperationException(
                    "A matching attached necromancer auto behavior is required.");
            }

            if (!_targeting.TryAcquireTarget(out var target))
            {
                _elapsedMilliseconds = 0d;
                return;
            }

            _elapsedMilliseconds += deltaTimeSeconds * 1000d;
            var interval = behavior.Spec.AttackIntervalMilliseconds;

            while (_elapsedMilliseconds + TimingEpsilonMilliseconds >= interval)
            {
                if (_battle.Phase != BattlePhase.Running)
                {
                    _elapsedMilliseconds = 0d;
                    return;
                }

                if (!_targeting.TryAcquireTarget(out target))
                {
                    _elapsedMilliseconds = 0d;
                    return;
                }

                if (!behavior.TryCreateBasicAttack(out var intent))
                {
                    _elapsedMilliseconds = 0d;
                    return;
                }

                string resolveCommandId = null;
                if (target.Model.Health <= intent.Damage &&
                    _defenseWave == null)
                {
                    resolveCommandId = _resolveCommandIdProvider();
                    if (string.IsNullOrWhiteSpace(resolveCommandId))
                    {
                        throw new InvalidOperationException(
                            "Resolve command id provider returned an invalid id.");
                    }
                }

                var raiseSourceId = GetStableRaiseSourceId(target.Model.Id);
                _elapsedMilliseconds = Math.Max(0d, _elapsedMilliseconds - interval);
                var damageResult = _damageDeathPipeline.Apply(
                    target,
                    intent.Damage,
                    raiseSourceId);

                if (damageResult.Changed) AttackApplied?.Invoke(_necromancer.Model.Id, damageResult);

                if (!damageResult.BecameDefeated)
                    continue;

                if (_defenseWave != null)
                {
                    _defenseWave.RecordDefeatedThreat(target);
                    _elapsedMilliseconds = 0d;
                    return;
                }

                var command = new ResolveBattleCommand(
                    resolveCommandId,
                    true,
                    _battle.Revision);
                _battle.ResolveFromCombatResult(command);
                _elapsedMilliseconds = 0d;
                return;
            }
        }

        private void Update()
        {
            if (!_initialized) return;
            Advance(Time.deltaTime);
        }

        private DomainEntityId GetStableRaiseSourceId(DomainEntityId enemyId)
        {
            if (_hasCachedRaiseSourceId && _cachedEnemyId.Equals(enemyId))
                return _cachedRaiseSourceId;

            var value = _raiseSourceIdForEnemy(enemyId);
            _cachedEnemyId = enemyId;
            _cachedRaiseSourceId = value;
            _hasCachedRaiseSourceId = true;
            return value;
        }

        private void EnsureInitialized()
        {
            if (!_initialized ||
                _battle == null ||
                _necromancer == null ||
                _targeting == null ||
                _damageDeathPipeline == null ||
                _raiseSourceIdForEnemy == null ||
                _resolveCommandIdProvider == null)
            {
                throw new InvalidOperationException(
                    "Auto combat loop is not initialized.");
            }
        }
    }
}