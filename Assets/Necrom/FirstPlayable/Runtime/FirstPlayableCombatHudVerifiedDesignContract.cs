using System;
using System.Collections.Generic;
using UnityEngine;

namespace Necrom.FirstPlayable.Runtime
{
    public sealed class FirstPlayableCombatHudVerifiedColorToken
    {
        public string FigmaFileKey { get; }
        public string Path { get; }
        public string TokenId { get; }
        public string SourceVariableId { get; }
        public string SourceCollection { get; }
        public Color Value { get; }
        public string SourceVersionEvidence { get; }
        public string RuntimeDestination { get; }

        public FirstPlayableCombatHudVerifiedColorToken(
            string figmaFileKey,
            string path,
            string tokenId,
            string sourceVariableId,
            string sourceCollection,
            Color value,
            string sourceVersionEvidence,
            string runtimeDestination)
        {
            FigmaFileKey = Require(figmaFileKey, nameof(figmaFileKey));
            Path = Require(path, nameof(path));
            TokenId = Require(tokenId, nameof(tokenId));
            SourceVariableId = Require(sourceVariableId, nameof(sourceVariableId));
            SourceCollection = Require(sourceCollection, nameof(sourceCollection));
            Value = value;
            SourceVersionEvidence = Require(sourceVersionEvidence, nameof(sourceVersionEvidence));
            RuntimeDestination = Require(runtimeDestination, nameof(runtimeDestination));
        }

        private static string Require(string value, string parameter)
            => string.IsNullOrWhiteSpace(value)
                ? throw new ArgumentException("Verified token provenance is required.", parameter)
                : value;
    }

    public sealed class FirstPlayableCombatHudVerifiedNumberToken
    {
        public string FigmaFileKey { get; }
        public string Path { get; }
        public string TokenId { get; }
        public string SourceVariableId { get; }
        public string SourceCollection { get; }
        public float Value { get; }
        public string SourceVersionEvidence { get; }
        public string RuntimeDestination { get; }

        public FirstPlayableCombatHudVerifiedNumberToken(
            string figmaFileKey,
            string path,
            string tokenId,
            string sourceVariableId,
            string sourceCollection,
            float value,
            string sourceVersionEvidence,
            string runtimeDestination)
        {
            FigmaFileKey = Require(figmaFileKey, nameof(figmaFileKey));
            Path = Require(path, nameof(path));
            TokenId = Require(tokenId, nameof(tokenId));
            SourceVariableId = Require(sourceVariableId, nameof(sourceVariableId));
            SourceCollection = Require(sourceCollection, nameof(sourceCollection));
            Value = value;
            SourceVersionEvidence = Require(sourceVersionEvidence, nameof(sourceVersionEvidence));
            RuntimeDestination = Require(runtimeDestination, nameof(runtimeDestination));
        }

        private static string Require(string value, string parameter)
            => string.IsNullOrWhiteSpace(value)
                ? throw new ArgumentException("Verified token provenance is required.", parameter)
                : value;
    }

    public sealed class FirstPlayableCombatHudRuntimeTheme
    {
        public FirstPlayableCombatHudVerifiedColorToken PanelBackground { get; }
        public FirstPlayableCombatHudVerifiedColorToken TextOnDark { get; }
        public FirstPlayableCombatHudVerifiedColorToken Danger { get; }
        public FirstPlayableCombatHudVerifiedColorToken SoulAccent { get; }
        public FirstPlayableCombatHudVerifiedNumberToken StateFontSize { get; }
        public FirstPlayableCombatHudVerifiedNumberToken StateLineHeight { get; }
        public FirstPlayableCombatHudVerifiedNumberToken PrimaryFontSize { get; }
        public FirstPlayableCombatHudVerifiedNumberToken PrimaryLineHeight { get; }
        public FirstPlayableCombatHudVerifiedNumberToken SecondaryFontSize { get; }
        public FirstPlayableCombatHudVerifiedNumberToken SecondaryLineHeight { get; }
        public FirstPlayableCombatHudVerifiedNumberToken InnerGap { get; }
        public FirstPlayableCombatHudVerifiedNumberToken PanelPadding { get; }
        public FirstPlayableCombatHudVerifiedNumberToken PrimaryCtaMinHeight { get; }

        public FirstPlayableCombatHudRuntimeTheme(
            FirstPlayableCombatHudVerifiedColorToken panelBackground,
            FirstPlayableCombatHudVerifiedColorToken textOnDark,
            FirstPlayableCombatHudVerifiedColorToken danger,
            FirstPlayableCombatHudVerifiedColorToken soulAccent,
            FirstPlayableCombatHudVerifiedNumberToken stateFontSize,
            FirstPlayableCombatHudVerifiedNumberToken stateLineHeight,
            FirstPlayableCombatHudVerifiedNumberToken primaryFontSize,
            FirstPlayableCombatHudVerifiedNumberToken primaryLineHeight,
            FirstPlayableCombatHudVerifiedNumberToken secondaryFontSize,
            FirstPlayableCombatHudVerifiedNumberToken secondaryLineHeight,
            FirstPlayableCombatHudVerifiedNumberToken innerGap,
            FirstPlayableCombatHudVerifiedNumberToken panelPadding,
            FirstPlayableCombatHudVerifiedNumberToken primaryCtaMinHeight)
        {
            PanelBackground = panelBackground ?? throw new ArgumentNullException(nameof(panelBackground));
            TextOnDark = textOnDark ?? throw new ArgumentNullException(nameof(textOnDark));
            Danger = danger ?? throw new ArgumentNullException(nameof(danger));
            SoulAccent = soulAccent ?? throw new ArgumentNullException(nameof(soulAccent));
            StateFontSize = stateFontSize ?? throw new ArgumentNullException(nameof(stateFontSize));
            StateLineHeight = stateLineHeight ?? throw new ArgumentNullException(nameof(stateLineHeight));
            PrimaryFontSize = primaryFontSize ?? throw new ArgumentNullException(nameof(primaryFontSize));
            PrimaryLineHeight = primaryLineHeight ?? throw new ArgumentNullException(nameof(primaryLineHeight));
            SecondaryFontSize = secondaryFontSize ?? throw new ArgumentNullException(nameof(secondaryFontSize));
            SecondaryLineHeight = secondaryLineHeight ?? throw new ArgumentNullException(nameof(secondaryLineHeight));
            InnerGap = innerGap ?? throw new ArgumentNullException(nameof(innerGap));
            PanelPadding = panelPadding ?? throw new ArgumentNullException(nameof(panelPadding));
            PrimaryCtaMinHeight = primaryCtaMinHeight ?? throw new ArgumentNullException(nameof(primaryCtaMinHeight));
        }
    }

    public sealed class FirstPlayableCombatHudSemanticStateContract
    {
        public FirstPlayableCombatHudContentKey ContentKey { get; }
        public string ComponentSetId { get; }
        public string VariantId { get; }

        public FirstPlayableCombatHudSemanticStateContract(
            FirstPlayableCombatHudContentKey contentKey,
            string componentSetId,
            string variantId)
        {
            ContentKey = contentKey;
            ComponentSetId = string.IsNullOrWhiteSpace(componentSetId)
                ? throw new ArgumentException("Component set id is required.", nameof(componentSetId))
                : componentSetId;
            VariantId = string.IsNullOrWhiteSpace(variantId)
                ? throw new ArgumentException("Variant id is required.", nameof(variantId))
                : variantId;
        }
    }

    public sealed class FirstPlayableCombatHudVerifiedDesignContract
    {
        private const string FileKey = "eXqKU1qHXsn52SJfIGltZo";
        private const string TokenVersion =
            "figma-fresh-read-2026-10-02T16:58:32.5890125Z";

        private readonly IReadOnlyDictionary<
            FirstPlayableCombatHudContentKey,
            FirstPlayableCombatHudSemanticStateContract> _states;

        public string FigmaFileKey => FileKey;
        public string TokenSourceVersionEvidence => TokenVersion;
        public string TokenArtifactSha256 =>
            "4347833166f5d6e97ed7b61957ca0fa5b6ffd99c52dd91be2ba5d3e070b56cbb";
        public string ComponentArtifactSha256 =>
            "b6902979e4cec516b38b851556000c12138389abad562e0727ef1540a9d02ad2";
        public int TokenCount => 40;
        public int SemanticStateCount => _states.Count;
        public FirstPlayableCombatHudRuntimeTheme Theme { get; }

        private FirstPlayableCombatHudVerifiedDesignContract(
            FirstPlayableCombatHudRuntimeTheme theme,
            IReadOnlyDictionary<
                FirstPlayableCombatHudContentKey,
                FirstPlayableCombatHudSemanticStateContract> states)
        {
            Theme = theme ?? throw new ArgumentNullException(nameof(theme));
            _states = states ?? throw new ArgumentNullException(nameof(states));
            if (_states.Count != 14)
                throw new InvalidOperationException("Verified HUD contract must contain all 14 semantic states.");
        }

        public FirstPlayableCombatHudSemanticStateContract GetState(
            FirstPlayableCombatHudContentKey contentKey)
        {
            if (!_states.TryGetValue(contentKey, out var value))
                throw new InvalidOperationException(
                    "No verified Figma state for HUD key: " + contentKey);
            return value;
        }

        public static FirstPlayableCombatHudVerifiedDesignContract Create()
        {
            var theme = new FirstPlayableCombatHudRuntimeTheme(
                ColorToken(
                    "NECRO Color v1/DARK-SURFACE",
                    "e5e44742acda7f52ea367b3bd28a1ce16dda745b",
                    "VariableID:5:11",
                    new Color(0.125490203499794f, 0.14509804546833038f, 0.19607843458652496f, 1f),
                    "Theme.PanelBackground"),
                ColorToken(
                    "NECRO Color v1/TEXT-ON-DARK",
                    "5148ba6d11c77202ad4eec09308af49543973358",
                    "VariableID:5:12",
                    new Color(1f, 1f, 1f, 1f),
                    "Theme.TextOnDark"),
                ColorToken(
                    "NECRO Color v1/DANGER",
                    "9b393983cc62cb0b52bd7907cbac7ebf03a3c925",
                    "VariableID:5:9",
                    new Color(0.7882353067398071f, 0.16470588743686676f, 0.16470588743686676f, 1f),
                    "Theme.Danger"),
                ColorToken(
                    "NECRO Color v1/NECRO-SOUL",
                    "03446bceb3d0a4d3dd86b0531324bd4ebecd586d",
                    "VariableID:5:7",
                    new Color(0.0313725508749485f, 0.49803921580314636f, 0.35686275362968445f, 1f),
                    "Theme.SoulAccent"),
                NumberToken(
                    "NECRO Typography v1/CAPTION/font-size",
                    "8109f19cb008cf50787d232a2063d540eb3e3619",
                    "VariableID:5:26", 12f, "Theme.StateFontSize"),
                NumberToken(
                    "NECRO Typography v1/CAPTION/line-height",
                    "84fbd65df8c286a0dc1d2719c7607bb786c9325a",
                    "VariableID:5:27", 16f, "Theme.StateLineHeight"),
                NumberToken(
                    "NECRO Typography v1/H2/font-size",
                    "ffa6791cafb6db4b4c08b285d41226060a89d963",
                    "VariableID:5:18", 20f, "Theme.PrimaryFontSize"),
                NumberToken(
                    "NECRO Typography v1/H2/line-height",
                    "60ab97349f0c3fb73c306a12991a2bda44548577",
                    "VariableID:5:19", 28f, "Theme.PrimaryLineHeight"),
                NumberToken(
                    "NECRO Typography v1/BODY-SM/font-size",
                    "9afb9a1793d17818f15999c16c9c70714edf70e0",
                    "VariableID:5:22", 14f, "Theme.SecondaryFontSize"),
                NumberToken(
                    "NECRO Typography v1/BODY-SM/line-height",
                    "0281122f371d78fcc01de01cde0e41edd00ecf35",
                    "VariableID:5:23", 20f, "Theme.SecondaryLineHeight"),
                NumberToken(
                    "NECRO Layout v1/S2",
                    "580d9f939f577b22aabc566e225ec0dc14810d98",
                    "VariableID:5:30", 8f, "Theme.InnerGap"),
                NumberToken(
                    "NECRO Layout v1/S4",
                    "740315e26be287bc1d85353a985c344bb4ee7275",
                    "VariableID:5:32", 16f, "Theme.PanelPadding"),
                NumberToken(
                    "NECRO A11Y Interaction v1/primary-cta/min-height",
                    "25fd8d0f5bb1dd9e273a4327ddd610699bfe9784",
                    "VariableID:5:45", 48f, "Theme.PrimaryCtaMinHeight"));

            var states = new Dictionary<
                FirstPlayableCombatHudContentKey,
                FirstPlayableCombatHudSemanticStateContract>
            {
                [FirstPlayableCombatHudContentKey.TargetNone] = State(FirstPlayableCombatHudContentKey.TargetNone, "11:20", "11:2"),
                [FirstPlayableCombatHudContentKey.TargetActive] = State(FirstPlayableCombatHudContentKey.TargetActive, "11:20", "11:8"),
                [FirstPlayableCombatHudContentKey.TargetDefeated] = State(FirstPlayableCombatHudContentKey.TargetDefeated, "11:20", "11:14"),
                [FirstPlayableCombatHudContentKey.RaiseNoTarget] = State(FirstPlayableCombatHudContentKey.RaiseNoTarget, "11:63", "11:21"),
                [FirstPlayableCombatHudContentKey.RaiseTargetNotReady] = State(FirstPlayableCombatHudContentKey.RaiseTargetNotReady, "11:63", "11:27"),
                [FirstPlayableCombatHudContentKey.RaiseSourceUnavailableOrConsumed] = State(FirstPlayableCombatHudContentKey.RaiseSourceUnavailableOrConsumed, "11:63", "11:33"),
                [FirstPlayableCombatHudContentKey.RaiseInsufficientSoul] = State(FirstPlayableCombatHudContentKey.RaiseInsufficientSoul, "11:63", "11:39"),
                [FirstPlayableCombatHudContentKey.RaiseEligible] = State(FirstPlayableCombatHudContentKey.RaiseEligible, "11:63", "11:45"),
                [FirstPlayableCombatHudContentKey.RaiseCommittedAwaitingProof] = State(FirstPlayableCombatHudContentKey.RaiseCommittedAwaitingProof, "11:63", "11:51"),
                [FirstPlayableCombatHudContentKey.RaiseProofObserved] = State(FirstPlayableCombatHudContentKey.RaiseProofObserved, "11:63", "11:57"),
                [FirstPlayableCombatHudContentKey.ArmyEmpty] = State(FirstPlayableCombatHudContentKey.ArmyEmpty, "11:88", "11:64"),
                [FirstPlayableCombatHudContentKey.ArmyOwned] = State(FirstPlayableCombatHudContentKey.ArmyOwned, "11:88", "11:70"),
                [FirstPlayableCombatHudContentKey.ArmyProofPending] = State(FirstPlayableCombatHudContentKey.ArmyProofPending, "11:88", "11:76"),
                [FirstPlayableCombatHudContentKey.ArmyProofObserved] = State(FirstPlayableCombatHudContentKey.ArmyProofObserved, "11:88", "11:82")
            };

            return new FirstPlayableCombatHudVerifiedDesignContract(theme, states);
        }

        private static FirstPlayableCombatHudVerifiedColorToken ColorToken(
            string path,
            string tokenId,
            string sourceVariableId,
            Color value,
            string destination)
            => new FirstPlayableCombatHudVerifiedColorToken(
                FileKey,
                path,
                tokenId,
                sourceVariableId,
                "NECRO Color v1",
                value,
                TokenVersion,
                destination);

        private static FirstPlayableCombatHudVerifiedNumberToken NumberToken(
            string path,
            string tokenId,
            string sourceVariableId,
            float value,
            string destination)
        {
            var slash = path.IndexOf('/');
            var collection = slash > 0 ? path.Substring(0, slash) : path;
            return new FirstPlayableCombatHudVerifiedNumberToken(
                FileKey,
                path,
                tokenId,
                sourceVariableId,
                collection,
                value,
                TokenVersion,
                destination);
        }

        private static FirstPlayableCombatHudSemanticStateContract State(
            FirstPlayableCombatHudContentKey key,
            string componentSetId,
            string variantId)
            => new FirstPlayableCombatHudSemanticStateContract(
                key,
                componentSetId,
                variantId);
    }
}
