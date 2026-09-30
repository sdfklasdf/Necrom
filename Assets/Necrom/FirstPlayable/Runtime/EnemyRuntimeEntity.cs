using System;
using Necrom.Core.Domain;
using UnityEngine;

namespace Necrom.FirstPlayable.Runtime
{
    public sealed class EnemyRuntimeEntity : MonoBehaviour
    {
        public Combatant Model { get; private set; }
        public string RoleId { get; private set; }

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
    }
}
