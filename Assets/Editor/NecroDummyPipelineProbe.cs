using System;
using System.IO;
using System.Linq;
using Necrom.Core.Domain;
using Necrom.ContentDraft;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
[InitializeOnLoad] public static class NecroDummyPipelineProbe {
 const string Folder="Assets/Necrom/ContentDraft/Dummy";
 static string Root=>Path.GetFullPath(Path.Combine(Application.dataPath,".."));
 static string Output=>Path.Combine(Root,"docs/evidence/TFT-Dummy-20261010");
 static int frames;
 static NecroDummyPipelineProbe(){EditorApplication.update+=Tick;Application.logMessageReceived+=OnLog;}
 static void OnLog(string condition,string stack,LogType type){if(SessionState.GetBool("NecroDummyChecking",false)&&(type==LogType.Error||type==LogType.Exception||type==LogType.Assert))SessionState.SetInt("NecroDummyErrors",SessionState.GetInt("NecroDummyErrors",0)+1);}
 static void Check(bool ok,string message){if(!ok)throw new InvalidOperationException(message);}
 static void Tick(){
 if(EditorApplication.isCompiling||EditorApplication.isUpdating)return;
 if(EditorApplication.isPlaying){
 if(!SessionState.GetBool("NecroDummyPlay",false))return;
 if(++frames<60){EditorApplication.QueuePlayerLoopUpdate();return;}
 SessionState.SetBool("NecroDummyPlay",false);
 try{
 var visual=AssetDatabase.LoadAssetAtPath<CharacterVisualCatalogAsset>(Folder+"/VisualCatalog.asset");
 Check(visual.Validate().Length==0,"Play visual validation");
 var bindings=UnityEngine.Object.FindObjectsByType<CharacterVisualBinding>(FindObjectsSortMode.None);
 Check(bindings.Length==3,"Play scene must contain 3 bindings");
 foreach(var b in bindings)Check(visual.ResolvePrefab(b.characterId)!=null,"Play ID resolve");
 var temp=UnityEngine.Object.Instantiate(visual.ResolvePrefab("MON_003"));
 Check(temp.GetComponent<CharacterVisualBinding>().characterId=="MON_003","Play instantiate identity");
 UnityEngine.Object.DestroyImmediate(temp);
 Render(Camera.main,Path.Combine(Output,"dummy-play.png"));
 Check(SessionState.GetInt("NecroDummyErrors",0)==0,"Scoped Unity error count must be zero");
 File.AppendAllText(Path.Combine(Output,"validation.txt"),"SCOPED_UNITY_ERRORS=0"+Environment.NewLine);
 File.AppendAllText(Path.Combine(Output,"validation.txt"),"ACTUAL_PLAYMODE PASS: 3 bindings, ID resolve, instantiated MON_003, camera render; Unity "+Application.unityVersion+"\n");
 }catch(Exception e){File.AppendAllText(Path.Combine(Output,"validation.txt"),"PLAY FAIL "+e+"\n");Debug.LogException(e);}
 SessionState.SetBool("NecroDummyChecking",false);EditorApplication.ExitPlaymode();SessionState.SetBool("NecroDummyRestore",true);return;
 }
 if(EditorApplication.isPlayingOrWillChangePlaymode)return;
 if(SessionState.GetBool("NecroDummyRestore",false)){
 SessionState.SetBool("NecroDummyRestore",false);Application.runInBackground=SessionState.GetBool("NecroDummyBackground",false);
 var path=SessionState.GetString("NecroDummyOriginal","");
 if(!string.IsNullOrEmpty(path))EditorSceneManager.OpenScene(path,OpenSceneMode.Single);
 return;
 }
 var job=Path.Combine(Root,"Library/tft-dummy-job.txt");
 if(!File.Exists(job))return;var command=File.ReadAllText(job).Trim();File.Delete(job);Directory.CreateDirectory(Output);
 if(command=="READBACK"){Readback();return;}
 SessionState.SetInt("NecroDummyErrors",0);SessionState.SetBool("NecroDummyChecking",true);
 try{Run();}catch(Exception e){File.AppendAllText(Path.Combine(Output,"validation.txt"),"FAIL "+e+"\n");Debug.LogException(e);}
 }
  static void Readback(){
 try {
 var visual=AssetDatabase.LoadAssetAtPath<CharacterVisualCatalogAsset>(Folder+"/VisualCatalog.asset");
 Check(visual!=null && visual.Validate().Length==0,"Fresh domain serialized catalog readback");
 foreach(var entry in visual.entries) {
 var prefab=visual.ResolvePrefab(entry.characterId);
 Check(PrefabUtility.IsPartOfPrefabAsset(prefab),"Not a stored prefab");
 Check(prefab.GetComponent<SpriteRenderer>().sprite!=null,"Serialized Sprite unresolved");
 }
 var scene=SceneManager.GetActiveScene();
 File.WriteAllText(Path.Combine(Output,"fresh-readback.txt"),DateTime.Now.ToString("o")+"\nFresh Unity domain saved SO/prefab/Sprite links PASS\n"+string.Join("\n",visual.entries.Select(e=>e.characterId+" -> "+e.assetKey+" -> "+AssetDatabase.GetAssetPath(e.prefab)+" -> "+AssetDatabase.GetAssetPath(e.prefab.GetComponent<SpriteRenderer>().sprite)))+"\nRestored scene="+scene.path+" dirty="+scene.isDirty+" playing="+EditorApplication.isPlaying+"\n");
 }catch(Exception e){File.WriteAllText(Path.Combine(Output,"fresh-readback.txt"),"FAIL "+e);Debug.LogException(e);}
 }
 static void Run(){
 var original=SceneManager.GetActiveScene();
 Check(EditorSceneManager.sceneCount==1&&!original.isDirty,"Requires one clean original scene; never discard user scene edits");
 var originalPath=original.path;
 Directory.CreateDirectory(Folder);
 var source=AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Resources/monster_catalog.json");
 var all=JsonUtility.FromJson<MonsterCatalogData>(source.text);
 var units=all.monsters.Take(3).Select(m=>JsonUtility.FromJson<MonsterEntry>(JsonUtility.ToJson(m))).ToArray();
 var traits=new[]{new[]{"Skeleton","Guardian","Reaper"},new[]{"Plague","Warlock","Mimic"},new[]{"Dragon","Ranger","Vampire"}};
 for(int i=0;i<3;i++)units[i].traits=traits[i];
 File.WriteAllText(Folder+"/dummy-monsters.json",JsonUtility.ToJson(new MonsterCatalogData{schemaVersion=3,balanceStatus="DRAFT_DUMMY",monsters=units},true));
 File.WriteAllText(Folder+"/dummy-synergies.json",JsonUtility.ToJson(new SynergyCatalog{schemaVersion=2,rules=Array.Empty<SynergyRule>()},true));
 AssetDatabase.Refresh();
 var catalog=AssetDatabase.LoadAssetAtPath<CharacterCatalogDraftAsset>(Folder+"/Catalog.asset");
 if(catalog==null){catalog=ScriptableObject.CreateInstance<CharacterCatalogDraftAsset>();AssetDatabase.CreateAsset(catalog,Folder+"/Catalog.asset");}
 catalog.expectedCharacterCount=3;catalog.contentRevision="DUMMY-3-NOT-PRODUCTION";catalog.balanceStatus="DRAFT_DUMMY";
 catalog.enforceTftAssignments=true;catalog.tftDefinitions=TftTaxonomy.Create();catalog.traitIds=catalog.tftDefinitions.Select(d=>d.id).ToArray();
 catalog.monsterSource=AssetDatabase.LoadAssetAtPath<TextAsset>(Folder+"/dummy-monsters.json");catalog.synergySource=AssetDatabase.LoadAssetAtPath<TextAsset>(Folder+"/dummy-synergies.json");
 catalog.affinityIds=units.Select(m=>m.element).Distinct().ToArray();
 catalog.profiles=units.Select(m=>new CharacterContentProfile{characterId=m.id,affinityId=m.element,assetStatus="MAPPED",assetKey="dummy/"+m.id}).ToArray();
 EditorUtility.SetDirty(catalog);
 var visual=AssetDatabase.LoadAssetAtPath<CharacterVisualCatalogAsset>(Folder+"/VisualCatalog.asset");
 if(visual==null){visual=ScriptableObject.CreateInstance<CharacterVisualCatalogAsset>();AssetDatabase.CreateAsset(visual,Folder+"/VisualCatalog.asset");}
 visual.catalog=catalog;visual.entries=new CharacterVisualEntry[3];
 var colors=new[]{new Color(.3f,.8f,1f),new Color(.6f,1f,.3f),new Color(1f,.4f,.4f)};
 for(int i=0;i<3;i++){
 var id=units[i].id;var tex=new Texture2D(64,64,TextureFormat.RGBA32,false);
 for(int y=0;y<64;y++)for(int x=0;x<64;x++)tex.SetPixel(x,y,(x<4||y<4||x>59||y>59)?new Color(.08f,.1f,.15f):colors[i]);
 tex.Apply();var imagePath=Folder+"/"+id+".png";File.WriteAllBytes(imagePath,tex.EncodeToPNG());UnityEngine.Object.DestroyImmediate(tex);
 AssetDatabase.ImportAsset(imagePath);
 var importer=(TextureImporter)AssetImporter.GetAtPath(imagePath);importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.spritePixelsPerUnit=64;importer.filterMode=FilterMode.Point;importer.SaveAndReimport();
 var go=new GameObject("DUMMY_"+id);go.AddComponent<SpriteRenderer>().sprite=AssetDatabase.LoadAssetAtPath<Sprite>(imagePath);go.AddComponent<CharacterVisualBinding>().characterId=id;
 var prefab=PrefabUtility.SaveAsPrefabAsset(go,Folder+"/"+id+".prefab");UnityEngine.Object.DestroyImmediate(go);
 visual.entries[i]=new CharacterVisualEntry{characterId=id,assetKey="dummy/"+id,prefab=prefab};
 }
 EditorUtility.SetDirty(visual);AssetDatabase.SaveAssets();
 var loaded=AssetDatabase.LoadAssetAtPath<CharacterVisualCatalogAsset>(Folder+"/VisualCatalog.asset");
 Check(loaded.Validate().Length==0,string.Join(";",loaded.Validate()));
 var round=JsonUtility.FromJson<CharacterContentDraft>(JsonUtility.ToJson(catalog.ReadDraft()));
 Check(round.Validate().Length==0&&round.enforceTftAssignments,"JSON roundtrip");
 var key=loaded.entries[0].assetKey;loaded.entries[0].assetKey="broken";Check(loaded.Validate().Length>0,"Bad key not rejected");loaded.entries[0].assetKey=key;
 var idOld=loaded.entries[1].characterId;loaded.entries[1].characterId=loaded.entries[0].characterId;Check(loaded.Validate().Length>0,"Duplicate ID not rejected");loaded.entries[1].characterId=idOld;
 var prefabOld=loaded.entries[2].prefab;loaded.entries[2].prefab=null;Check(loaded.Validate().Length>0,"Missing prefab not rejected");loaded.entries[2].prefab=prefabOld;
 bool rejected=false;try{loaded.ResolvePrefab("MON_999");}catch(InvalidOperationException){rejected=true;}Check(rejected,"Unknown resolve not rejected");
 var bad=ScriptableObject.CreateInstance<CharacterCatalogDraftAsset>();bad.monsterSource=catalog.monsterSource;bad.synergySource=catalog.synergySource;bad.profiles=catalog.profiles;bad.expectedCharacterCount=3;bad.traitIds=catalog.traitIds;bad.affinityIds=catalog.affinityIds;bad.contentRevision="DRAFT";bad.enforceTftAssignments=true;Check(bad.ReadDraft().Validate().Length>0,"Missing TFT definitions not rejected");UnityEngine.Object.DestroyImmediate(bad);
 var preview=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Additive);
 SceneManager.SetActiveScene(preview);
 for(int i=0;i<3;i++){
 var instance=(GameObject)PrefabUtility.InstantiatePrefab(loaded.ResolvePrefab(units[i].id),preview);instance.transform.position=new Vector3((i-1)*2.8f,0,0);
 Check(instance.GetComponent<CharacterVisualBinding>().characterId==units[i].id,"Instantiated identity mismatch");
 var text=new GameObject("Label_"+units[i].id);var tm=text.AddComponent<TextMesh>();tm.text=units[i].id+"\n"+string.Join("\n",traits[i])+"\nDUMMY";tm.anchor=TextAnchor.UpperCenter;tm.alignment=TextAlignment.Center;tm.characterSize=.09f;tm.fontSize=32;tm.color=Color.white;tm.transform.position=new Vector3((i-1)*2.8f,-.8f,0);
 }
 var cameraGo=new GameObject("DummyPreviewCamera");cameraGo.tag="MainCamera";var camera=cameraGo.AddComponent<Camera>();camera.orthographic=true;camera.orthographicSize=2.8f;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.035f,.045f,.065f);camera.transform.position=new Vector3(0,0,-10);
 Render(camera,Path.Combine(Output,"dummy-editor.png"));
 Check(EditorSceneManager.SaveScene(preview,Folder+"/DummyPipeline.unity"),"Dummy scene save");
 EditorSceneManager.CloseScene(preview,true);SceneManager.SetActiveScene(original);
 File.WriteAllText(Path.Combine(Output,"validation.txt"),DateTime.Now.ToString("o")+"\nUnity "+Application.unityVersion+"\nSO + imported sprites + saved prefabs + serialized links + JSON roundtrip PASS\nMON_001 Skeleton/Guardian/Reaper\nMON_002 Plague/Warlock/Mimic\nMON_003 Dragon/Ranger/Vampire\nUnknown ID / duplicate ID / wrong assetKey / missing prefab / missing TFT definitions rejected PASS\nThree prefab instances and camera rendering PASS\nReal purchased assets=0; combat hookup=0; numeric tiers=0 undecided\n");
 File.WriteAllText(Path.Combine(Output,"dummy-roundtrip.json"),JsonUtility.ToJson(round,true));
 SessionState.SetString("NecroDummyOriginal",originalPath);SessionState.SetBool("NecroDummyPlay",true);
 EditorSceneManager.OpenScene(Folder+"/DummyPipeline.unity",OpenSceneMode.Single);
 SessionState.SetBool("NecroDummyBackground",Application.runInBackground);Application.runInBackground=true;EditorApplication.EnterPlaymode();
 }
 static void Render(Camera camera,string path){
 var target=new RenderTexture(1200,675,24);target.Create();var oldTarget=camera.targetTexture;var oldActive=RenderTexture.active;
 var texture=new Texture2D(1200,675,TextureFormat.RGB24,false);
 try{camera.targetTexture=target;camera.Render();RenderTexture.active=target;texture.ReadPixels(new Rect(0,0,1200,675),0,0);texture.Apply();File.WriteAllBytes(path,texture.EncodeToPNG());}
 finally{camera.targetTexture=oldTarget;RenderTexture.active=oldActive;target.Release();UnityEngine.Object.DestroyImmediate(target);UnityEngine.Object.DestroyImmediate(texture);}
 }
}
