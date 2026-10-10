using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Necrom.ContentDraft;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Opt-in purchased-content visual fixture. No roster, combat or production catalog writes.
[InitializeOnLoad] public static class NecroPolygonScoutProbe {
 const string ScenePath="Assets/Necrom/QuarterView3D/DummyFormation.unity";
 const string Vendor="Assets/Synty/PolygonFantasyCharacters/";
 static readonly string[] Names={"SM_Chr_Male_Rouge_01","SM_Chr_Female_Witch_01","SM_Chr_Male_Wizard_01"};
 static string Root=>Path.GetFullPath(Path.Combine(Application.dataPath,".."));
 static string Output=>Path.Combine(Root,"docs/evidence/Polygon-Scout-20261011");
 static string Job=>Path.Combine(Root,"Library/polygon-scout-job.txt");
 static double next;
 static int frames;
 static NecroPolygonScoutProbe(){EditorApplication.update+=Tick;Application.logMessageReceived+=OnLog;}
 static void OnLog(string c,string s,LogType t){
  if(SessionState.GetBool("Polygon.Checking",false)&&(t==LogType.Error||t==LogType.Exception||t==LogType.Assert)){
   SessionState.SetInt("Polygon.Errors",SessionState.GetInt("Polygon.Errors",0)+1);
   File.AppendAllText(Path.Combine(Output,"runtime-errors.txt"),t+": "+c+"\n"+s+"\n");
  }
 }
 static void Check(bool value,string message){if(!value)throw new InvalidOperationException(message);}
 static CharacterVisualBinding[] Bindings()=>UnityEngine.Object.FindObjectsByType<CharacterVisualBinding>(FindObjectsSortMode.None).OrderBy(x=>x.name).ToArray();
 static string Quality()=>QualitySettings.shadows+"/"+QualitySettings.shadowResolution+"/"+QualitySettings.shadowDistance+"/"+QualitySettings.shadowCascades+"/"+QualitySettings.pixelLightCount+"/"+QualitySettings.antiAliasing;
 static void Log(string file,string line)=>File.AppendAllText(Path.Combine(Output,file),line+"\n");
 static void Tick(){
  if(EditorApplication.isCompiling||EditorApplication.isUpdating)return;
  if(EditorApplication.isPlaying){
   if(!SessionState.GetBool("Polygon.Play",false))return;
   if(EditorApplication.timeSinceStartup<next)return;
   if(++frames<40){EditorApplication.QueuePlayerLoopUpdate();return;}
   SessionState.SetBool("Polygon.Play",false);
   try{
    Check(Screen.width==390&&Screen.height==844,"Actual GameView size "+Screen.width+"x"+Screen.height);
    Validate(false);
    var camera=UnityEngine.Object.FindFirstObjectByType<MobileQuarterView>().GetComponent<Camera>();
    Check(camera.orthographic&&Mathf.Abs(camera.transform.eulerAngles.x-45)<.01f&&Mathf.Abs(camera.transform.eulerAngles.y-45)<.01f,"Camera X45/Y45");
    Check(Metrics(camera,390,844,"NATIVE PLAY")==0,"3D projected bounds overlap");
    Capture(camera,390,844,Path.Combine(Output,"play-390x844.png"));
    Capture(camera,540,960,Path.Combine(Output,"play-540x960.png"));
    Check(SessionState.GetInt("Polygon.Errors",0)==0,"Scoped runtime errors");
    Log("play-validation.txt",DateTimeOffset.Now.ToString("o")+" Native PlayMode Unity "+Application.unityVersion+"; 3 real models / 9 instances / IDs / supported materials / X45Y45 / unclipped / overlaps0 / scoped errors0 PASS");
    AnimationReport();
   }catch(Exception e){Log("play-validation.txt","FAIL "+e);Debug.LogException(e);}
   SessionState.SetBool("Polygon.Checking",false);SessionState.SetBool("Polygon.Restore",true);
   EditorApplication.ExitPlaymode();return;
  }
  if(EditorApplication.isPlayingOrWillChangePlaymode)return;
  if(SessionState.GetBool("Polygon.Restore",false)){
   SessionState.SetBool("Polygon.Restore",false);
   Application.runInBackground=SessionState.GetBool("Polygon.Background",false);
   Check(Quality()==SessionState.GetString("Polygon.Quality",""),"Quality not restored");
   EditorSceneManager.OpenScene(SessionState.GetString("Polygon.Original",""),OpenSceneMode.Single);
   Log("play-validation.txt","Original clean scene restored="+SceneManager.GetActiveScene().path+"; quality restored="+Quality());return;
  }
  if(!File.Exists(Job))return;
  string command;try{command=File.ReadAllText(Job).Trim();File.Delete(Job);}catch(IOException){return;}
  Directory.CreateDirectory(Output);
  try{if(command=="BUILD")Build();else if(command=="AUDIT")Audit();else if(command=="REFIT")Refit();else if(command=="PLAY")Play();else if(command=="READBACK")Readback();else throw new Exception("Unknown command");}
  catch(Exception e){Log("failure.txt",DateTimeOffset.Now.ToString("o")+" "+command+" FAIL "+e);Debug.LogException(e);}
 }
 static string Original(){
  var s=SceneManager.GetActiveScene();
  Check(EditorSceneManager.sceneCount==1&&!s.isDirty&&!string.IsNullOrEmpty(s.path),"Protect active dirty/unsaved scene");
  return s.path;
 }
 static void ImportReport(){
  var paths=AssetDatabase.FindAssets("t:GameObject",new[]{Vendor+"Prefabs"}).Select(AssetDatabase.GUIDToAssetPath).ToArray();
  var clips=AssetDatabase.FindAssets("t:AnimationClip",new[]{"Assets/Synty"}).Select(AssetDatabase.GUIDToAssetPath).Distinct().ToArray();
  var embedded=AssetDatabase.LoadAllAssetsAtPath(Vendor+"Models/Characters.fbx").OfType<AnimationClip>().ToArray();
  Log("import.txt",DateTimeOffset.Now.ToString("o")+" Unity "+Application.unityVersion+"; native AssetDatabase prefab paths="+paths.Length+"; vendor clip asset paths="+clips.Length+"; Characters.fbx embedded clips="+embedded.Length+"; render pipeline="+(UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline==null?"Built-in":UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline.name));
  foreach(var name in Names){
   var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(Vendor+"Prefabs/"+name+".prefab");Check(prefab!=null,"Imported prefab missing "+name);
   Check(prefab.GetComponentsInChildren<SkinnedMeshRenderer>(true).Any(r=>r.sharedMesh!=null),"Real skinned mesh missing "+name);
   foreach(var a in prefab.GetComponentsInChildren<Animator>(true))Log("import.txt",name+" avatar="+(a.avatar==null?"NULL":a.avatar.name+" valid="+a.avatar.isValid+" human="+a.avatar.isHuman)+" controller="+(a.runtimeAnimatorController==null?"NULL":a.runtimeAnimatorController.name));
   foreach(var mat in prefab.GetComponentsInChildren<Renderer>(true).SelectMany(r=>r.sharedMaterials).Where(m=>m!=null).Distinct())Log("import.txt",name+" material="+mat.name+" shader="+(mat.shader==null?"NULL":mat.shader.name+" supported="+mat.shader.isSupported)+" source="+AssetDatabase.GetAssetPath(mat));
  }
 }
 static void Build(){
  var original=Original();ImportReport();
  var scene=EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
  try{
   var old=Bindings();Check(old.Length==9&&old.All(b=>b.isDummy&&b.GetComponent<SpriteRenderer>()!=null),"Expected original nine dummy sprites");
   var camera=UnityEngine.Object.FindFirstObjectByType<MobileQuarterView>().GetComponent<Camera>();
   foreach(var b in old){
    int type=int.Parse(b.characterId.Substring(4))-1;Check(type>=0&&type<3,"Only MON001-003");
    string id=b.characterId,label=b.name;var parent=b.transform.parent;var position=b.transform.localPosition;
    var source=AssetDatabase.LoadAssetAtPath<GameObject>(Vendor+"Prefabs/"+Names[type]+".prefab");
    var real=(GameObject)PrefabUtility.InstantiatePrefab(source,scene);real.name=label;real.transform.SetParent(parent,false);
    real.transform.localPosition=new Vector3(position.x,.04f,position.z);real.transform.rotation=Quaternion.Euler(0,225,0);
    var bounds=BoundsOf(real);Check(bounds.size.y>0,"Model height");
    float scale=1.35f/bounds.size.y;real.transform.localScale=Vector3.one*scale;
    bounds=BoundsOf(real);real.transform.position+=Vector3.up*(.04f-bounds.min.y);
    var binding=real.AddComponent<CharacterVisualBinding>();binding.characterId=id;binding.isDummy=false;
    Log("mapping.txt",id+" -> "+Names[type]+"; instance="+label+" scale="+scale.ToString("F4")+" worldHeight="+BoundsOf(real).size.y.ToString("F4")+" yaw225/upright; TEST mapping only");
    UnityEngine.Object.DestroyImmediate(b.gameObject);
   }
   Validate(true);
   var rig=camera.GetComponent<MobileQuarterView>();rig.Configure(390f/844);
   Check(Metrics(camera,390,844,"SAVED EDIT")==0,"3D bounds overlap");
   Capture(camera,390,844,Path.Combine(Output,"editor-390x844.png"));
   Check(EditorSceneManager.SaveScene(scene),"Scene save");
   Log("placement.txt",DateTimeOffset.Now.ToString("o")+" AWU1 scene saved; three vendor prefab sources each repeated3; MON IDs+isDummyfalse; zero dummy sprites; normalized1.35height; original source prefabs/catalog/combat unchanged PASS");
  }finally{EditorSceneManager.OpenScene(original,OpenSceneMode.Single);}
  Readback();
 }
 static Vector3[] GeometryPoints(GameObject root){
  var points=new System.Collections.Generic.List<Vector3>();
  foreach(var renderer in root.GetComponentsInChildren<Renderer>().Where(r=>r.enabled&&r.gameObject.activeInHierarchy&&!(r is SpriteRenderer))){
   Mesh mesh=null;bool temporary=false;
   if(renderer is SkinnedMeshRenderer skin){mesh=new Mesh();skin.BakeMesh(mesh,true);temporary=true;}
   else{var filter=renderer.GetComponent<MeshFilter>();if(filter!=null)mesh=filter.sharedMesh;}
   if(mesh!=null)foreach(var vertex in mesh.vertices)points.Add(renderer.transform.TransformPoint(vertex));
   if(temporary)UnityEngine.Object.DestroyImmediate(mesh);
  }
  Check(points.Count>0,"No real mesh geometry "+root.name);return points.ToArray();
 }
 static Bounds BoundsOf(GameObject root){
  var points=GeometryPoints(root);var bounds=new Bounds(points[0],Vector3.zero);
  foreach(var p in points.Skip(1))bounds.Encapsulate(p);return bounds;
 }
 static void Validate(bool prefabSource){
  var bindings=Bindings();Check(bindings.Length==9,"Nine bindings");
  foreach(var id in new[]{"MON_001","MON_002","MON_003"})Check(bindings.Count(b=>b.characterId==id)==3,"Three each "+id);
  foreach(var b in bindings){
   Check(!b.isDummy&&b.GetComponentsInChildren<SpriteRenderer>(true).Length==0,"No dummy visual");
   Check(b.GetComponentsInChildren<SkinnedMeshRenderer>().Any(r=>r.sharedMesh!=null&&r.bones.Length>0),"Skinned mesh/bones missing");
   Check(Mathf.Abs(b.transform.up.y-1)<.001f,"Model not upright"); Check(Mathf.Abs(BoundsOf(b.gameObject).size.y-1.35f)<.001f,"Actual mesh height not normalized");
   foreach(var r in b.GetComponentsInChildren<Renderer>().Where(x=>x.enabled&&x.gameObject.activeInHierarchy))
    foreach(var m in r.sharedMaterials)Check(m!=null&&m.shader!=null&&m.shader.isSupported&&m.shader.name!="Hidden/InternalErrorShader","Unsupported material "+b.name);
   if(prefabSource){int type=int.Parse(b.characterId.Substring(4))-1;Check(PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(b.gameObject)==Vendor+"Prefabs/"+Names[type]+".prefab","Wrong vendor prefab link");}
  }
 }
 static int Metrics(Camera camera,int width,int height,string context){
  camera.GetComponent<MobileQuarterView>().Configure((float)width/height);
  var rects=Bindings().Select(binding=>{
   var points=GeometryPoints(binding.gameObject);var min=new Vector2(float.MaxValue,float.MaxValue);var max=new Vector2(float.MinValue,float.MinValue);
   foreach(var vertex in points){
    var v=camera.WorldToViewportPoint(vertex);Check(v.z>camera.nearClipPlane&&v.z<camera.farClipPlane&&v.x>0&&v.x<1&&v.y>0&&v.y<1,"Model clipped "+binding.name);
    var p=new Vector2(v.x*width,v.y*height);min=Vector2.Min(min,p);max=Vector2.Max(max,p);
   }return Rect.MinMaxRect(min.x,min.y,max.x,max.y);
  }).ToArray();
  int overlap=0;for(int i=0;i<rects.Length;i++)for(int j=i+1;j<rects.Length;j++)if(rects[i].Overlaps(rects[j]))overlap++;
  Log("metrics.txt",context+" "+width+"x"+height+" projected baked mesh geometry rect overlaps="+overlap+" widths="+rects.Min(r=>r.width).ToString("F2")+".."+rects.Max(r=>r.width).ToString("F2")+" heights="+rects.Min(r=>r.height).ToString("F2")+".."+rects.Max(r=>r.height).ToString("F2"));return overlap;
 }
 static void Refit(){
  string original=Original();EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
  try{
   foreach(var b in Bindings()){
    int type=int.Parse(b.characterId.Substring(4))-1;var pos=b.transform.localPosition;pos.x=(type-1)*1.75f;b.transform.localPosition=pos;
    b.transform.rotation=Quaternion.Euler(0,225,0);
    var height=BoundsOf(b.gameObject).size.y;b.transform.localScale*=1.35f/height;
    var bounds=BoundsOf(b.gameObject);b.transform.position+=Vector3.up*(.04f-bounds.min.y);
    var mat=AssetDatabase.LoadAssetAtPath<Material>(Vendor+"Materials/PolygonFantasyCharacters_01_"+(char)('A'+type)+".mat");Check(mat!=null,"Variant material");
    foreach(var renderer in b.GetComponentsInChildren<Renderer>(true)){
     var mats=renderer.sharedMaterials;
     for(int i=0;i<mats.Length;i++)if(mats[i]!=null&&mats[i].name.StartsWith("PolygonFantasyCharacters_01_"))mats[i]=mat;
     renderer.sharedMaterials=mats;PrefabUtility.RecordPrefabInstancePropertyModifications(renderer);
    }
    PrefabUtility.RecordPrefabInstancePropertyModifications(b.transform);
    Check(Mathf.Abs(BoundsOf(b.gameObject).size.y-1.35f)<.001f,"Normalized geometry height");
    Log("mapping-final.txt",b.characterId+" "+b.name+" source="+Names[type]+" yaw225 scale="+b.transform.localScale.x.ToString("F4")+" bakedHeight="+BoundsOf(b.gameObject).size.y.ToString("F4")+" material="+mat.name+"; TEST mapping");
   }
   foreach(var t in Bindings()[0].transform.parent.GetComponentsInChildren<Transform>()){
    if(t.name.StartsWith("Cell_")||t.name.StartsWith("ID_MON_")){
     int col=int.Parse(t.name.Split('_').Last().TrimStart('C'));var pos=t.localPosition;pos.x=(col-1)*1.75f;t.localPosition=pos;
    }
   }
   Validate(true);
   var camera=UnityEngine.Object.FindFirstObjectByType<MobileQuarterView>().GetComponent<Camera>();
   Check(Metrics(camera,390,844,"FINAL REFIT useScaleTrue")==0,"Final geometry overlap");
   Capture(camera,390,844,Path.Combine(Output,"editor-390x844.png"));
   Check(EditorSceneManager.SaveScene(SceneManager.GetActiveScene()),"Refit save");
   Log("placement.txt",DateTimeOffset.Now.ToString("o")+" FINAL AWU1 normalized true geometry height1.35; yaw225; original material variantsA/B/C scene overrides; grid1.75x2.2; no source asset edit; scene saved PASS");
  }finally{EditorSceneManager.OpenScene(original,OpenSceneMode.Single);}
  Readback();
 }
 static void Readback(){
  string original=Original();EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
  try{Validate(true);var c=UnityEngine.Object.FindFirstObjectByType<MobileQuarterView>().GetComponent<Camera>();Check(Metrics(c,390,844,"REOPEN READBACK")==0,"Readback overlap");foreach(var b in Bindings())Log("material-instances.txt",b.name+" actualMaterials="+string.Join(";",b.GetComponentsInChildren<Renderer>().SelectMany(r=>r.sharedMaterials).Where(m=>m!=null).Select(m=>m.name+"/"+(m.HasProperty("_Albedo_Map")&&m.GetTexture("_Albedo_Map")!=null?AssetDatabase.GetAssetPath(m.GetTexture("_Albedo_Map")):"noAlbedo")).Distinct())+" height="+BoundsOf(b.gameObject).size.y);Log("readback.txt",DateTimeOffset.Now.ToString("o")+" Saved scene reopened; three vendor sources/nine instances/MON IDs/upright/no dummy/3D materials/bounds PASS");}
  finally{EditorSceneManager.OpenScene(original,OpenSceneMode.Single);Check(!SceneManager.GetActiveScene().isDirty,"Original restored dirty");}
 }
 static void Play(){
  SessionState.SetString("Polygon.Original",Original());SessionState.SetString("Polygon.Quality",Quality());SessionState.SetBool("Polygon.Background",Application.runInBackground);
  EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);Validate(true);
  typeof(NecroLocalizationUiProbe).GetMethod("SetSize",BindingFlags.NonPublic|BindingFlags.Static).Invoke(null,new object[]{390,844});
  SessionState.SetInt("Polygon.Errors",0);SessionState.SetBool("Polygon.Checking",true);SessionState.SetBool("Polygon.Play",true);
  frames=0;next=EditorApplication.timeSinceStartup+5;Application.runInBackground=true;EditorApplication.EnterPlaymode();
 }
 static void Audit(){
  foreach(char suffix in new[]{'A','B','C'}){
   var mat=AssetDatabase.LoadAssetAtPath<Material>(Vendor+"Materials/PolygonFantasyCharacters_01_"+suffix+".mat");
   Log("material-audit.txt",mat.name+" shader="+mat.shader.name);
   for(int i=0;i<ShaderUtil.GetPropertyCount(mat.shader);i++){
    string name=ShaderUtil.GetPropertyName(mat.shader,i);var type=ShaderUtil.GetPropertyType(mat.shader,i);
    if(type==ShaderUtil.ShaderPropertyType.TexEnv)Log("material-audit.txt",name+" texture="+(mat.GetTexture(name)==null?"NULL":AssetDatabase.GetAssetPath(mat.GetTexture(name))));
    else if(type==ShaderUtil.ShaderPropertyType.Color)Log("material-audit.txt",name+" color="+mat.GetColor(name));
   }
  }
 }
 static void AnimationReport(){
  var animators=Bindings().SelectMany(b=>b.GetComponentsInChildren<Animator>()).ToArray();
  int playable=animators.Count(a=>a.runtimeAnimatorController!=null&&a.runtimeAnimatorController.animationClips.Length>0);
  Log("animation.txt",DateTimeOffset.Now.ToString("o")+" actual PlayMode Animators="+animators.Length+" valid humanoid avatars="+animators.Count(a=>a.avatar!=null&&a.avatar.isValid&&a.avatar.isHuman)+" controllers with clips="+playable);
  Log("animation.txt",playable==0?"BLOCKED_NO_ANIMATION_CLIPS: imported vendor provides rigs but no motion controller/clips; T-pose visible. Animation motion PASS cannot be claimed. No fake procedural motion or unapproved animation download.":"Motion present; detailed clip/time/deformation validation still required.");
 }
 static void Capture(Camera c,int width,int height,string path){
  c.GetComponent<MobileQuarterView>().Configure((float)width/height);
  var rt=new RenderTexture(width,height,24){antiAliasing=2};rt.Create();var before=c.targetTexture;var active=RenderTexture.active;var tex=new Texture2D(width,height,TextureFormat.RGB24,false);
  try{c.targetTexture=rt;c.Render();RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,width,height),0,0);tex.Apply();File.WriteAllBytes(path,tex.EncodeToPNG());}
  finally{c.targetTexture=before;RenderTexture.active=active;rt.Release();UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(tex);}
 }
}
