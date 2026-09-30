using System;
using Necrom.Core.Domain;
using DomainEntityId = Necrom.Core.Domain.EntityId;
using UnityEngine;

namespace Necrom.FirstPlayable.Runtime
{
    public sealed class FirstPlayableTargetingController : MonoBehaviour
    {
        private NecromancerRuntimeEntity _necromancer;
        private EnemySpawnController _enemies;

        public DomainEntityId? CurrentTargetId
        {
            get
            {
                if (!TryAcquireTarget(out var target))
                    return null;
                return target.Model.Id;
            }
        }

        public void Initialize(
            NecromancerRuntimeEntity necromancer,
            EnemySpawnController enemies)
        {
            _necromancer = necromancer ?? throw new ArgumentNullException(nameof(necromancer));
            _enemies = enemies ?? throw new ArgumentNullException(nameof(enemies));

            if (_necromancer.Model == null ||
                _necromancer.Model.Faction != Faction.Player)
            {
                throw new InvalidOperationException(
                    "Targeting requires an initialized Player necromancer.");
            }
        }

        public bool TryAcquireTarget(out EnemyRuntimeEntity target)
        {
            EnsureInitialized();
            var current = _enemies.CurrentTarget;
            if (current != null &&
                current.Model != null &&
                current.Model.Faction == Faction.Enemy &&
                current.Model.LifeState == CombatantLifeState.Active)
            {
                target = current;
                return true;
            }

            target = null;
            return false;
        }

        private void EnsureInitialized()
        {
            if (_necromancer == null || _enemies == null)
                throw new InvalidOperationException("Targeting controller is not initialized.");
        }
    }
}