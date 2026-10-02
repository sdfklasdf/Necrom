using System;
using System.Collections.Generic;
using System.Linq;
using Necrom.Core.Application;
using Necrom.Core.Domain;

namespace Necrom.FirstPlayable.Runtime
{
    public enum FirstPlayableRaiseAvailabilityReason
    {
        NoTarget,
        TargetNotRaiseReady,
        SourceUnavailableOrConsumed,
        InsufficientSoul,
        Eligible
    }

    public enum FirstPlayableProofStatus
    {
        None,
        Pending,
        Observed
    }

    public sealed class SoulResourceRaiseQuote
    {
        public int Balance { get; }
        public long Revision { get; }
        public int Cost { get; }
        public bool CanAfford { get; }

        public SoulResourceRaiseQuote(
            int balance,
            long revision,
            int cost,
            bool canAfford)
        {
            if (balance < 0) throw new ArgumentOutOfRangeException(nameof(balance));
            if (revision < 0) throw new ArgumentOutOfRangeException(nameof(revision));
            if (cost <= 0) throw new ArgumentOutOfRangeException(nameof(cost));

            Balance = balance;
            Revision = revision;
            Cost = cost;
            CanAfford = canAfford;
        }
    }

    public sealed class FirstPlayableCombatHudTargetState
    {
        public EntityId EntityId { get; }
        public string ArchetypeId { get; }
        public string RoleId { get; }
        public int Health { get; }
        public CombatantLifeState LifeState { get; }

        public FirstPlayableCombatHudTargetState(
            EntityId entityId,
            string archetypeId,
            string roleId,
            int health,
            CombatantLifeState lifeState)
        {
            if (string.IsNullOrWhiteSpace(archetypeId))
                throw new ArgumentException("Archetype id is required.", nameof(archetypeId));
            if (string.IsNullOrWhiteSpace(roleId))
                throw new ArgumentException("Role id is required.", nameof(roleId));
            if (health < 0)
                throw new ArgumentOutOfRangeException(nameof(health));

            EntityId = entityId;
            ArchetypeId = archetypeId;
            RoleId = roleId;
            Health = health;
            LifeState = lifeState;
        }
    }

    public sealed class FirstPlayableCombatHudFormationSlotState
    {
        public int Slot { get; }
        public EntityId? OwnedUnitId { get; }
        public EntityId? RuntimeUnitId { get; }
        public CombatantLifeState? RuntimeLifeState { get; }
        public bool OwnershipMatchesRuntime { get; }

        public FirstPlayableCombatHudFormationSlotState(
            int slot,
            EntityId? ownedUnitId,
            EntityId? runtimeUnitId,
            CombatantLifeState? runtimeLifeState,
            bool ownershipMatchesRuntime)
        {
            if (slot < 0 || slot >= Formation.Capacity)
                throw new ArgumentOutOfRangeException(nameof(slot));

            Slot = slot;
            OwnedUnitId = ownedUnitId;
            RuntimeUnitId = runtimeUnitId;
            RuntimeLifeState = runtimeLifeState;
            OwnershipMatchesRuntime = ownershipMatchesRuntime;
        }
    }

    public sealed class FirstPlayableRaiseReceipt
    {
        public string CommandId { get; }
        public EntityId SourceId { get; }
        public EntityId UnitId { get; }
        public string ArchetypeId { get; }
        public int Slot { get; }

        public FirstPlayableRaiseReceipt(
            string commandId,
            EntityId sourceId,
            EntityId unitId,
            string archetypeId,
            int slot)
        {
            if (string.IsNullOrWhiteSpace(commandId))
                throw new ArgumentException("Command id is required.", nameof(commandId));
            if (string.IsNullOrWhiteSpace(archetypeId))
                throw new ArgumentException("Archetype id is required.", nameof(archetypeId));
            if (slot < 0 || slot >= Formation.Capacity)
                throw new ArgumentOutOfRangeException(nameof(slot));

            CommandId = commandId;
            SourceId = sourceId;
            UnitId = unitId;
            ArchetypeId = archetypeId;
            Slot = slot;
        }
    }

    public sealed class FirstPlayableContributionReceipt
    {
        public EntityId ActingUnitId { get; }
        public EntityId TargetId { get; }
        public int ActualContribution { get; }

        public FirstPlayableContributionReceipt(
            EntityId actingUnitId,
            EntityId targetId,
            int actualContribution)
        {
            if (actualContribution <= 0)
                throw new ArgumentOutOfRangeException(nameof(actualContribution));

            ActingUnitId = actingUnitId;
            TargetId = targetId;
            ActualContribution = actualContribution;
        }
    }

    public sealed class FirstPlayableCombatHudState
    {
        public BattlePhase BattlePhase { get; }
        public long BattleRevision { get; }
        public FirstPlayableCombatHudTargetState Target { get; }
        public FirstPlayableRaiseAvailabilityReason RaiseReason { get; }
        public SoulResourceRaiseQuote SoulQuote { get; }
        public IReadOnlyList<FirstPlayableCombatHudFormationSlotState> FormationSlots { get; }
        public FirstPlayableRaiseReceipt LastCommittedRaise { get; }
        public FirstPlayableProofStatus ProofStatus { get; }
        public FirstPlayableContributionReceipt ObservedContribution { get; }

        public bool IsRaiseProcessing => false;

        public FirstPlayableCombatHudState(
            BattlePhase battlePhase,
            long battleRevision,
            FirstPlayableCombatHudTargetState target,
            FirstPlayableRaiseAvailabilityReason raiseReason,
            SoulResourceRaiseQuote soulQuote,
            IReadOnlyList<FirstPlayableCombatHudFormationSlotState> formationSlots,
            FirstPlayableRaiseReceipt lastCommittedRaise,
            FirstPlayableProofStatus proofStatus,
            FirstPlayableContributionReceipt observedContribution)
        {
            if (battleRevision < 0)
                throw new ArgumentOutOfRangeException(nameof(battleRevision));
            if (formationSlots == null)
                throw new ArgumentNullException(nameof(formationSlots));
            if (formationSlots.Count != Formation.Capacity)
                throw new ArgumentException(
                    "HUD formation projection must contain all formation slots.",
                    nameof(formationSlots));
            if (proofStatus == FirstPlayableProofStatus.Observed &&
                observedContribution == null)
            {
                throw new ArgumentException(
                    "Observed proof requires an actual contribution receipt.",
                    nameof(observedContribution));
            }

            BattlePhase = battlePhase;
            BattleRevision = battleRevision;
            Target = target;
            RaiseReason = raiseReason;
            SoulQuote = soulQuote;
            FormationSlots = formationSlots;
            LastCommittedRaise = lastCommittedRaise;
            ProofStatus = proofStatus;
            ObservedContribution = observedContribution;
        }
    }

    public sealed class FirstPlayableCombatHudSession
    {
        internal FirstPlayableRaiseReceipt LastCommittedRaise { get; private set; }
        internal FirstPlayableContributionReceipt ObservedContribution { get; private set; }

        public bool ObserveCommittedRaise(
            RaiseIntoFormationCommand command,
            CommandResult result)
        {
            if (command == null || result == null)
                return false;

            var raised = result.Events
                .OfType<UnitRaised>()
                .SingleOrDefault();
            var assigned = result.Events
                .OfType<UnitAssignedToFormation>()
                .SingleOrDefault();

            if (raised == null ||
                assigned == null ||
                !raised.UnitId.Equals(command.UndeadId) ||
                !assigned.UnitId.Equals(command.UndeadId) ||
                assigned.Slot != command.Slot ||
                !raised.SourceId.Equals(command.Source.SourceId))
            {
                return false;
            }

            LastCommittedRaise = new FirstPlayableRaiseReceipt(
                result.CommandId,
                raised.SourceId,
                raised.UnitId,
                raised.ArchetypeId,
                assigned.Slot);
            ObservedContribution = null;
            return true;
        }

        public bool ObserveContribution(
            EntityId actingUnitId,
            EntityId targetId,
            int actualContribution)
        {
            if (actualContribution <= 0 ||
                LastCommittedRaise == null ||
                !LastCommittedRaise.UnitId.Equals(actingUnitId))
            {
                return false;
            }

            ObservedContribution = new FirstPlayableContributionReceipt(
                actingUnitId,
                targetId,
                actualContribution);
            return true;
        }
    }
}