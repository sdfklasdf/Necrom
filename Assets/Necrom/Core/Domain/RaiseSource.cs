using System;

namespace Necrom.Core.Domain
{
    public enum RaiseSourceState { Available, Consumed }

    public sealed class RaiseSource
    {
        public EntityId SourceId { get; }
        public EntityId DefeatedEntityId { get; }
        public string ArchetypeId { get; }
        public RaiseSourceState State { get; private set; }
        public long Revision { get; private set; }

        public RaiseSource(EntityId sourceId, Combatant defeated)
        {
            if (defeated == null) throw new ArgumentNullException(nameof(defeated));
            if (defeated.LifeState != CombatantLifeState.Defeated) throw new InvalidOperationException("Raise source requires a defeated combatant.");
            SourceId = sourceId;
            DefeatedEntityId = defeated.Id;
            ArchetypeId = defeated.ArchetypeId;
            State = RaiseSourceState.Available;
        }

        public void Consume(long expectedRevision)
        {
            if (expectedRevision != Revision) throw new InvalidOperationException("Revision conflict.");
            if (State != RaiseSourceState.Available) throw new InvalidOperationException("Raise source is not available.");
            State = RaiseSourceState.Consumed;
            Revision++;
        }
    }
}
