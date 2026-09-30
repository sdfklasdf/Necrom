using System;
using Necrom.Core.Domain;
using UnityEngine;

namespace Necrom.FirstPlayable.Runtime
{
    public sealed class NecromancerAnchorController : MonoBehaviour
    {
        public NecromancerRuntimeEntity CurrentNecromancer { get; private set; }

        public NecromancerRuntimeEntity BindNecromancer(Combatant model, RectTransform spawnZone)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            if (spawnZone == null) throw new ArgumentNullException(nameof(spawnZone));
            if (CurrentNecromancer != null)
                throw new InvalidOperationException("A necromancer anchor is already bound.");
            if (model.Faction != Faction.Player)
                throw new InvalidOperationException("Necromancer anchor requires Player faction.");

            var entityObject = new GameObject(
                $"Necromancer:{model.Id.Value}",
                typeof(RectTransform),
                typeof(NecromancerRuntimeEntity));

            var rect = entityObject.GetComponent<RectTransform>();
            rect.SetParent(spawnZone, false);
            rect.anchoredPosition = Vector2.zero;
            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.identity;

            var runtimeEntity = entityObject.GetComponent<NecromancerRuntimeEntity>();
            runtimeEntity.Initialize(model);
            CurrentNecromancer = runtimeEntity;
            return runtimeEntity;
        }
    }
}
