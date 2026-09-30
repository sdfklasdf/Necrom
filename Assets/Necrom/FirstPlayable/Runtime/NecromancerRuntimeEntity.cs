using System;
using Necrom.Core.Domain;
using UnityEngine;

namespace Necrom.FirstPlayable.Runtime
{
    public sealed class NecromancerRuntimeEntity : MonoBehaviour
    {
        public Combatant Model { get; private set; }

        public void Initialize(Combatant model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            if (model.Faction != Faction.Player)
                throw new InvalidOperationException("Necromancer runtime entity requires Player faction.");
            if (Model != null)
                throw new InvalidOperationException("Necromancer runtime entity is already initialized.");

            Model = model;
        }
    }
}
