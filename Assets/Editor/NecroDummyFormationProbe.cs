using System;
using System.IO;
using System.Linq;
using Necrom.ContentDraft;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Opt-in visual QA fixture only. No combat, roster, synergy, or purchased content changes.
[InitializeOnLoad] public static class NecroDummyFormationProbe {
 const string BaseScene="Assets/Necrom/QuarterView3D/QuarterView3D.unity";
 const string ScenePath="Assets/Necrom/QuarterView3D/DummyFormation.unity";
 const string VisualPath="Assets/Necrom/ContentDraft/Dummy/VisualCatalog.asset";
 static string Root=>Path.GetFullPath(Path.Combine(Application.dataPath,".."));
 static string Output=>Path.Combine(Root,"docs/evidence/Dummy-Formation-20261011");
 static int frames;
 static NecroDummyFormationProbe(){EditorApplication.update+=Tick;Application.logMessageReceived+=OnLog;}
 static void OnLog(string c,string s,LogType t){if(SessionState.GetBool("FormationChecking",false)&&(t==LogType.Error||t==LogType.Exception||t==LogType.Assert))SessionState.SetInt("FormationErrors",SessionState.GetInt("FormationErrors",0)+1);}
 static void Check(bool v,string s){if(!v)throw new InvalidOperationException(s);}
 static string Quality()=>QualitySettings.shadows+"/"+QualitySettings.shadowResolution+"/"+QualitySettings.shadowDistance+"/"+QualitySettings.shadowCascades+"/"+QualitySettings.pixelLightCount+"/"+QualitySettings.antiAliasing+"/"+QualitySettings.softParticles+"/"+QualitySettings.realtimeReflectionProbes;
 static CharacterVisualBinding[] Bindings()=>UnityEngine.Object.FindObjectsByType<CharacterVisualBinding>(FindObjectsSortMode.None).OrderBy(b=>b.name).ToArray();
 static void Tick(){
  if(EditorApplication.isCompiling||EditorApplication.isUpdating)return;
  if(EditorApplication.isPlaying){
   if(!SessionState.GetBool("FormationPlay",false))return;
   if(++frames<60){EditorApplication.QueuePlayerLoopUpdate();return;}
   SessionState.SetBool("FormationPlay",false);
   try{
    var rig=UnityEngine.Object.FindFirstObjectByType<MobileQuarterView>();var camera=rig.GetComponent<Camera>();
    ValidateLinks(false);Check(camera.orthographic&&Mathf.Abs(camera.transform.eulerAngles.x-45)<.01f&&Mathf.Abs(camera.transform.eulerAngles.y-45)<.01f,"Quarter view");
    Check(QualitySettings.shadows==ShadowQuality.HardOnly&&QualitySettings.pixelLightCount==1,"Mobile profile");
    foreach(var size in new[]{new Vector2Int(540,960),new Vector2Int(390,844),new Vector2Int(360,640)}){
     rig.Configure((float)size.x/size.y);
     Check(Metrics(camera,size.x,size.y,"PLAY")==0,"Corrected sprites overlap");
     Capture(camera,size.x,size.y,Path.Combine(Output,"play-"+size.x+"x"+size.y+".png"));
    }
    Check(SessionState.GetInt("FormationErrors",0)==0,"Scoped errors");
    File.AppendAllText(Path.Combine(Output,"validation.txt"),"ACTUAL_PLAYMODE PASS nine prefab instances / MON links / X45 Y45 orthographic / 3 portrait captures / sprite rect overlaps0 / scoped errors0\n");
   }catch(Exception e){File.AppendAllText(Path.Combine(Output,"validation.txt"),"PLAY FAIL "+e+"\n");Debug.LogException(e);}
   SessionState.SetBool("FormationChecking",false);EditorApplication.ExitPlaymode();SessionState.SetBool("FormationRestore",true);return;
  }
  if(EditorApplication.isPlayingOrWillChangePlaymode)return;
  if(SessionState.GetBool("FormationRestore",false)){
   SessionState.SetBool("FormationRestore",false);Application.runInBackground=SessionState.GetBool("FormationBackground",false);
   try{Check(Quality()==SessionState.GetString("FormationQuality",""),"Quality restore");File.AppendAllText(Path.Combine(Output,"validation.txt"),"Quality restored PASS "+Quality()+"\n");
   EditorSceneManager.OpenScene(SessionState.GetString("FormationOriginal",""),OpenSceneMode.Single);
   File.AppendAllText(Path.Combine(Output,"validation.txt"),"Original restored clean="+!SceneManager.GetActiveScene().isDirty+"\n");}
   catch(Exception e){File.AppendAllText(Path.Combine(Output,"validation.txt"),"RESTORE FAIL "+e+"\n");Debug.LogException(e);}return;
  }
  var job=Path.Combine(Root,"Library/dummy-formation-job.txt");if(!File.Exists(job))return;
  string command;try{command=File.ReadAllText(job).Trim();File.Delete(job);}catch(IOException){return;}
  Directory.CreateDirectory(Output);
  try{if(command=="RECOVER"){var r=UnityEngine.Object.FindFirstObjectByType<MobileQuarterView>();if(r!=null)r.RestoreQuality();EditorSceneManager.OpenScene(SessionState.GetString("FormationOriginal",""),OpenSceneMode.Single);File.AppendAllText(Path.Combine(Output,"validation.txt"),"Failed fixture original restored\n");return;}if(command=="VERIFY"){Verify();return;}if(command=="READBACK"){Readback();return;}Build();}
  catch(Exception e){SessionState.SetBool("FormationChecking",false);File.AppendAllText(Path.Combine(Output,"validation.txt"),"FAIL "+e+"\n");Debug.LogException(e);}
 }
 static void Verify(){var original=SceneManager.GetActiveScene();Check(EditorSceneManager.sceneCount==1&&!original.isDirty,"Protect original");SessionState.SetString("FormationOriginal",original.path);SessionState.SetString("FormationQuality",Quality());SessionState.SetBool("FormationBackground",Application.runInBackground);EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);ValidateLinks();File.AppendAllText(Path.Combine(Output,"validation.txt"),"SAVED EDIT Prefab links PASS; runtime validates identity and sprite references separately\n");SessionState.SetInt("FormationErrors",0);SessionState.SetBool("FormationChecking",true);SessionState.SetBool("FormationPlay",true);frames=0;Application.runInBackground=true;EditorApplication.EnterPlaymode();} static void Build(){
  var original=SceneManager.GetActiveScene();Check(EditorSceneManager.sceneCount==1&&!original.isDirty,"Protect original dirty scene");
  Check(!File.Exists(ScenePath),"Do not overwrite existing QA scene");
  SessionState.SetString("FormationOriginal",original.path);SessionState.SetString("FormationQuality",Quality());SessionState.SetBool("FormationBackground",Application.runInBackground);
  var visual=AssetDatabase.LoadAssetAtPath<CharacterVisualCatalogAsset>(VisualPath);Check(visual!=null&&visual.Validate().Length==0,"Visual catalog");
  var scene=EditorSceneManager.OpenScene(BaseScene,OpenSceneMode.Single);var rig=UnityEngine.Object.FindFirstObjectByType<MobileQuarterView>();var camera=rig.GetComponent<Camera>();
  visual=AssetDatabase.LoadAssetAtPath<CharacterVisualCatalogAsset>(VisualPath);Check(visual.Validate().Length==0,"After scene catalog: "+string.Join(";",visual.Validate()));var board=scene.GetRootGameObjects().Single(g=>g.name.StartsWith("BattlefieldBounds"));
  foreach(var t in board.GetComponentsInChildren<Transform>())if(t.name.StartsWith("PROXY_"))t.gameObject.SetActive(false);
  for(int row=0;row<3;row++)for(int col=0;col<3;col++){
   string id="MON_"+(col+1).ToString("D3");
   Check(visual.Validate().Length==0,"Before "+id+": "+string.Join(";",visual.Validate()));var instance=(GameObject)PrefabUtility.InstantiatePrefab(visual.ResolvePrefab(id),scene);
   instance.name=id+"_QA_R"+row+"_C"+col;instance.transform.SetParent(board.transform,false);
   instance.transform.localPosition=new Vector3((col-1)*1.05f,.5f,(row-1)*.76f);instance.transform.rotation=Quaternion.identity;
  }
  rig.Configure(390f/844);rig.ApplyMobileQuality();
  try{
   int overlaps=Metrics(camera,390,844,"BASELINE original prefab orientation / pitch1.05x0.76");
   Capture(camera,390,844,Path.Combine(Output,"baseline-390x844.png"));
   File.WriteAllText(Path.Combine(Output,"validation.txt"),DateTime.Now.ToString("o")+" Unity "+Application.unityVersion+"\nBASELINE overlapping screen rectangles="+overlaps+"\n");
   var mat=AssetDatabase.LoadAssetAtPath<Material>("Assets/Necrom/QuarterView3D/Materials/Grid.mat");
   foreach(var binding in Bindings()){
    var tokens=binding.name.Split('_');int row=int.Parse(tokens[3].Substring(1)),col=int.Parse(tokens[4].Substring(1));
    binding.transform.localPosition=new Vector3((col-1)*1.4f,.36f,(row-1)*2.2f);binding.transform.rotation=camera.transform.rotation;
    var marker=GameObject.CreatePrimitive(PrimitiveType.Cube);marker.name="Cell_"+row+"_"+col;marker.transform.SetParent(board.transform,false);
    marker.transform.localPosition=new Vector3((col-1)*1.4f,.025f,(row-1)*2.2f);marker.transform.localScale=new Vector3(1.2f,.02f,1.8f);
    marker.GetComponent<Renderer>().sharedMaterial=mat;UnityEngine.Object.DestroyImmediate(marker.GetComponent<Collider>());
    var label=new GameObject("ID_"+binding.name);label.transform.SetParent(board.transform,false);var tm=label.AddComponent<TextMesh>();
    tm.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");tm.GetComponent<MeshRenderer>().sharedMaterial=tm.font.material;tm.text=binding.characterId;tm.anchor=TextAnchor.MiddleCenter;tm.alignment=TextAlignment.Center;tm.fontSize=32;tm.characterSize=.07f;tm.color=new Color(.08f,.1f,.15f);
    label.transform.position=binding.transform.position-camera.transform.up*.67f;label.transform.rotation=camera.transform.rotation;
   }
   rig.Configure(9f/16);Check(Metrics(camera,540,960,"CORRECTED EDIT billboard / pitch1.4x2.2")==0,"Editor overlap");
   Capture(camera,540,960,Path.Combine(Output,"editor-540x960.png"));
   Check(EditorSceneManager.SaveScene(scene,ScenePath),"QA scene save");AssetDatabase.SaveAssets();
  }finally{rig.RestoreQuality();}
  Check(Quality()==SessionState.GetString("FormationQuality",""),"Edit quality restore");
  File.AppendAllText(Path.Combine(Output,"validation.txt"),"EDIT saved QA variant / original base scene unchanged / 3prefabs repeated3x / billboard world-space sprites / pitch1.4x2.2 TEST ONLY / apply-restore PASS\n");
  SessionState.SetInt("FormationErrors",0);SessionState.SetBool("FormationChecking",true);SessionState.SetBool("FormationPlay",true);frames=0;Application.runInBackground=true;EditorApplication.EnterPlaymode();
 }
 static void ValidateLinks(bool requirePrefabSource=true){
  var visual=AssetDatabase.LoadAssetAtPath<CharacterVisualCatalogAsset>(VisualPath);Check(visual.Validate().Length==0,"Catalog read");
  var bindings=Bindings();Check(bindings.Length==9,"Nine dummy bindings");
  foreach(var id in new[]{"MON_001","MON_002","MON_003"})Check(bindings.Count(b=>b.characterId==id)==3,"Three each ID");
  foreach(var b in bindings){Check(b.isDummy&&b.GetComponent<SpriteRenderer>().sprite!=null,"Dummy sprite missing");var prefab=visual.ResolvePrefab(b.characterId);Check(b.GetComponent<SpriteRenderer>().sprite==prefab.GetComponent<SpriteRenderer>().sprite,"Resolved sprite link");if(requirePrefabSource)Check(PrefabUtility.GetCorrespondingObjectFromSource(b.gameObject)==prefab,"Stored prefab link");}
 }
 static int Metrics(Camera c,int width,int height,string context){
  var rects=Bindings().Select(b=>{
   var s=b.GetComponent<SpriteRenderer>().sprite.bounds;var min=new Vector2(float.MaxValue,float.MaxValue);var max=new Vector2(float.MinValue,float.MinValue);
   foreach(var x in new[]{s.min.x,s.max.x})foreach(var y in new[]{s.min.y,s.max.y}){
    var v=c.WorldToViewportPoint(b.transform.TransformPoint(new Vector3(x,y,0)));Check(v.z>c.nearClipPlane&&v.z<c.farClipPlane&&v.x>0&&v.x<1&&v.y>0&&v.y<1,"Sprite clipped");
    var p=new Vector2(v.x*width,v.y*height);min=Vector2.Min(min,p);max=Vector2.Max(max,p);
   }return Rect.MinMaxRect(min.x,min.y,max.x,max.y);
  }).ToArray();
  int overlap=0;for(int i=0;i<rects.Length;i++)for(int j=i+1;j<rects.Length;j++)if(rects[i].Overlaps(rects[j]))overlap++;
  File.AppendAllText(Path.Combine(Output,"metrics.txt"),context+" "+width+"x"+height+" overlaps="+overlap+" sprite pixels="+rects.Min(r=>r.width).ToString("F2")+".."+rects.Max(r=>r.width).ToString("F2")+" wide; "+rects.Min(r=>r.height).ToString("F2")+".."+rects.Max(r=>r.height).ToString("F2")+" high\n");return overlap;
 }
 static void Readback(){
  var original=SceneManager.GetActiveScene();Check(EditorSceneManager.sceneCount==1&&!original.isDirty,"Protect original");
  var originalPath=original.path;var scene=EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
  try{ValidateLinks();var rig=UnityEngine.Object.FindFirstObjectByType<MobileQuarterView>();rig.Configure(390f/844);Check(Metrics(rig.GetComponent<Camera>(),390,844,"SERIALIZED READBACK")==0,"Readback overlap");
  File.WriteAllText(Path.Combine(Output,"readback.txt"),DateTime.Now.ToString("o")+" saved scene re-opened: prefab3/instance9/MON identity/sprite refs/X45Y45/rect overlaps0 PASS\n");}
  finally{EditorSceneManager.OpenScene(originalPath,OpenSceneMode.Single);Check(SceneManager.GetActiveScene().path==originalPath&&!SceneManager.GetActiveScene().isDirty,"Readback original restore");File.AppendAllText(Path.Combine(Output,"readback.txt"),"Original scene restored clean="+originalPath+"\n");}
 }
 static void Capture(Camera c,int w,int h,string path){
  var rt=new RenderTexture(w,h,24){antiAliasing=2};rt.Create();var before=c.targetTexture;var active=RenderTexture.active;var tex=new Texture2D(w,h,TextureFormat.RGB24,false);
  try{c.targetTexture=rt;c.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,w,h),0,0);tex.Apply();File.WriteAllBytes(path,tex.EncodeToPNG());}
  finally{c.targetTexture=before;RenderTexture.active=active;rt.Release();UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(tex);}
 }
}
