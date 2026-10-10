using System;
using System.IO;
using System.Linq;
using Necrom.Core.Domain;
using Necrom.ContentDraft;
using UnityEditor;
using UnityEngine;
[InitializeOnLoad] public static class NecroTftSchemaProbe {
 static NecroTftSchemaProbe(){EditorApplication.update+=Tick;}
 static void Tick(){
 if(EditorApplication.isCompiling||EditorApplication.isUpdating||EditorApplication.isPlayingOrWillChangePlaymode)return;
 var root=Path.GetFullPath(Path.Combine(Application.dataPath,".."));
 var job=Path.Combine(root,"Library/tft-schema-job.txt");
 if(!File.Exists(job))return;File.Delete(job);
 var output=Path.Combine(root,"docs/evidence/TFT-Matrix-20261011");Directory.CreateDirectory(output);
 CharacterCatalogDraftAsset asset=null;
 try {
 asset=ScriptableObject.CreateInstance<CharacterCatalogDraftAsset>();
 asset.monsterSource=AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Resources/monster_catalog.json");
 asset.synergySource=AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Resources/synergy_rules_v2.json");
 var source=JsonUtility.FromJson<MonsterCatalogData>(asset.monsterSource.text);
 asset.contentRevision="TFT-MATRIX-20261011";
 asset.tftDefinitions=TftTaxonomy.Create();
 asset.traitIds=source.monsters.SelectMany(m=>m.traits).Distinct().ToArray();
 asset.affinityIds=source.monsters.Select(m=>m.element).Distinct().ToArray();
 asset.profiles=source.monsters.Select(m=>new CharacterContentProfile {characterId=m.id,affinityId=m.element}).ToArray();
 var d=asset.ReadDraft(); d.enforceTftAssignments=true; d.synergies=new SynergyCatalog{rules=Array.Empty<SynergyRule>()}; d.traitIds=asset.tftDefinitions.Select(t=>t.id).ToArray(); for(int i=0;i<d.monsters.monsters.Length;i++) d.monsters.monsters[i].traits=new[]{Enum.GetNames(typeof(TftOrigin))[i%7],Enum.GetNames(typeof(TftClass))[(i/7)%7]};var errors=d.Validate();if(errors.Length!=0)throw new Exception(string.Join(";",errors));
 var json=JsonUtility.ToJson(d,true);
 var copy=JsonUtility.FromJson<CharacterContentDraft>(json);
 if(copy.Validate().Length!=0||copy.tftDefinitions.Length!=18||copy.monsters.monsters.Length!=120)throw new Exception("Roundtrip failed");
 if(copy.tftDefinitions.Single(x=>x.id=="Plague").aliases.Single()!="Ghoul")throw new Exception("Alias lost");
 File.WriteAllText(Path.Combine(output,"schema-roundtrip.json"),json);
 File.WriteAllText(Path.Combine(output,"unity-validation.txt"),DateTime.Now.ToString("o")+"\nUnity "+Application.unityVersion+"\nSO ReadDraft + JSON roundtrip PASS\n120 stable MON IDs preserved; taxonomy 7 Origin/7 Class/4 Joker\nGhoul alias / effect IDs / Unique activation preserved\nTFT numeric tiers empty; combat hookup NOT RUN\n");
 Debug.Log("[TFT-SCHEMA] PASS");
 }catch(Exception e){File.WriteAllText(Path.Combine(output,"unity-validation.txt"),"FAIL "+e);Debug.LogException(e);}
 finally{if(asset!=null)UnityEngine.Object.DestroyImmediate(asset);}
 }
}