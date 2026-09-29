using System;

namespace Necrom.Core.Domain
{
    public sealed class RaiseResult
    {
        public Combatant Undead { get; }
        public EntityId SourceId { get; }

        public RaiseResult(Combatant undead, EntityId sourceId)
        {
            Undead = undead ?? throw new ArgumentNullException(nameof(undead));
            SourceId = sourceId;
        }
    }

    public sealed class RaiseService
    {
        public RaiseResult Raise(RaiseSource source, long expectedRevision, EntityId newUndeadId, int restoredHealth)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (restoredHealth <= 0) throw new ArgumentOutOfRangeException(nameof(restoredHealth));
            if (source.State != RaiseSourceState.Available) throw new InvalidOperationException("Raise source is unavailable.");

            source.Consume(expectedRevision);
            var undead = new Combatant(newUndeadId, source.ArchetypeId, Faction.Player, restoredHealth);
            return new RaiseResult(undead, source.SourceId);
        }
    }
}
