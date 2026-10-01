using System;
using System.Collections.Generic;

namespace Necrom.Core.Domain
{
    public sealed class DamageApplied : DomainEvent
    {
        public EntityId TargetId { get; }
        public int RequestedDamage { get; }
        public int ActualDamage { get; }
        public int HealthBefore { get; }
        public int HealthAfter { get; }

        public DamageApplied(
            string eventId,
            EntityId targetId,
            int requestedDamage,
            int actualDamage,
            int healthBefore,
            int healthAfter)
            : base(eventId, "combat.damage_applied")
        {
            if (requestedDamage <= 0)
                throw new ArgumentOutOfRangeException(nameof(requestedDamage));
            if (actualDamage <= 0 || actualDamage > requestedDamage)
                throw new ArgumentOutOfRangeException(nameof(actualDamage));
            if (healthBefore <= 0)
                throw new ArgumentOutOfRangeException(nameof(healthBefore));
            if (healthAfter < 0 || healthAfter >= healthBefore)
                throw new ArgumentOutOfRangeException(nameof(healthAfter));
            if (healthBefore - healthAfter != actualDamage)
                throw new ArgumentException(
                    "Actual damage must equal the observed health delta.",
                    nameof(actualDamage));

            TargetId = targetId;
            RequestedDamage = requestedDamage;
            ActualDamage = actualDamage;
            HealthBefore = healthBefore;
            HealthAfter = healthAfter;
        }
    }

    public sealed class CombatantDefeated : DomainEvent
    {
        public EntityId TargetId { get; }
        public EntityId SourceId { get; }

        public CombatantDefeated(
            string eventId,
            EntityId targetId,
            EntityId sourceId)
            : base(eventId, "combat.combatant_defeated")
        {
            TargetId = targetId;
            SourceId = sourceId;
        }
    }

    public sealed class DamageDeathResult
    {
        public bool Changed { get; }
        public bool BecameDefeated { get; }
        public IReadOnlyList<DomainEvent> Events { get; }

        public DamageDeathResult(
            bool changed,
            bool becameDefeated,
            DomainEvent[] events)
        {
            if (!changed && becameDefeated)
                throw new ArgumentException(
                    "An unchanged damage result cannot become defeated.",
                    nameof(becameDefeated));
            if (events == null)
                throw new ArgumentNullException(nameof(events));

            Changed = changed;
            BecameDefeated = becameDefeated;
            Events = Array.AsReadOnly((DomainEvent[])events.Clone());
        }
    }
}