using System;
using System.Collections.Generic;
using Necrom.Core.Domain;
using UnityEngine;

namespace Necrom.FirstPlayable.Runtime
{
    [DefaultExecutionOrder(500)]
    [DisallowMultipleComponent]
    public sealed class FirstPlayableGatePressureController : MonoBehaviour
    {
        sealed class ThreatProgress
        {
            public EnemyRuntimeEntity Target;
            public float ElapsedSeconds;
        }

        readonly List<ThreatProgress> _threats = new List<ThreatProgress>();
        FirstPlayableDefenseWaveRuntimeController _defense;
        EnemySpawnController _enemies;
        float _travelDurationSeconds;
        int _integrityDamage;
        bool _initialized;
        Func<int,bool> _permanentDefenderHit;
        public void ConfigurePermanentDefender(Func<int,bool> defenderHit)
        {
            if(_permanentDefenderHit!=null)throw new InvalidOperationException("Permanent defender already bound");
            _permanentDefenderHit=defenderHit??throw new ArgumentNullException(nameof(defenderHit));
        }

        public bool IsInitialized => _initialized;
        public float TravelDurationSeconds => _travelDurationSeconds;
        public int IntegrityDamage => _integrityDamage;
        public void ConfigureWaveDamage(int scaledDamage)
        {
            EnsureInitialized();
            if(scaledDamage<=0)throw new ArgumentOutOfRangeException(nameof(scaledDamage));
            _integrityDamage=scaledDamage;
        }

        public void Initialize(
            FirstPlayableDefenseWaveRuntimeController defense,
            EnemySpawnController enemies,
            float travelDurationSeconds,
            int integrityDamage)
        {
            if (_initialized)
                throw new InvalidOperationException("Gate pressure is already initialized.");
            if (travelDurationSeconds <= 0f ||
                float.IsNaN(travelDurationSeconds) ||
                float.IsInfinity(travelDurationSeconds))
                throw new ArgumentOutOfRangeException(nameof(travelDurationSeconds));
            if (integrityDamage <= 0)
                throw new ArgumentOutOfRangeException(nameof(integrityDamage));

            _defense = defense ?? throw new ArgumentNullException(nameof(defense));
            _enemies = enemies ?? throw new ArgumentNullException(nameof(enemies));
            _travelDurationSeconds = travelDurationSeconds;
            _integrityDamage = integrityDamage;
            _initialized = true;
        }

        public void ResetForWave()
        {
            EnsureInitialized();
            _threats.Clear();
        }

        public void RegisterThreat(EnemyRuntimeEntity target)
        {
            EnsureInitialized();
            if (target == null)
                throw new ArgumentNullException(nameof(target));
            if (target.Model == null ||
                target.Model.Faction != Faction.Enemy ||
                target.Model.LifeState != CombatantLifeState.Active ||
                !_enemies.IsTrackedThreat(target))
            {
                throw new InvalidOperationException(
                    "Gate pressure requires an active tracked Enemy threat.");
            }

            for (var i = 0; i < _threats.Count; i++)
                if (ReferenceEquals(_threats[i].Target, target))
                    throw new InvalidOperationException(
                        "Threat is already registered for gate pressure.");

            _threats.Add(new ThreatProgress
            {
                Target = target,
                ElapsedSeconds = 0f
            });
        }

        public bool TryGetProgress(
            EnemyRuntimeEntity target,
            out float progress)
        {
            EnsureInitialized();
            if (target != null)
            {
                for (var i = 0; i < _threats.Count; i++)
                {
                    var entry = _threats[i];
                    if (ReferenceEquals(entry.Target, target))
                    {
                        progress = Mathf.Clamp01(
                            entry.ElapsedSeconds / _travelDurationSeconds);
                        return true;
                    }
                }
            }

            progress = 0f;
            return false;
        }

        public bool TryGetFirstActiveProgress(
            out EnemyRuntimeEntity target,
            out float progress)
        {
            EnsureInitialized();
            if (_enemies.TryGetFirstActiveTarget(out target) &&
                TryGetProgress(target, out progress))
                return true;

            target = null;
            progress = 0f;
            return false;
        }

        public void Advance(float deltaTimeSeconds)
        {
            EnsureInitialized();
            if (float.IsNaN(deltaTimeSeconds) ||
                float.IsInfinity(deltaTimeSeconds) ||
                deltaTimeSeconds < 0f)
                throw new ArgumentOutOfRangeException(nameof(deltaTimeSeconds));

            if (_defense.Phase != DefenseWavePhase.Running)
                return;

            for (var i = _threats.Count - 1; i >= 0; i--)
            {
                var entry = _threats[i];
                var target = entry.Target;
                if (target == null ||
                    target.Model == null ||
                    target.Model.LifeState != CombatantLifeState.Active ||
                    !_enemies.IsTrackedThreat(target))
                {
                    _threats.RemoveAt(i);
                    continue;
                }

                entry.ElapsedSeconds += deltaTimeSeconds;
            }

            for (var i = 0; i < _threats.Count; i++)
            {
                if (_defense.Phase != DefenseWavePhase.Running)
                    break;

                var entry = _threats[i];
                var target = entry.Target;
                if (target == null ||
                    target.Model == null ||
                    target.Model.LifeState != CombatantLifeState.Active ||
                    !_enemies.IsTrackedThreat(target))
                    continue;

                if (entry.ElapsedSeconds + 0.0001f < _travelDurationSeconds)
                    continue;

                // A living permanent defender intercepts the breach without consuming the enemy threat.
                if(_permanentDefenderHit != null && _permanentDefenderHit(_integrityDamage))
                {
                    entry.ElapsedSeconds=0f;
                    continue;
                }
                _defense.RecordGateBreach(target, _integrityDamage);
                _threats.RemoveAt(i);
                i--;
            }
        }

        void Update()
        {
            if (_initialized)
                Advance(Time.deltaTime);
        }

        void EnsureInitialized()
        {
            if (!_initialized || _defense == null || _enemies == null)
                throw new InvalidOperationException(
                    "Gate pressure controller is not initialized.");
        }
    }
}
