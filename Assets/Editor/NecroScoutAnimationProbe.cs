using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using Necrom.ContentDraft;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
[InitializeOnLoad]
public static class NecroScoutAnimationProbe {
 const string Scene="Assets/Necrom/QuarterView3D/DummyFormation.unity";
 const string Dir="Assets/Necrom/QuarterView3D/Animations";
 const string Source="Assets/Kevin Iglesias/Human Animations/Animations/";
 const string Out="docs/evidence/Animation-20261011";
 const string Job="Library/scout-animation-job.txt";
 static readonly string[] States={"Idle","Walk","Attack"};
 static int phase=-1,samples,errors,overlaps;static double phaseStart,next;static bool running;static string original;static Quaternion[][] initial;static float[] maxMotion;static Vector3[] roots;
 static NecroScoutAnimationProbe(){EditorApplication.update+=Tick;Application.logMessageReceived+=OnLog;EditorApplication.playModeStateChanged+=OnMode;}
 static void Check(bool b,string m){if(!b)throw new InvalidOperationException(m);}
 static void Log(string f,string s){Directory.CreateDirectory(Out);File.AppendAllText(Out+"/"+f,DateTimeOffset.Now.ToString("o")+" "+s+"\n");}
 static CharacterVisualBinding[] Bindings()=>UnityEngine.Object.FindObjectsByType<CharacterVisualBinding>(FindObjectsSortMode.None).OrderBy(b=>b.name).ToArray();
 static Animator[] Animators()=>Bindings().Select(b=>b.GetComponentInChildren<Animator>()).ToArray();
 static string Original(){var s=SceneManager.GetActiveScene();Check(EditorSceneManager.sceneCount==1&&!s.isDirty&&!string.IsNullOrEmpty(s.path),"Protect dirty/unsaved current scene");return s.path;}
 static void OnLog(string c,string st,LogType t){if(SessionState.GetBool("ScoutAnim.Checking",false)&&(t==LogType.Error||t==LogType.Exception||t==LogType.Assert)){errors++;Log("runtime-errors.txt",t+" "+c+"\n"+st);}}
 static void OnMode(PlayModeStateChange mode){if(mode==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool("ScoutAnim.Play",false)){running=true;phase=-1;next=EditorApplication.timeSinceStartup+2;errors=0;overlaps=0;}}
 static void Tick(){
  if(EditorApplication.isCompiling||EditorApplication.isUpdating)return;
  if(EditorApplication.isPlaying){if(!running)return;EditorApplication.QueuePlayerLoopUpdate();if(EditorApplication.timeSinceStartup<next)return;next=EditorApplication.timeSinceStartup+.04;
   try{if(phase<0){Check(Screen.width==390&&Screen.height==844,"Actual GameView "+Screen.width+"x"+Screen.height);Validate();StartPhase(0);return;}Sample();double duration=phase==0?3.4:phase==1?2.1:1.05;if(EditorApplication.timeSinceStartup-phaseStart>=duration){FinishPhase();if(phase<2){StartPhase(phase+1);return;}Check(errors==0,"Scoped runtime errors");Check(overlaps==0,"Animated projected overlaps");Log("play-validation.txt","NativePlay Unity"+Application.unityVersion+" 390x844 Idle/Walk/Attack, nine valid animators, bone motion and normalizedTime, no clipping, sampled overlaps0/scopederrors0 PASS");Stop();}}
   catch(Exception e){Log("failure-motion.txt",e.ToString());Stop();}return;
  }
  if(SessionState.GetBool("ScoutAnim.Restore",false)&&!EditorApplication.isPlayingOrWillChangePlaymode){SessionState.SetBool("ScoutAnim.Restore",false);Application.runInBackground=SessionState.GetBool("ScoutAnim.Background",false);EditorSceneManager.OpenScene(SessionState.GetString("ScoutAnim.Original",""),OpenSceneMode.Single);Check(!SceneManager.GetActiveScene().isDirty,"Original dirty after restore");Log("play-validation.txt","Original clean scene restored "+SceneManager.GetActiveScene().path);}
  if(EditorApplication.isPlayingOrWillChangePlaymode||!File.Exists(Job))return;string command=File.ReadAllText(Job).Trim();File.Delete(Job);
  try{if(command=="ASSERT")Readback(true);else if(command=="BUILD")Build();else if(command=="READBACK")Readback(false);else if(command=="PLAY")Play();else throw new Exception("Unknown job "+command);}catch(Exception e){Log("setup-failure.txt",command+" "+e);}
 }
 static AnimationClip Clip(string sex,string relative){var p=Source+(sex=="M"?"Male/":"Female/")+relative.Replace("SEX",sex);var c=AssetDatabase.LoadAllAssetsAtPath(p).OfType<AnimationClip>().FirstOrDefault(x=>!x.name.StartsWith("__preview__"));Check(c!=null&&c.isHumanMotion&&c.length>0,"Missing human clip "+p);return c;}
 static AnimationClip[] Clips(string sex)=>new[]{Clip(sex,"Idles/HumanSEX@Idle01.fbx"),Clip(sex,"Movement/Walk/HumanSEX@Walk01_Forward.fbx"),Clip(sex,"Combat/1H/HumanSEX@Attack1H01_R.fbx")};
 static void Validate(){var b=Bindings();Check(b.Length==9,"Expected exactly nine scouts");foreach(var id in new[]{"MON_001","MON_002","MON_003"})Check(b.Count(x=>x.characterId==id)==3,"Three each "+id);foreach(var x in b){var a=x.GetComponentInChildren<Animator>();Check(!x.isDummy&&a!=null&&a.avatar!=null&&a.avatar.isValid&&a.avatar.isHuman,"Valid real humanoid "+x.name);Check(a.runtimeAnimatorController!=null&&a.runtimeAnimatorController.animationClips.Count(c=>c!=null&&c.isHumanMotion)>=3,"Controller missing Idle Walk Attack "+x.name);Check(!a.applyRootMotion,"Root motion must remain disabled");foreach(var r in x.GetComponentsInChildren<Renderer>())foreach(var m in r.sharedMaterials)Check(m!=null&&m.shader.isSupported,"Material unsupported");}}
 static void Readback(bool red){var o=Original();EditorSceneManager.OpenScene(Scene,OpenSceneMode.Single);try{if(red){try{Validate();throw new Exception("RED assertion unexpectedly passed");}catch(InvalidOperationException e){Check(e.Message.StartsWith("Controller missing"),"Wrong RED reason "+e.Message);Log("red-before.txt","EXPECTED_FAIL "+e.Message);}}else{Validate();foreach(var b in Bindings())Log("readback.txt",b.name+"/"+b.characterId+" controller="+AssetDatabase.GetAssetPath(b.GetComponentInChildren<Animator>().runtimeAnimatorController));Log("readback.txt","Saved scene reopened, 9 rig/controller/3human clips/rootMotionOFF PASS");}}finally{EditorSceneManager.OpenScene(o,OpenSceneMode.Single);}}
 static void Build(){
  var o=Original();Check(!AssetDatabase.IsValidFolder(Dir),"Protect existing animation config folder");
  var male=Clips("M");var female=Clips("F");
  AssetDatabase.CreateFolder("Assets/Necrom/QuarterView3D","Animations");
  var controller=AnimatorController.CreateAnimatorControllerAtPath(Dir+"/ScoutHumanoidPreview.controller");
  var sm=controller.layers[0].stateMachine;for(int i=0;i<3;i++){var state=sm.AddState(States[i]);state.motion=male[i];if(i==0)sm.defaultState=state;}
  var witch=new AnimatorOverrideController(controller);AssetDatabase.CreateAsset(witch,Dir+"/ScoutWitchPreview.overrideController");
  var overrides=new List<KeyValuePair<AnimationClip,AnimationClip>>();witch.GetOverrides(overrides);for(int i=0;i<overrides.Count;i++){var ix=Array.IndexOf(male,overrides[i].Key);Check(ix>=0,"Unknown override clip");overrides[i]=new KeyValuePair<AnimationClip,AnimationClip>(overrides[i].Key,female[ix]);}witch.ApplyOverrides(overrides);EditorUtility.SetDirty(witch);AssetDatabase.SaveAssets();
  EditorSceneManager.OpenScene(Scene,OpenSceneMode.Single);
  try{Check(Bindings().Length==9,"Exactly nine existing scouts");foreach(var b in Bindings()){var a=b.GetComponentInChildren<Animator>();Check(a.runtimeAnimatorController==null,"Protect existing controller "+b.name);a.runtimeAnimatorController=b.characterId=="MON_002"?witch:controller;a.applyRootMotion=false;a.cullingMode=AnimatorCullingMode.AlwaysAnimate;PrefabUtility.RecordPrefabInstancePropertyModifications(a);}
   Validate();Check(EditorSceneManager.SaveScene(SceneManager.GetActiveScene()),"Save controller assignment");Log("configuration.txt","Created3state base+femaleOverride; assigned9 Idle/Walk/Attack/rootMotionOFF; source vendor prefabs and battle/catalog unchanged");
   foreach(var c in male.Concat(female))Log("configuration.txt",AssetDatabase.GetAssetPath(c)+"/"+c.name+" length="+c.length+" loop="+AnimationUtility.GetAnimationClipSettings(c).loopTime);
  }finally{EditorSceneManager.OpenScene(o,OpenSceneMode.Single);}Readback(false);
 }
 static void Play(){original=Original();SessionState.SetString("ScoutAnim.Original",original);SessionState.SetBool("ScoutAnim.Background",Application.runInBackground);try{EditorSceneManager.OpenScene(Scene,OpenSceneMode.Single);Validate();typeof(NecroLocalizationUiProbe).GetMethod("SetSize",BindingFlags.NonPublic|BindingFlags.Static).Invoke(null,new object[]{390,844});SessionState.SetBool("ScoutAnim.Play",true);SessionState.SetBool("ScoutAnim.Checking",true);Application.runInBackground=true;EditorApplication.EnterPlaymode();}catch{SessionState.SetBool("ScoutAnim.Play",false);SessionState.SetBool("ScoutAnim.Checking",false);Application.runInBackground=SessionState.GetBool("ScoutAnim.Background",false);EditorSceneManager.OpenScene(original,OpenSceneMode.Single);throw;}}
 static Transform[] Bones(Animator a)=>new[]{HumanBodyBones.Hips,HumanBodyBones.Spine,HumanBodyBones.LeftUpperArm,HumanBodyBones.RightUpperArm,HumanBodyBones.LeftLowerArm,HumanBodyBones.RightLowerArm,HumanBodyBones.LeftUpperLeg,HumanBodyBones.RightUpperLeg,HumanBodyBones.LeftLowerLeg,HumanBodyBones.RightLowerLeg}.Select(a.GetBoneTransform).ToArray();
 static void StartPhase(int p){phase=p;samples=0;phaseStart=EditorApplication.timeSinceStartup;var aa=Animators();maxMotion=new float[9];roots=aa.Select(a=>a.transform.position).ToArray();foreach(var a in aa){a.Play(States[p],0,0);a.Update(0);}initial=aa.Select(a=>Bones(a).Select(t=>t.localRotation).ToArray()).ToArray();Log("motion.txt","Start "+States[p]);}
 static void Sample(){samples++;var aa=Animators();var c=UnityEngine.Object.FindFirstObjectByType<MobileQuarterView>().GetComponent<Camera>();c.GetComponent<MobileQuarterView>().Configure(390f/844);var rects=new List<Rect>();for(int i=0;i<aa.Length;i++){var a=aa[i];var state=a.GetCurrentAnimatorStateInfo(0);Check(state.IsName(States[phase]),"Wrong active state");Check((a.transform.position-roots[i]).magnitude<.001f,"Unexpected root drift");var bones=Bones(a);for(int j=0;j<bones.Length;j++){Check(bones[j]!=null&&float.IsFinite(bones[j].position.x)&&float.IsFinite(bones[j].position.y)&&float.IsFinite(bones[j].position.z),"Invalid bone");maxMotion[i]=Mathf.Max(maxMotion[i],Quaternion.Angle(initial[i][j],bones[j].localRotation));}var vertices=(Vector3[])typeof(NecroPolygonScoutProbe).GetMethod("GeometryPoints",BindingFlags.NonPublic|BindingFlags.Static).Invoke(null,new object[]{Bindings()[i].gameObject});var min=new Vector2(float.MaxValue,float.MaxValue);var max=new Vector2(float.MinValue,float.MinValue);foreach(var v in vertices){var q=c.WorldToViewportPoint(v);Check(q.z>c.nearClipPlane&&q.z<c.farClipPlane&&q.x>0&&q.x<1&&q.y>0&&q.y<1,"Animated clipping");var pos=new Vector2(q.x*390,q.y*844);min=Vector2.Min(min,pos);max=Vector2.Max(max,pos);}rects.Add(Rect.MinMaxRect(min.x,min.y,max.x,max.y));}
  int pairs=0;for(int i=0;i<9;i++)for(int j=i+1;j<9;j++)if(rects[i].Overlaps(rects[j]))pairs++;overlaps+=pairs;Check(pairs==0,"Animated projected rectangle overlaps="+pairs);
  if(samples==3||samples==10||samples==20){Capture(c,Out+"/play-"+States[phase].ToLowerInvariant()+"-"+samples+"-390x844.png");}
 }
 static void FinishPhase(){Check(samples>=5,"Insufficient native samples");Check(maxMotion.All(x=>x>.05f),"No real bone motion in some scouts");var aa=Animators();foreach(var a in aa){Check(a.GetCurrentAnimatorStateInfo(0).normalizedTime>.01f,"Animation time not progressing");Log("motion.txt",a.name+" "+States[phase]+" normalizedTime="+a.GetCurrentAnimatorStateInfo(0).normalizedTime.ToString("F3"));}Log("motion.txt",States[phase]+" samples="+samples+" boneDeltaDegrees="+string.Join(",",maxMotion.Select(x=>x.ToString("F2")))+" overlaps0/unclipped/rootDrift0 PASS");}
 static void Capture(Camera c,string path){typeof(NecroPolygonScoutProbe).GetMethod("Capture",BindingFlags.NonPublic|BindingFlags.Static).Invoke(null,new object[]{c,390,844,path});}
 static void Stop(){running=false;SessionState.SetBool("ScoutAnim.Play",false);SessionState.SetBool("ScoutAnim.Checking",false);SessionState.SetBool("ScoutAnim.Restore",true);EditorApplication.ExitPlaymode();}
}
