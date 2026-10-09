using System;
using Necrom.Core.Domain;
using UnityEngine;

namespace Necrom.ContentDraft
{
    // Opt-in authoring only. No references from FirstPlayable composition or combat.
    [CreateAssetMenu(menuName="NECRO Draft/Character Catalog")]
    public sealed class CharacterCatalogDraftAsset : ScriptableObject
    {
        public TextAsset monsterSource;
        public TextAsset synergySource;
        public int expectedCharacterCount = 120;
        public string contentRevision = "DRAFT";
        public string balanceStatus = "DRAFT_NOT_APPROVED";
        public TftTraitDefinition[] tftDefinitions = Array.Empty<TftTraitDefinition>();
        public string[] traitIds = Array.Empty<string>();
        public string[] affinityIds = Array.Empty<string>();
        public CharacterContentProfile[] profiles = Array.Empty<CharacterContentProfile>();
        public DirectedAffinityRule[] matchups = Array.Empty<DirectedAffinityRule>();

        public CharacterContentDraft ReadDraft()
        {
            return new CharacterContentDraft {
                expectedCharacterCount=expectedCharacterCount, contentRevision=contentRevision, balanceStatus=balanceStatus,
                monsters=monsterSource==null?null:JsonUtility.FromJson<MonsterCatalogData>(monsterSource.text),
                synergies=synergySource==null?null:JsonUtility.FromJson<SynergyCatalog>(synergySource.text),
                tftDefinitions=tftDefinitions, traitIds=traitIds, affinityIds=affinityIds, profiles=profiles, matchups=matchups
            };
        }
        // Source TextAssets preserve full JSON fields, including SPEC_ONLY special-effect descriptions.
        // ReadDraft produces a validation view; it must never be used to overwrite those source JSON files.
    }
}

