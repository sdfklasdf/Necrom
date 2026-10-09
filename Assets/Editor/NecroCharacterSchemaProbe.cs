using System;
using System.IO;
using System.Linq;
using Necrom.Core.Domain;
using Necrom.ContentDraft;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class NecroCharacterSchemaProbe
{
    static NecroCharacterSchemaProbe() { EditorApplication.update += Tick; }
    static void Tick()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || EditorApplication.isUpdating) return;
        string root=Path.GetFullPath(Path.Combine(Application.dataPath,".."));
        string job=Path.Combine(root,"Library/roster-schema-job.txt");
        if(!File.Exists(job))return;
        File.Delete(job);
        string output=Path.Combine(root,"docs/design/character-schema");
        Directory.CreateDirectory(output);
        string result=Path.Combine(output,"unity-validation.txt");
        CharacterCatalogDraftAsset asset=null;
        try
        {
            asset=ScriptableObject.CreateInstance<CharacterCatalogDraftAsset>();
            asset.monsterSource=AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Resources/monster_catalog.json");
            asset.synergySource=AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Resources/synergy_rules_v2.json");
            if(asset.monsterSource==null || asset.synergySource==null)throw new InvalidOperationException("Existing source asset unresolved.");
            var source=JsonUtility.FromJson<MonsterCatalogData>(asset.monsterSource.text);
            asset.contentRevision="DRAFT-20261010-EXISTING-CATALOG";
            asset.traitIds=source.monsters.SelectMany(m=>m.traits).Distinct(StringComparer.Ordinal).OrderBy(v=>v,StringComparer.Ordinal).ToArray();
            asset.affinityIds=source.monsters.Select(m=>m.element).Distinct(StringComparer.Ordinal).OrderBy(v=>v,StringComparer.Ordinal).ToArray();
            asset.profiles=source.monsters.Select(m=>new CharacterContentProfile {characterId=m.id,affinityId=m.element,assetStatus="UNASSIGNED",assetKey="",defense=0,attacksPerSecond=1}).ToArray();
            asset.matchups=Array.Empty<DirectedAffinityRule>(); // No final matchup coefficients supplied.
            var draft=asset.ReadDraft();
            var errors=draft.Validate();
            if(errors.Length>0)throw new InvalidOperationException(string.Join("; ",errors));
            string json=JsonUtility.ToJson(draft,true);
            var restored=JsonUtility.FromJson<CharacterContentDraft>(json);
            var restoredErrors=restored.Validate();
            if(restoredErrors.Length>0 || restored.monsters.monsters.Length!=120 || restored.profiles.Length!=120)throw new InvalidOperationException("Unity JSON roundtrip invalid.");
            if(!restored.monsters.monsters.Select(m=>m.id).SequenceEqual(source.monsters.Select(m=>m.id)))throw new InvalidOperationException("Stable IDs changed.");
            var mutant=JsonUtility.FromJson<CharacterContentDraft>(json);
            mutant.profiles[0].characterId="MISSING";
            if(mutant.Validate().Length==0)throw new InvalidOperationException("Unknown ID accepted.");
            File.WriteAllText(Path.Combine(output,"existing-catalog-draft.json"),json);
            File.WriteAllText(result,"ACTUAL_UNITY_EDITOR "+DateTime.Now.ToString("o")+"\nScriptableObject.ReadDraft PASS\nExisting characters=120 profiles=120 traits="+draft.traitIds.Length+" affinities="+draft.affinityIds.Length+" synergyRules="+draft.synergies.rules.Length+"\nUnity JsonUtility roundtrip PASS\nStable IDs/order preserved PASS\nUnknown profile ID rejected PASS\nAsset mappings=0; import=NOT RUN; matchups=0 UNDECIDED; stats inherited/provisional\n");
            Debug.Log("[ROSTER-SCHEMA] Draft and Unity JSON roundtrip PASS; no asset import/combat hookup.");
        }
        catch(Exception e) {File.WriteAllText(result,"FAIL "+e);Debug.LogException(e);}
        finally {if(asset!=null)UnityEngine.Object.DestroyImmediate(asset);}
    }
}
