using System;

namespace Necrom.Core.Domain
{
    // Local first-playable orchestration assumes serialized application commands.
    // A future remote authoritative implementation must wrap raise-source consumption
    // and formation mutation in one authoritative transaction as required by 06-11.
    public sealed class FirstPlayableEncounter
    {
        private readonly RaiseService _raiseService;
        public BattleStateMachine Battle { get; }
        public Formation Formation { get; }

        public FirstPlayableEncounter(RaiseService raiseService, BattleStateMachine battle, Formation formation)
        {
            _raiseService = raiseService ?? throw new ArgumentNullException(nameof(raiseService));
            Battle = battle ?? throw new ArgumentNullException(nameof(battle));
            Formation = formation ?? throw new ArgumentNullException(nameof(formation));
        }

        public RaiseResult RaiseIntoFormation(
            RaiseSource source,
            long expectedSourceRevision,
            EntityId undeadId,
            int restoredHealth,
            int slot,
            long expectedFormationRevision)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (source.Revision != expectedSourceRevision) throw new InvalidOperationException("Raise source revision conflict.");
            if (source.State != RaiseSourceState.Available) throw new InvalidOperationException("Raise source is unavailable.");

            Formation.EnsureCanAssign(slot, undeadId, expectedFormationRevision);
            var result = _raiseService.Raise(source, expectedSourceRevision, undeadId, restoredHealth);
            Formation.Assign(slot, result.Undead.Id, expectedFormationRevision);
            return result;
        }

        public bool IsReadyForNextCombat(EntityId undeadId)
        {
            for (var i = 0; i < Formation.Capacity; i++)
            {
                var unit = Formation.GetSlot(i);
                if (unit.HasValue && unit.Value.Equals(undeadId)) return true;
            }
            return false;
        }
    }
}
