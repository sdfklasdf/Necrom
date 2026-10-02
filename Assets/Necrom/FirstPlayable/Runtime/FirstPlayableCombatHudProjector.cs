using System;
using System.Collections.Generic;
using Necrom.Core.Domain;

namespace Necrom.FirstPlayable.Runtime
{
    public sealed class FirstPlayableCombatHudProjector
    {
        private readonly FirstPlayableBattleRuntimeController _battle;
        private readonly EnemySpawnController _enemies;
        private readonly FirstPlayableSoulResourceBridge _soul;
        private readonly Formation _formation;
        private readonly FirstPlayableAlliedRosterController _roster;
        private readonly FirstPlayableCombatHudSession _session;

        public FirstPlayableCombatHudProjector(
            FirstPlayableBattleRuntimeController battle,
            EnemySpawnController enemies,
            FirstPlayableSoulResourceBridge soul,
            Formation formation,
            FirstPlayableAlliedRosterController roster,
            FirstPlayableCombatHudSession session)
        {
            _battle = battle ?? throw new ArgumentNullException(nameof(battle));
            _enemies = enemies ?? throw new ArgumentNullException(nameof(enemies));
            _soul = soul ?? throw new ArgumentNullException(nameof(soul));
            _formation = formation ?? throw new ArgumentNullException(nameof(formation));
            _roster = roster ?? throw new ArgumentNullException(nameof(roster));
            _session = session ?? throw new ArgumentNullException(nameof(session));
        }

        public FirstPlayableCombatHudState Capture()
        {
            var target = BuildTargetState(_enemies.CurrentTarget);
            var raiseReason = ResolveRaiseReason(
                _enemies.CurrentTarget,
                out var quote);
            var slots = BuildFormationSlots();
            var committed = ValidateCommittedRaise();
            var contribution =
                committed != null &&
                _session.ObservedContribution != null &&
                _session.ObservedContribution.ActingUnitId.Equals(committed.UnitId)
                    ? _session.ObservedContribution
                    : null;
            var proofStatus = committed == null
                ? FirstPlayableProofStatus.None
                : contribution == null
                    ? FirstPlayableProofStatus.Pending
                    : FirstPlayableProofStatus.Observed;

            return new FirstPlayableCombatHudState(
                _battle.Phase,
                _battle.Revision,
                target,
                raiseReason,
                quote,
                slots,
                committed,
                proofStatus,
                contribution);
        }

        private FirstPlayableRaiseAvailabilityReason ResolveRaiseReason(
            EnemyRuntimeEntity target,
            out SoulResourceRaiseQuote quote)
        {
            quote = null;
            if (target == null || target.Model == null)
                return FirstPlayableRaiseAvailabilityReason.NoTarget;

            if (target.Model.LifeState != CombatantLifeState.Defeated)
                return FirstPlayableRaiseAvailabilityReason.TargetNotRaiseReady;

            var source = target.RaiseSource;
            if (source == null)
                return FirstPlayableRaiseAvailabilityReason.TargetNotRaiseReady;

            if (source.State != RaiseSourceState.Available ||
                !source.DefeatedEntityId.Equals(target.Model.Id))
            {
                return FirstPlayableRaiseAvailabilityReason.SourceUnavailableOrConsumed;
            }

            quote = _soul.QuoteRaise(source);
            return quote.CanAfford
                ? FirstPlayableRaiseAvailabilityReason.Eligible
                : FirstPlayableRaiseAvailabilityReason.InsufficientSoul;
        }

        private IReadOnlyList<FirstPlayableCombatHudFormationSlotState>
            BuildFormationSlots()
        {
            var slots =
                new FirstPlayableCombatHudFormationSlotState[Formation.Capacity];

            for (var slot = 0; slot < Formation.Capacity; slot++)
            {
                var owned = _formation.GetSlot(slot);
                var runtime = _roster.GetSlot(slot);
                EntityId? runtimeId = null;
                CombatantLifeState? runtimeLifeState = null;

                if (runtime != null && runtime.Model != null)
                {
                    runtimeId = runtime.Model.Id;
                    runtimeLifeState = runtime.Model.LifeState;
                }

                var matches =
                    !owned.HasValue && !runtimeId.HasValue ||
                    owned.HasValue &&
                    runtimeId.HasValue &&
                    owned.Value.Equals(runtimeId.Value) &&
                    runtime != null &&
                    runtime.Slot == slot;

                slots[slot] =
                    new FirstPlayableCombatHudFormationSlotState(
                        slot,
                        owned,
                        runtimeId,
                        runtimeLifeState,
                        matches);
            }

            return Array.AsReadOnly(slots);
        }

        private FirstPlayableRaiseReceipt ValidateCommittedRaise()
        {
            var receipt = _session.LastCommittedRaise;
            if (receipt == null ||
                receipt.Slot < 0 ||
                receipt.Slot >= Formation.Capacity)
            {
                return null;
            }

            var owned = _formation.GetSlot(receipt.Slot);
            if (!owned.HasValue ||
                !owned.Value.Equals(receipt.UnitId))
            {
                return null;
            }

            var runtime = _roster.GetSlot(receipt.Slot);
            if (runtime == null ||
                runtime.Model == null ||
                runtime.Slot != receipt.Slot ||
                !runtime.Model.Id.Equals(receipt.UnitId))
            {
                return null;
            }

            return receipt;
        }

        private static FirstPlayableCombatHudTargetState BuildTargetState(
            EnemyRuntimeEntity target)
        {
            if (target == null || target.Model == null)
                return null;

            return new FirstPlayableCombatHudTargetState(
                target.Model.Id,
                target.Model.ArchetypeId,
                target.RoleId,
                target.Model.Health,
                target.Model.LifeState);
        }
    }
}