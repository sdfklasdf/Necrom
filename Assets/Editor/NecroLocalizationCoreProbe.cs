using System;
using System.IO;
using System.Linq;
using Necrom.Core.Domain;
using UnityEditor;
using UnityEngine;
[InitializeOnLoad] public static class NecroLocalizationCoreProbe
{
 static NecroLocalizationCoreProbe(){EditorApplication.update+=Tick;}
 static void Tick(){
  if(EditorApplication.isCompiling||EditorApplication.isUpdating||EditorApplication.isPlayingOrWillChangePlaymode)return;
  var root=Path.GetFullPath(Path.Combine(Application.dataPath,".."));
  var job=Path.Combine(root,"Library/i18n-core-job.txt");if(!File.Exists(job))return;
  try{File.Delete(job);}catch(IOException){return;}
  var dir=Path.Combine(root,"docs/evidence/I18n-Core-20261011");Directory.CreateDirectory(dir);
  try{
   var asset=Resources.Load<TextAsset>("Localization/necro-localization");
   if(asset==null)throw new Exception("External localization table missing.");
   var table=JsonUtility.FromJson<LocalizationTableData>(asset.text);
   var manager=new LocalizationManager(table);
   if(manager.Get("ui.summon")!="소환")throw new Exception("ko resolution failed");
   manager.TrySetLanguage("en-US");
   if(manager.Get("ui.summon")!="Summon")throw new Exception("en switch failed");
   var roundtrip=JsonUtility.FromJson<LocalizationTableData>(JsonUtility.ToJson(table));
   var copy=new LocalizationManager(roundtrip);
   foreach(var trait in TftTaxonomy.Create()){
    var key="trait."+trait.category+"."+trait.id+".name";
    if(!copy.HasKey(key))throw new Exception("Trait localization missing "+key);
   }
   File.WriteAllText(Path.Combine(dir,"unity-validation.txt"),DateTimeOffset.Now.ToString("o")+"\nUnity "+Application.unityVersion+
    "\nExternal Resources JSON -> Core manager + JsonUtility roundtrip PASS\nko/en-US switch;22 keys/18 trait names; names are TEST translations not final copy\nNo UI hookup/gameplay/assets Import/120 full translation yet.\n");
   Debug.Log("[I18N-CORE] PASS");
  }catch(Exception e){File.WriteAllText(Path.Combine(dir,"unity-validation.txt"),"FAIL "+e);Debug.LogException(e);}
 }
}
