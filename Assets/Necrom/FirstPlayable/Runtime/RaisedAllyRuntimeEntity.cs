using System;
using Necrom.Core.Domain;
using UnityEngine;

namespace Necrom.FirstPlayable.Runtime
{
    public sealed class RaisedAllyRuntimeEntity : MonoBehaviour
    {
        public Combatant Model { get; private set; }
        public int Slot { get; private set; }
        public BasicAutoBehaviorSpec AutoBehaviorSpec { get; private set; }

        public void Initialize(
            Combatant model,
            int slot,
            BasicAutoBehaviorSpec autoBehaviorSpec)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            if (model.Faction != Faction.Player)
                throw new InvalidOperationException("Raised ally runtime entity requires Player faction.");
            if (slot < 0 || slot >= Formation.Capacity)
                throw new ArgumentOutOfRangeException(nameof(slot));
            if (autoBehaviorSpec == null)
                throw new ArgumentNullException(nameof(autoBehaviorSpec));
            if (Model != null)
                throw new InvalidOperationException("Raised ally runtime entity is already initialized.");

            Model = model;
            Slot = slot;
            AutoBehaviorSpec = autoBehaviorSpec;
        }

        public bool TryCreateBasicAttack(out BasicAttackIntent intent)
        {
            if (Model == null || AutoBehaviorSpec == null)
                throw new InvalidOperationException("Raised ally runtime entity is not initialized.");

            if (Model.LifeState != CombatantLifeState.Active)
            {
                intent = null;
                return false;
            }

            intent = new BasicAttackIntent(
                Model.Id,
                AutoBehaviorSpec.DamagePerAction);
            return true;
        }
    }
}