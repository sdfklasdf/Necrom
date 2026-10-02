using System;
using System.Collections.Generic;
using Necrom.Core.Domain;
using UnityEngine;

namespace Necrom.FirstPlayable.Runtime
{
    public enum FirstPlayableCombatHudContentKey
    {
        TargetNone,
        TargetActive,
        TargetDefeated,
        RaiseNoTarget,
        RaiseTargetNotReady,
        RaiseSourceUnavailableOrConsumed,
        RaiseInsufficientSoul,
        RaiseEligible,
        RaiseCommittedAwaitingProof,
        RaiseProofObserved,
        ArmyEmpty,
        ArmyOwned,
        ArmyProofPending,
        ArmyProofObserved
    }

    public sealed class FirstPlayableCombatHudTargetSection
    {
        public FirstPlayableCombatHudContentKey ContentKey { get; }
        public BattlePhase BattlePhase { get; }
        public long BattleRevision { get; }
        public FirstPlayableCombatHudTargetState Target { get; }

        public FirstPlayableCombatHudTargetSection(
            FirstPlayableCombatHudContentKey contentKey,
            BattlePhase battlePhase,
            long battleRevision,
            FirstPlayableCombatHudTargetState target)
        {
            if (battleRevision < 0)
                throw new ArgumentOutOfRangeException(nameof(battleRevision));

            ContentKey = contentKey;
            BattlePhase = battlePhase;
            BattleRevision = battleRevision;
            Target = target;
        }
    }

    public sealed class FirstPlayableCombatHudRaiseSection
    {
        public FirstPlayableCombatHudContentKey ContentKey { get; }
        public FirstPlayableRaiseAvailabilityReason RaiseReason { get; }
        public SoulResourceRaiseQuote SoulQuote { get; }
        public FirstPlayableRaiseReceipt LastCommittedRaise { get; }
        public FirstPlayableProofStatus ProofStatus { get; }
        public FirstPlayableContributionReceipt ObservedContribution { get; }
        public bool IsProcessing { get; }

        public FirstPlayableCombatHudRaiseSection(
            FirstPlayableCombatHudContentKey contentKey,
            FirstPlayableRaiseAvailabilityReason raiseReason,
            SoulResourceRaiseQuote soulQuote,
            FirstPlayableRaiseReceipt lastCommittedRaise,
            FirstPlayableProofStatus proofStatus,
            FirstPlayableContributionReceipt observedContribution,
            bool isProcessing)
        {
            ContentKey = contentKey;
            RaiseReason = raiseReason;
            SoulQuote = soulQuote;
            LastCommittedRaise = lastCommittedRaise;
            ProofStatus = proofStatus;
            ObservedContribution = observedContribution;
            IsProcessing = isProcessing;
        }
    }

    public sealed class FirstPlayableCombatHudArmySection
    {
        public FirstPlayableCombatHudContentKey ContentKey { get; }
        public IReadOnlyList<FirstPlayableCombatHudFormationSlotState>
            FormationSlots { get; }
        public FirstPlayableProofStatus ProofStatus { get; }
        public FirstPlayableContributionReceipt ObservedContribution { get; }

        public FirstPlayableCombatHudArmySection(
            FirstPlayableCombatHudContentKey contentKey,
            IReadOnlyList<FirstPlayableCombatHudFormationSlotState> formationSlots,
            FirstPlayableProofStatus proofStatus,
            FirstPlayableContributionReceipt observedContribution)
        {
            FormationSlots = formationSlots
                ?? throw new ArgumentNullException(nameof(formationSlots));
            if (formationSlots.Count != Formation.Capacity)
                throw new ArgumentException(
                    "Army presentation must contain all Formation slots.",
                    nameof(formationSlots));

            ContentKey = contentKey;
            ProofStatus = proofStatus;
            ObservedContribution = observedContribution;
        }
    }

    public sealed class FirstPlayableCombatHudPresentation
    {
        public FirstPlayableCombatHudTargetSection Target { get; }
        public FirstPlayableCombatHudRaiseSection Raise { get; }
        public FirstPlayableCombatHudArmySection Army { get; }

        public FirstPlayableCombatHudPresentation(
            FirstPlayableCombatHudTargetSection target,
            FirstPlayableCombatHudRaiseSection raise,
            FirstPlayableCombatHudArmySection army)
        {
            Target = target ?? throw new ArgumentNullException(nameof(target));
            Raise = raise ?? throw new ArgumentNullException(nameof(raise));
            Army = army ?? throw new ArgumentNullException(nameof(army));
        }
    }

    public interface IFirstPlayableCombatHudStateSource
    {
        FirstPlayableCombatHudState Capture();
    }

    public sealed class FirstPlayableCombatHudStateSourceAdapter
        : IFirstPlayableCombatHudStateSource
    {
        private readonly Func<FirstPlayableCombatHudState> _capture;

        public FirstPlayableCombatHudStateSourceAdapter(
            Func<FirstPlayableCombatHudState> capture)
        {
            _capture = capture ?? throw new ArgumentNullException(nameof(capture));
        }

        public FirstPlayableCombatHudState Capture()
            => _capture()
                ?? throw new InvalidOperationException(
                    "Combat HUD state source returned no state.");
    }

    public interface IFirstPlayableCombatHudView
    {
        void Render(
            FirstPlayableCombatHudPresentation presentation,
            FirstPlayableCombatHudZoneBinding zones);
    }

    public sealed class FirstPlayableCombatHudViewAdapter
        : IFirstPlayableCombatHudView
    {
        private readonly Action<
            FirstPlayableCombatHudPresentation,
            FirstPlayableCombatHudZoneBinding> _render;

        public FirstPlayableCombatHudViewAdapter(
            Action<
                FirstPlayableCombatHudPresentation,
                FirstPlayableCombatHudZoneBinding> render)
        {
            _render = render ?? throw new ArgumentNullException(nameof(render));
        }

        public void Render(
            FirstPlayableCombatHudPresentation presentation,
            FirstPlayableCombatHudZoneBinding zones)
        {
            if (presentation == null)
                throw new ArgumentNullException(nameof(presentation));
            if (zones == null)
                throw new ArgumentNullException(nameof(zones));

            _render(presentation, zones);
        }
    }

    public sealed class FirstPlayableCombatHudZoneBinding
    {
        public RectTransform TargetStatusZone { get; }
        public RectTransform RaiseActionStatusZone { get; }
        public RectTransform ArmyStatusZone { get; }
        public RectTransform ProtectedCombatReadabilityZone { get; }

        private FirstPlayableCombatHudZoneBinding(
            RectTransform targetStatusZone,
            RectTransform raiseActionStatusZone,
            RectTransform armyStatusZone,
            RectTransform protectedCombatReadabilityZone)
        {
            TargetStatusZone = targetStatusZone;
            RaiseActionStatusZone = raiseActionStatusZone;
            ArmyStatusZone = armyStatusZone;
            ProtectedCombatReadabilityZone = protectedCombatReadabilityZone;
        }

        public static FirstPlayableCombatHudZoneBinding Bind(
            RectTransform safeArea)
        {
            if (safeArea == null)
                throw new ArgumentNullException(nameof(safeArea));

            var target = RequireDirectZone(
                safeArea,
                "TargetStatusReadabilityZone");
            var raise = RequireDirectZone(
                safeArea,
                "RaiseActionStatusReadabilityZone");
            var army = RequireDirectZone(
                safeArea,
                "ArmyStatusReadabilityZone");
            var protectedCombat = RequireDirectZone(
                safeArea,
                "ProtectedCombatReadabilityZone");

            return new FirstPlayableCombatHudZoneBinding(
                target,
                raise,
                army,
                protectedCombat);
        }

        private static RectTransform RequireDirectZone(
            RectTransform safeArea,
            string zoneName)
        {
            var zone = safeArea.Find(zoneName) as RectTransform;
            if (zone == null || !ReferenceEquals(zone.parent, safeArea))
            {
                throw new InvalidOperationException(
                    "Required combat HUD semantic zone is missing: " +
                    zoneName);
            }

            return zone;
        }
    }
}