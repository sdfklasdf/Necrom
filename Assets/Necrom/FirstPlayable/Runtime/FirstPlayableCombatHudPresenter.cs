using System;
using System.Linq;

namespace Necrom.FirstPlayable.Runtime
{
    public sealed class FirstPlayableCombatHudPresenter
    {
        private readonly IFirstPlayableCombatHudStateSource _stateSource;
        private readonly IFirstPlayableCombatHudView _view;
        private readonly FirstPlayableCombatHudZoneBinding _zones;

        public FirstPlayableCombatHudPresenter(
            IFirstPlayableCombatHudStateSource stateSource,
            IFirstPlayableCombatHudView view,
            FirstPlayableCombatHudZoneBinding zones)
        {
            _stateSource = stateSource
                ?? throw new ArgumentNullException(nameof(stateSource));
            _view = view ?? throw new ArgumentNullException(nameof(view));
            _zones = zones ?? throw new ArgumentNullException(nameof(zones));
        }

        public FirstPlayableCombatHudPresentation Refresh()
        {
            var state = _stateSource.Capture();
            var presentation = BuildPresentation(state);
            _view.Render(presentation, _zones);
            return presentation;
        }

        private static FirstPlayableCombatHudPresentation BuildPresentation(
            FirstPlayableCombatHudState state)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));

            var observed = HasExactObservedProof(state);
            var target = new FirstPlayableCombatHudTargetSection(
                ResolveTargetKey(state),
                state.BattlePhase,
                state.BattleRevision,
                state.Target);

            var raise = new FirstPlayableCombatHudRaiseSection(
                ResolveRaiseKey(state, observed),
                state.RaiseReason,
                state.SoulQuote,
                state.LastCommittedRaise,
                state.ProofStatus,
                observed ? state.ObservedContribution : null,
                state.IsRaiseProcessing);

            var formationSnapshot = Array.AsReadOnly(
                state.FormationSlots.ToArray());
            var army = new FirstPlayableCombatHudArmySection(
                ResolveArmyKey(state, observed),
                formationSnapshot,
                state.ProofStatus,
                observed ? state.ObservedContribution : null);

            return new FirstPlayableCombatHudPresentation(
                target,
                raise,
                army);
        }

        private static FirstPlayableCombatHudContentKey ResolveTargetKey(
            FirstPlayableCombatHudState state)
        {
            if (state.Target == null)
                return FirstPlayableCombatHudContentKey.TargetNone;

            return state.Target.LifeState ==
                Necrom.Core.Domain.CombatantLifeState.Defeated
                ? FirstPlayableCombatHudContentKey.TargetDefeated
                : FirstPlayableCombatHudContentKey.TargetActive;
        }

        private static FirstPlayableCombatHudContentKey ResolveRaiseKey(
            FirstPlayableCombatHudState state,
            bool observed)
        {
            if (observed)
                return FirstPlayableCombatHudContentKey.RaiseProofObserved;

            if (state.LastCommittedRaise != null)
            {
                return FirstPlayableCombatHudContentKey
                    .RaiseCommittedAwaitingProof;
            }

            switch (state.RaiseReason)
            {
                case FirstPlayableRaiseAvailabilityReason.NoTarget:
                    return FirstPlayableCombatHudContentKey.RaiseNoTarget;
                case FirstPlayableRaiseAvailabilityReason.TargetNotRaiseReady:
                    return FirstPlayableCombatHudContentKey.RaiseTargetNotReady;
                case FirstPlayableRaiseAvailabilityReason
                    .SourceUnavailableOrConsumed:
                    return FirstPlayableCombatHudContentKey
                        .RaiseSourceUnavailableOrConsumed;
                case FirstPlayableRaiseAvailabilityReason.InsufficientSoul:
                    return FirstPlayableCombatHudContentKey
                        .RaiseInsufficientSoul;
                case FirstPlayableRaiseAvailabilityReason.Eligible:
                    return FirstPlayableCombatHudContentKey.RaiseEligible;
                default:
                    throw new InvalidOperationException(
                        "Unknown Raise availability reason.");
            }
        }

        private static FirstPlayableCombatHudContentKey ResolveArmyKey(
            FirstPlayableCombatHudState state,
            bool observed)
        {
            if (observed)
                return FirstPlayableCombatHudContentKey.ArmyProofObserved;

            if (state.LastCommittedRaise != null &&
                state.ProofStatus == FirstPlayableProofStatus.Pending)
            {
                return FirstPlayableCombatHudContentKey.ArmyProofPending;
            }

            return state.FormationSlots.Any(slot =>
                    slot != null && slot.OwnedUnitId.HasValue)
                ? FirstPlayableCombatHudContentKey.ArmyOwned
                : FirstPlayableCombatHudContentKey.ArmyEmpty;
        }

        private static bool HasExactObservedProof(
            FirstPlayableCombatHudState state)
        {
            return state.ProofStatus == FirstPlayableProofStatus.Observed &&
                state.LastCommittedRaise != null &&
                state.ObservedContribution != null &&
                state.ObservedContribution.ActingUnitId.Equals(
                    state.LastCommittedRaise.UnitId);
        }
    }
}