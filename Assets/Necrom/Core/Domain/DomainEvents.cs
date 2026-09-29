using System;

namespace Necrom.Core.Domain
{
    public abstract class DomainEvent
    {
        public string EventId { get; }
        public string EventType { get; }

        protected DomainEvent(string eventId, string eventType)
        {
            if (string.IsNullOrWhiteSpace(eventId)) throw new ArgumentException("Event id is required.", nameof(eventId));
            if (string.IsNullOrWhiteSpace(eventType)) throw new ArgumentException("Event type is required.", nameof(eventType));
            EventId = eventId;
            EventType = eventType;
        }
    }

    public sealed class UnitRaised : DomainEvent
    {
        public EntityId SourceId { get; }
        public EntityId UnitId { get; }
        public string ArchetypeId { get; }

        public UnitRaised(string eventId, EntityId sourceId, EntityId unitId, string archetypeId)
            : base(eventId, "unit.raised")
        {
            if (string.IsNullOrWhiteSpace(archetypeId)) throw new ArgumentException("Archetype is required.", nameof(archetypeId));
            SourceId = sourceId;
            UnitId = unitId;
            ArchetypeId = archetypeId;
        }
    }

    public sealed class UnitAssignedToFormation : DomainEvent
    {
        public EntityId UnitId { get; }
        public int Slot { get; }

        public UnitAssignedToFormation(string eventId, EntityId unitId, int slot)
            : base(eventId, "formation.unit_assigned")
        {
            if (slot < 0 || slot >= Formation.Capacity) throw new ArgumentOutOfRangeException(nameof(slot));
            UnitId = unitId;
            Slot = slot;
        }
    }
}
