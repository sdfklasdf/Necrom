using System;
using System.Linq;
using Necrom.Core.Application;
using Necrom.Core.Domain;
using DomainEntityId = Necrom.Core.Domain.EntityId;
using UnityEngine;

namespace Necrom.FirstPlayable.Runtime
{
    public sealed class FirstPlayableAlliedAutoCombatLoop : MonoBehaviour
    {
        public event Action<DomainEntityId, DamageDeathResult> AttackApplied;
        private readonly double[] _elapsedMilliseconds =
            new double[Formation.Capacity];
        private FirstPlayableApplicationService _application;
        private FirstPlayableAlliedRosterController _roster;
        private FirstPlayableTargetingController _targeting;
        private FirstPlayableDamageDeathPipeline _damageDeathPipeline;
        private Func<DomainEntityId, DomainEntityId> _raiseSourceIdForEnemy;
        private Func<string> _resolveCommandIdProvider;
        private FirstPlayableCombatHudSession _hudSession;
        private bool _hasCachedRaiseSourceId;
        private DomainEntityId _cachedEnemyId;
        private DomainEntityId _cachedRaiseSourceId;
        private bool _initialized;
        private const double TimingEpsilonMilliseconds = 0.001d;

        public void Initialize(
            FirstPlayableApplicationService application,
            FirstPlayableAlliedRosterController roster,
            FirstPlayableTargetingController targeting,
            FirstPlayableDamageDeathPipeline damageDeathPipeline,
            Func<DomainEntityId, DomainEntityId> raiseSourceIdForEnemy,
            Func<string> resolveCommandIdProvider)
        {
            _application = application ?? throw new ArgumentNullException(nameof(application));
            _roster = roster ?? throw new ArgumentNullException(nameof(roster));
            _targeting = targeting ?? throw new ArgumentNullException(nameof(targeting));
            _damageDeathPipeline = damageDeathPipeline
                ?? throw new ArgumentNullException(nameof(damageDeathPipeline));
            _raiseSourceIdForEnemy = raiseSourceIdForEnemy
                ?? throw new ArgumentNullException(nameof(raiseSourceIdForEnemy));
            _resolveCommandIdProvider = resolveCommandIdProvider
                ?? throw new ArgumentNullException(nameof(resolveCommandIdProvider));
            ClearElapsed();
            _hasCachedRaiseSourceId = false;
            _initialized = true;
        }

        public void ConfigureHudSession(
            FirstPlayableCombatHudSession hudSession)
        {
            EnsureInitialized();
            if (hudSession == null)
                throw new ArgumentNullException(nameof(hudSession));
            if (_hudSession != null)
                throw new InvalidOperationException(
                    "Combat HUD session is already configured.");

            _hudSession = hudSession;
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

            if (_application.BattlePhase != BattlePhase.Running)
            {
                ClearElapsed();
                return;
            }

            if (!_targeting.TryAcquireTarget(out _))
            {
                ClearElapsed();
                return;
            }

            for (var slot = 0; slot < Formation.Capacity; slot++)
            {
                var ally = _roster.GetSlot(slot);
                if (ally == null ||
                    ally.Model == null ||
                    ally.Model.LifeState != CombatantLifeState.Active)
                {
                    _elapsedMilliseconds[slot] = 0d;
                    continue;
                }

                _elapsedMilliseconds[slot] +=
                    deltaTimeSeconds * 1000d;
                var interval =
                    ally.AutoBehaviorSpec.AttackIntervalMilliseconds;

                while (_elapsedMilliseconds[slot] +
                    TimingEpsilonMilliseconds >= interval)
                {
                    if (_application.BattlePhase != BattlePhase.Running)
                    {
                        ClearElapsed();
                        return;
                    }

                    if (!_targeting.TryAcquireTarget(out var target))
                    {
                        ClearElapsed();
                        return;
                    }

                    if (!ally.TryCreateBasicAttack(out var intent))
                    {
                        _elapsedMilliseconds[slot] = 0d;
                        break;
                    }

                    string resolveCommandId = null;
                    if (target.Model.Health <= intent.Damage)
                    {
                        resolveCommandId = _resolveCommandIdProvider();
                        if (string.IsNullOrWhiteSpace(resolveCommandId))
                            throw new InvalidOperationException(
                                "Resolve command id provider returned an invalid id.");
                    }

                    var raiseSourceId =
                        GetStableRaiseSourceId(target.Model.Id);
                    _elapsedMilliseconds[slot] = Math.Max(
                        0d,
                        _elapsedMilliseconds[slot] - interval);

                    var damageResult = _damageDeathPipeline.Apply(
                        target,
                        intent.Damage,
                        raiseSourceId);

                    if (damageResult.Changed &&
                        _hudSession != null)
                    {
                        var damage = damageResult.Events
                            .OfType<DamageApplied>()
                            .SingleOrDefault();
                        if (damage != null)
                        {
                            _hudSession.ObserveContribution(
                                ally.Model.Id,
                                damage.TargetId,
                                damage.ActualDamage);
                        }
                    }

                    if (damageResult.Changed) AttackApplied?.Invoke(ally.Model.Id, damageResult);

                    if (!damageResult.BecameDefeated)
                        continue;

                    _application.Execute(
                        new ResolveBattleCommand(
                            resolveCommandId,
                            true,
                            _application.BattleRevision));
                    ClearElapsed();
                    return;
                }
            }
        }

        private void Update()
        {
            if (!_initialized) return;
            Advance(Time.deltaTime);
        }

        private DomainEntityId GetStableRaiseSourceId(
            DomainEntityId enemyId)
        {
            if (_hasCachedRaiseSourceId &&
                _cachedEnemyId.Equals(enemyId))
                return _cachedRaiseSourceId;

            var value = _raiseSourceIdForEnemy(enemyId);
            _cachedEnemyId = enemyId;
            _cachedRaiseSourceId = value;
            _hasCachedRaiseSourceId = true;
            return value;
        }

        private void ClearElapsed()
        {
            for (var i = 0; i < _elapsedMilliseconds.Length; i++)
                _elapsedMilliseconds[i] = 0d;
        }

        private void EnsureInitialized()
        {
            if (!_initialized ||
                _application == null ||
                _roster == null ||
                _targeting == null ||
                _damageDeathPipeline == null ||
                _raiseSourceIdForEnemy == null ||
                _resolveCommandIdProvider == null)
            {
                throw new InvalidOperationException(
                    "Allied auto combat loop is not initialized.");
            }
        }
    }
}