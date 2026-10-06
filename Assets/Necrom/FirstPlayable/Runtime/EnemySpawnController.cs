using System;
using System.Collections.Generic;
using Necrom.Core.Domain;
using UnityEngine;

namespace Necrom.FirstPlayable.Runtime
{
    public sealed class EnemySpawnController : MonoBehaviour
    {
        private readonly List<EnemyRuntimeEntity> _activeThreats =
            new List<EnemyRuntimeEntity>();

        // Compatibility/current interaction target. In hybrid defense this may be
        // the latest defeated Raise candidate while combat targeting continues
        // through ActiveThreats.
        public EnemyRuntimeEntity CurrentTarget { get; private set; }

        public int ActiveThreatCount
        {
            get
            {
                PruneDestroyedThreats();
                return _activeThreats.Count;
            }
        }

        public EnemyRuntimeEntity SpawnEnemy(
            string instanceId,
            EnemyArchetypeDefinition definition,
            RectTransform spawnZone)
        {
            if (string.IsNullOrWhiteSpace(instanceId))
                throw new ArgumentException(
                    "Enemy instance id is required.",
                    nameof(instanceId));
            if (definition == null)
                throw new ArgumentNullException(nameof(definition));
            if (spawnZone == null)
                throw new ArgumentNullException(nameof(spawnZone));

            var entityObject = new GameObject(
                $"Enemy:{instanceId}",
                typeof(RectTransform),
                typeof(EnemyRuntimeEntity));
            entityObject.transform.SetParent(spawnZone, false);

            var model = new Combatant(
                new Necrom.Core.Domain.EntityId(instanceId),
                definition.ArchetypeId,
                Faction.Enemy,
                definition.MaxHealth);

            var runtimeEntity =
                entityObject.GetComponent<EnemyRuntimeEntity>();
            runtimeEntity.Initialize(model, definition.RoleId);

            _activeThreats.Add(runtimeEntity);
            if (CurrentTarget == null ||
                CurrentTarget.Model == null ||
                CurrentTarget.Model.LifeState !=
                    CombatantLifeState.Active)
            {
                CurrentTarget = runtimeEntity;
            }

            return runtimeEntity;
        }

        public bool TryGetFirstActiveTarget(
            out EnemyRuntimeEntity target)
        {
            PruneDestroyedThreats();

            for (var i = 0; i < _activeThreats.Count; i++)
            {
                var candidate = _activeThreats[i];
                if (candidate != null &&
                    candidate.Model != null &&
                    candidate.Model.Faction == Faction.Enemy &&
                    candidate.Model.LifeState ==
                        CombatantLifeState.Active &&
                    candidate.gameObject.activeInHierarchy)
                {
                    target = candidate;
                    return true;
                }
            }

            target = null;
            return false;
        }

        public bool IsTrackedThreat(EnemyRuntimeEntity target)
        {
            if (target == null)
                return false;

            PruneDestroyedThreats();
            return _activeThreats.Contains(target);
        }

        public void ResolveDefeatedThreat(
            EnemyRuntimeEntity target)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));
            if (target.Model == null ||
                target.Model.LifeState !=
                    CombatantLifeState.Defeated)
            {
                throw new InvalidOperationException(
                    "Only a defeated tracked threat can resolve as defeated.");
            }
            if (!_activeThreats.Remove(target))
            {
                throw new InvalidOperationException(
                    "Defeated threat is not tracked as active.");
            }

            // Preserve the defeated object as the current interaction/Raise
            // candidate while active combat targeting can move to another enemy.
            CurrentTarget = target;
        }

        public void ResolveEscapedThreat(
            EnemyRuntimeEntity target)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));
            if (target.Model == null ||
                target.Model.LifeState !=
                    CombatantLifeState.Active)
            {
                throw new InvalidOperationException(
                    "Only an active tracked threat can reach the gate.");
            }
            if (!_activeThreats.Remove(target))
            {
                throw new InvalidOperationException(
                    "Escaped threat is not tracked as active.");
            }

            if (target.gameObject.activeSelf)
                target.gameObject.SetActive(false);

            if (ReferenceEquals(CurrentTarget, target))
            {
                CurrentTarget = null;
                if (TryGetFirstActiveTarget(out var next))
                    CurrentTarget = next;
            }
        }

        private void PruneDestroyedThreats()
        {
            for (var i = _activeThreats.Count - 1; i >= 0; i--)
            {
                if (_activeThreats[i] == null)
                    _activeThreats.RemoveAt(i);
            }
        }
    }
}
