using System;
using Necrom.Core.Domain;
using UnityEngine;

namespace Necrom.FirstPlayable.Runtime
{
    public sealed class EnemySpawnController : MonoBehaviour
    {
        public EnemyRuntimeEntity CurrentTarget { get; private set; }

        public EnemyRuntimeEntity SpawnEnemy(
            string instanceId,
            EnemyArchetypeDefinition definition,
            RectTransform spawnZone)
        {
            if (string.IsNullOrWhiteSpace(instanceId))
                throw new ArgumentException("Enemy instance id is required.", nameof(instanceId));
            if (definition == null) throw new ArgumentNullException(nameof(definition));
            if (spawnZone == null) throw new ArgumentNullException(nameof(spawnZone));
            if (CurrentTarget != null &&
                CurrentTarget.Model != null &&
                CurrentTarget.Model.LifeState == CombatantLifeState.Active)
                throw new InvalidOperationException("An active enemy target already exists.");

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

            var runtimeEntity = entityObject.GetComponent<EnemyRuntimeEntity>();
            runtimeEntity.Initialize(model, definition.RoleId);
            CurrentTarget = runtimeEntity;
            return runtimeEntity;
        }
    }
}
