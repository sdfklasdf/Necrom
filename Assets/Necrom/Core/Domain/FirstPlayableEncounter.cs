using System;

namespace Necrom.Core.Domain
{
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
            if (Formation.GetSlot(slot).HasValue)
                throw new InvalidOperationException("Target formation slot is occupied.");

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
