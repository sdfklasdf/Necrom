using System;
using System.Collections.Generic;
using System.Linq;

namespace Necrom.Core.Domain
{
    // Unconnected authoring draft. Existing runtime catalogs/save IDs remain unchanged.
    [Serializable]
    public sealed class CharacterContentDraft
    {
        public int schemaVersion = 1;
        public int expectedCharacterCount = 120;
        public string contentRevision;
        public string balanceStatus = "DRAFT_NOT_APPROVED";
        public MonsterCatalogData monsters;
        public SynergyCatalog synergies;
        public bool enforceTftAssignments; // Opt-in authoring convention: one Origin, one Class, optional Joker.
        public TftTraitDefinition[] tftDefinitions = Array.Empty<TftTraitDefinition>();
        public string[] traitIds = Array.Empty<string>();
        public string[] affinityIds = Array.Empty<string>();
        public CharacterContentProfile[] profiles = Array.Empty<CharacterContentProfile>();
        public DirectedAffinityRule[] matchups = Array.Empty<DirectedAffinityRule>();

        public string[] Validate()
        {
            var errors = new List<string>();
            if (schemaVersion != 1) errors.Add("Unsupported schemaVersion.");
            if (expectedCharacterCount < 1) errors.Add("expectedCharacterCount must be positive.");
            if (string.IsNullOrWhiteSpace(contentRevision)) errors.Add("contentRevision required.");
            if (string.IsNullOrWhiteSpace(balanceStatus) || !balanceStatus.StartsWith("DRAFT",StringComparison.Ordinal))
                errors.Add("Draft balanceStatus required; this is not a release approval.");
            if (tftDefinitions == null) errors.Add("TFT definitions array required.");
            else if (tftDefinitions.Length > 0) errors.AddRange(TftTaxonomy.Validate(tftDefinitions));
            var traits = Registry(traitIds,"trait",errors);
            if (enforceTftAssignments && (tftDefinitions == null || tftDefinitions.Length == 0)) errors.Add("TFT assignment definitions required.");
            var affinities = Registry(affinityIds,"affinity",errors);
            var units = monsters?.monsters;
            var ids = new HashSet<string>(StringComparer.Ordinal);
            if (units == null || units.Length != expectedCharacterCount) errors.Add("Character count mismatch.");
            foreach (var unit in units ?? Array.Empty<MonsterEntry>())
            {
                if (unit == null) { errors.Add("Null character."); continue; }
                if (string.IsNullOrWhiteSpace(unit.id) || !ids.Add(unit.id)) errors.Add("Missing/duplicate character ID: "+unit.id);
                if (string.IsNullOrWhiteSpace(unit.name)) errors.Add("Character name required: "+unit.id);
                if (unit.hp <= 0 || unit.attack < 0 || unit.gachaWeight <= 0) errors.Add("Invalid base stats/weight: "+unit.id);
                if (!Enum.TryParse<MonsterRarity>(unit.rarity,true,out var rarity) || !Enum.IsDefined(typeof(MonsterRarity),rarity))
                    errors.Add("Invalid rarity: "+unit.id);
                if (unit.traits == null || unit.traits.Length < 1 || unit.traits.Length > 3 ||
                    unit.traits.Distinct(StringComparer.Ordinal).Count() != unit.traits.Length)
                    errors.Add("Expected 1-3 distinct traits: "+unit.id);
                if (enforceTftAssignments) {
                    var definitions=(tftDefinitions ?? Array.Empty<TftTraitDefinition>()).Where(x=>x!=null).ToArray();
                    var assigned=(unit.traits ?? Array.Empty<string>()).Select(id=>definitions.FirstOrDefault(d=>d.id==id)).ToArray();
                    if (assigned.Any(d=>d==null) || assigned.Count(d=>d?.category=="Origin")!=1 ||
                        assigned.Count(d=>d?.category=="Class")!=1 || assigned.Count(d=>d?.category=="Joker")>1)
                        errors.Add("TFT assignment requires canonical Origin + Class + optional Joker: "+unit.id);
                }
                foreach (var trait in unit.traits ?? Array.Empty<string>())
                    if (trait == null || !traits.Contains(trait)) errors.Add("Unknown trait: "+unit.id+"/"+trait);
            }

            if (synergies?.rules == null) errors.Add("Synergy definitions required (empty allowed for draft).");
            else
            {
                try { _ = new SynergyManager(synergies); }
                catch (ArgumentException e) { errors.Add("Invalid synergy tiers: "+e.Message); }
                foreach (var rule in synergies.rules)
                {
                    if (rule == null) continue;
                    if (rule.trait == null || !traits.Contains(rule.trait)) errors.Add("Unknown synergy trait: "+rule.trait);
                    if (rule.thresholds != null && rule.thresholds.Any(v=>v>6))
                        errors.Add("Threshold exceeds existing six-unit permanent-deck contract: "+rule.trait);
                }
            }

            var mapped = new HashSet<string>(StringComparer.Ordinal);
            if (profiles == null || profiles.Length != expectedCharacterCount) errors.Add("Profile count mismatch.");
            foreach (var profile in profiles ?? Array.Empty<CharacterContentProfile>())
            {
                if (profile == null) { errors.Add("Null profile."); continue; }
                if (string.IsNullOrWhiteSpace(profile.characterId) || !ids.Contains(profile.characterId) ||
                    !mapped.Add(profile.characterId)) errors.Add("Missing/unknown/duplicate profile character: "+profile.characterId);
                if (profile.affinityId == null || !affinities.Contains(profile.affinityId)) errors.Add("Unknown profile affinity: "+profile.characterId);
                if (profile.defense < 0 || !Finite(profile.attacksPerSecond) || profile.attacksPerSecond <= 0)
                    errors.Add("Invalid supplemental stats: "+profile.characterId);
                if (profile.assetStatus != "UNASSIGNED" && profile.assetStatus != "MAPPED")
                    errors.Add("Asset status must be UNASSIGNED or MAPPED: "+profile.characterId);
                if (profile.assetStatus == "MAPPED" && string.IsNullOrWhiteSpace(profile.assetKey))
                    errors.Add("Mapped asset key required: "+profile.characterId);
                if (profile.assetStatus == "UNASSIGNED" && !string.IsNullOrWhiteSpace(profile.assetKey))
                    errors.Add("Unassigned asset must have no key: "+profile.characterId);
            }
            if (!ids.SetEquals(mapped)) errors.Add("Profiles must cover each stable character ID exactly once.");
            var pairs = new HashSet<string>(StringComparer.Ordinal);
            if (matchups == null) errors.Add("Matchups array required (empty means no approved table).");
            foreach (var rule in matchups ?? Array.Empty<DirectedAffinityRule>())
            {
                if (rule == null) { errors.Add("Null affinity rule."); continue; }
                if (rule.attackerAffinityId == null || !affinities.Contains(rule.attackerAffinityId) ||
                    rule.defenderAffinityId == null || !affinities.Contains(rule.defenderAffinityId))
                    errors.Add("Unknown directed affinity reference.");
                var pair = rule.attackerAffinityId+"\u001f"+rule.defenderAffinityId;
                if (!pairs.Add(pair)) errors.Add("Duplicate directed affinity pair.");
                if (!Finite(rule.damageMultiplier) || rule.damageMultiplier < 0) errors.Add("Invalid affinity multiplier.");
            }
            return errors.ToArray();
        }
        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
        private static HashSet<string> Registry(string[] source,string kind,List<string> errors)
        {
            var result = new HashSet<string>(StringComparer.Ordinal);
            if (source == null || source.Length == 0) errors.Add(kind+" registry required.");
            foreach (var id in source ?? Array.Empty<string>())
                if (string.IsNullOrWhiteSpace(id) || !result.Add(id)) errors.Add("Missing/duplicate "+kind+" ID.");
            return result;
        }
    }

    [Serializable]
    public sealed class CharacterContentProfile
    {
        public string characterId;
        public string familyId; // Optional visual grouping; no automatic trait or rarity inference.
        public string assetKey;
        public string assetStatus = "UNASSIGNED"; // Mapping is not an import or a rights approval.
        public string affinityId;
        public int defense;
        public float attacksPerSecond = 1;
    }

    [Serializable]
    public sealed class DirectedAffinityRule
    {
        public string attackerAffinityId;
        public string defenderAffinityId;
        public float damageMultiplier;
        // Draft data only; absence is unspecified, never silently treated as a confirmed neutral rule.
    }
}
