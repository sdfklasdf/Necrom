using System;
using Necrom.Core.Domain;
using UnityEngine;

namespace Necrom.FirstPlayable.Runtime
{
    public sealed class EnemyRuntimeEntity : MonoBehaviour
    {
        public Combatant Model { get; private set; }
        public string RoleId { get; private set; }
        public RaiseSource RaiseSource { get; private set; }

        public void Initialize(Combatant model, string roleId)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            if (model.Faction != Faction.Enemy)
                throw new ArgumentException("Runtime enemy requires Enemy faction.", nameof(model));
            if (string.IsNullOrWhiteSpace(roleId))
                throw new ArgumentException("Role id is required.", nameof(roleId));

            Model = model;
            RoleId = roleId;
        }

        public bool ApplyDamage(int amount, Necrom.Core.Domain.EntityId raiseSourceId)
        {
            if (Model == null)
                throw new InvalidOperationException("Runtime enemy is not initialized.");

            var changed = Model.ApplyDamage(amount);
            if (!changed) return false;

            if (Model.LifeState == CombatantLifeState.Defeated && RaiseSource == null)
                RaiseSource = new RaiseSource(raiseSourceId, Model);

            return true;
        }

        public bool TryGetAvailableRaiseSource(out RaiseSource source)
        {
            source = null;

            if (Model == null || Model.LifeState != CombatantLifeState.Defeated)
                return false;

            var candidate = RaiseSource;
            if (candidate == null || candidate.State != RaiseSourceState.Available)
                return false;

            if (!candidate.DefeatedEntityId.Equals(Model.Id))
                return false;

            source = candidate;
            return true;
        }
    }
}
