using System;
using Necrom.Core.Domain;
using UnityEngine;

namespace Necrom.FirstPlayable.Runtime
{
    public sealed class NecromancerRuntimeEntity : MonoBehaviour
    {
        public Combatant Model { get; private set; }
        public NecromancerBasicAutoBehavior AutoBehavior { get; private set; }

        public void Initialize(Combatant model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            if (model.Faction != Faction.Player)
                throw new InvalidOperationException("Necromancer runtime entity requires Player faction.");
            if (Model != null)
                throw new InvalidOperationException("Necromancer runtime entity is already initialized.");

            Model = model;
        }

        public void AttachAutoBehavior(NecromancerBasicAutoBehavior behavior)
        {
            if (behavior == null) throw new ArgumentNullException(nameof(behavior));
            if (Model == null)
                throw new InvalidOperationException("Necromancer runtime entity must be initialized before attaching auto behavior.");
            if (AutoBehavior != null)
                throw new InvalidOperationException("Necromancer auto behavior is already attached.");
            if (!ReferenceEquals(Model, behavior.Actor))
                throw new InvalidOperationException("Necromancer auto behavior actor must match the bound model.");

            AutoBehavior = behavior;
        }
    }
}
