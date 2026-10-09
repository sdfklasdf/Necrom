using System;
using System.IO;
using System.Linq;
using Necrom.ContentDraft;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
[InitializeOnLoad] public static class NecroQuarterViewProbe {
 const string Folder="Assets/Necrom/QuarterView3D",ScenePath=Folder+"/QuarterView3D.unity";
 static string Root=>Path.GetFullPath(Path.Combine(Application.dataPath,".."));
 static string Output=>Path.Combine(Root,"docs/evidence/QuarterView-20261010");
 static int frames;
 static NecroQuarterViewProbe(){EditorApplication.update+=Tick;Application.logMessageReceived+=OnLog;}
 static void OnLog(string c,string s,LogType t){if(SessionState.GetBool("QuarterChecking",false)&&(t==LogType.Error||t==LogType.Exception||t==LogType.Assert))SessionState.SetInt("QuarterErrors",SessionState.GetInt("QuarterErrors",0)+1);}
 static void Check(bool ok,string message){if(!ok)throw new InvalidOperationException(message);}
 static string Quality()=>QualitySettings.shadows+"/"+QualitySettings.shadowResolution+"/"+QualitySettings.shadowDistance+"/"+QualitySettings.shadowCascades+"/"+QualitySettings.pixelLightCount+"/"+QualitySettings.antiAliasing+"/"+QualitySettings.softParticles+"/"+QualitySettings.realtimeReflectionProbes;
 static void Tick(){
 if(EditorApplication.isCompiling||EditorApplication.isUpdating)return;
 if(EditorApplication.isPlaying) {
 if(!SessionState.GetBool("QuarterPlay",false))return;
 if(++frames<60){EditorApplication.QueuePlayerLoopUpdate();return;}
 SessionState.SetBool("QuarterPlay",false);
 try{
 var rig=UnityEngine.Object.FindFirstObjectByType<MobileQuarterView>();Check(rig!=null,"Runtime camera rig missing");
 Check(QualitySettings.shadows==ShadowQuality.HardOnly&&QualitySettings.shadowCascades==1&&QualitySettings.pixelLightCount==1,"Runtime shadow profile not applied");
 Check(Mathf.Approximately(QualitySettings.shadowDistance,40)&&QualitySettings.antiAliasing==2&&!QualitySettings.softParticles&&!QualitySettings.realtimeReflectionProbes,"Runtime mobile quality mismatch");
 var camera=rig.GetComponent<Camera>();Check(Mathf.Abs(camera.transform.eulerAngles.x-45)<.01f&&Mathf.Abs(camera.transform.eulerAngles.y-45)<.01f,"Runtime camera angle");
 foreach(var size in new[]{new Vector2Int(540,960),new Vector2Int(390,844),new Vector2Int(360,640)}) {
 rig.Configure((float)size.x/size.y);BoundsCheck(rig);Render(camera,size.x,size.y,Path.Combine(Output,"play-"+size.x+"x"+size.y+".png"));
 }
 Check(SessionState.GetInt("QuarterErrors",0)==0,"Scoped Unity errors present");
 File.AppendAllText(Path.Combine(Output,"validation.txt"),"ACTUAL_PLAYMODE PASS: camera angles/3 portrait sizes/120 cube proxies/HardOnly shadows/1024 light map/40m/1 cascade/1 pixel light/2xAA/error0\nRuntime quality="+Quality()+"\n");
 }catch(Exception e){File.AppendAllText(Path.Combine(Output,"validation.txt"),"PLAY FAIL "+e+"\n");Debug.LogException(e);}
 SessionState.SetBool("QuarterChecking",false);EditorApplication.ExitPlaymode();SessionState.SetBool("QuarterRestore",true);return;
 }
 if(EditorApplication.isPlayingOrWillChangePlaymode)return;
 if(SessionState.GetBool("QuarterRestore",false)){
 SessionState.SetBool("QuarterRestore",false);Application.runInBackground=SessionState.GetBool("QuarterBackground",false);
 Check(Quality()==SessionState.GetString("QuarterOldQuality",""),"Runtime quality restoration failed");
 File.AppendAllText(Path.Combine(Output,"validation.txt"),"After PlayMode quality restored PASS: "+Quality()+"\n");
 EditorSceneManager.OpenScene(SessionState.GetString("QuarterOriginal",""),OpenSceneMode.Single);
 File.AppendAllText(Path.Combine(Output,"validation.txt"),"Original scene restored clean="+!SceneManager.GetActiveScene().isDirty+"\n");return;
 }
 var job=Path.Combine(Root,"Library/quarterview-job.txt");if(!File.Exists(job))return;
 var command=File.ReadAllText(job).Trim();File.Delete(job);Directory.CreateDirectory(Output);
 try{
 if(command=="VERIFY"){Verify();return;} if(command=="READBACK"){Readback();return;}
 SessionState.SetInt("QuarterErrors",0);SessionState.SetBool("QuarterChecking",true);Build();
 }catch(Exception e){File.AppendAllText(Path.Combine(Output,"validation.txt"),"FAIL "+e+"\n");SessionState.SetBool("QuarterChecking",false);Debug.LogException(e);}
 }
 static void Verify(){ var original=SceneManager.GetActiveScene();Check(EditorSceneManager.sceneCount==1&&!original.isDirty,"Protect original");SessionState.SetString("QuarterOriginal",original.path);SessionState.SetString("QuarterOldQuality",Quality());SessionState.SetBool("QuarterBackground",Application.runInBackground);EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);var rig=UnityEngine.Object.FindFirstObjectByType<MobileQuarterView>();rig.Configure(9f/16);EditorSceneManager.SaveScene(SceneManager.GetActiveScene());var old=Quality();try{rig.ApplyMobileQuality();Check(QualitySettings.shadowCascades==1&&QualitySettings.pixelLightCount==1,"Actual quality="+Quality());BoundsCheck(rig);Render(rig.GetComponent<Camera>(),540,960,Path.Combine(Output,"editor-540x960.png"));}finally{rig.RestoreQuality();}Check(Quality()==old,"Editor restore");File.WriteAllText(Path.Combine(Output,"validation.txt"),DateTime.Now.ToString("o")+" Unity "+Application.unityVersion+" Built-in; EDIT apply/restore PASS; actual quality baseline="+old+"\n");SessionState.SetInt("QuarterErrors",0);SessionState.SetBool("QuarterChecking",true);SessionState.SetBool("QuarterPlay",true);frames=0;Application.runInBackground=true;EditorApplication.EnterPlaymode(); } static void Build(){
 Check(GraphicsSettings.currentRenderPipeline==null,"Expected current Built-in pipeline");
 var original=SceneManager.GetActiveScene();Check(EditorSceneManager.sceneCount==1&&!original.isDirty,"Protect unsaved original scenes");
 Check(!File.Exists(ScenePath),"Do not overwrite a pre-existing preparation scene");
 Directory.CreateDirectory(Folder+"/Materials");AssetDatabase.Refresh();
 var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Additive);SceneManager.SetActiveScene(scene);
 RenderSettings.skybox=null;RenderSettings.ambientMode=AmbientMode.Flat;RenderSettings.ambientLight=new Color(.68f,.7f,.76f);RenderSettings.reflectionIntensity=0;RenderSettings.fog=false;
 var floorMat=Material("Board",new Color(.33f,.52f,.44f));
 var allyMat=Material("AllyProxy",new Color(.23f,.8f,.75f));
 var enemyMat=Material("EnemyProxy",new Color(1f,.53f,.42f));
 var stripeMat=Material("Grid",new Color(.7f,.79f,.65f));
 var board=new GameObject("BattlefieldBounds_7x16_LayoutFixture");board.transform.rotation=Quaternion.Euler(0,45,0);
 var floor=Cube("Board",board.transform,new Vector3(0,-.1f,0),new Vector3(7,.2f,16),floorMat);floor.GetComponent<Renderer>().shadowCastingMode=ShadowCastingMode.Off;
 for(int col=0;col<=6;col++){var line=Cube("GridColumn",board.transform,new Vector3(-3.15f+col*1.05f,.01f,0),new Vector3(.02f,.02f,15.6f),stripeMat);line.GetComponent<Renderer>().shadowCastingMode=ShadowCastingMode.Off;}
 for(int row=0;row<=20;row++){var line=Cube("GridRow",board.transform,new Vector3(0,.01f,-7.6f+row*.76f),new Vector3(6.3f,.02f,.02f),stripeMat);line.GetComponent<Renderer>().shadowCastingMode=ShadowCastingMode.Off;}
 for(int row=0;row<20;row++)for(int col=0;col<6;col++){
 var go=Cube("PROXY_"+(row*6+col+1).ToString("D3"),board.transform,new Vector3((col-2.5f)*1.05f,.45f,(row-9.5f)*.76f),new Vector3(.48f,.9f,.48f),row<10?allyMat:enemyMat);
 go.transform.localRotation=Quaternion.Euler(0,45,0);
 }
 var lightGo=new GameObject("Directional Light");var light=lightGo.AddComponent<Light>();lightGo.transform.rotation=Quaternion.Euler(55,-25,0);light.intensity=1;light.color=new Color(1f,.96f,.88f);light.lightmapBakeType=LightmapBakeType.Realtime;RenderSettings.sun=light;
 var cameraGo=new GameObject("Main Camera");cameraGo.tag="MainCamera";var camera=cameraGo.AddComponent<Camera>();camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.1f,.15f,.19f);
 cameraGo.AddComponent<AudioListener>();var rig=cameraGo.AddComponent<MobileQuarterView>();rig.mainLight=light;rig.Configure(9f/16);
 Check(EditorSceneManager.SaveScene(scene,ScenePath),"Scene save failed");AssetDatabase.SaveAssets();
 var old=Quality();rig.ApplyMobileQuality();Check(QualitySettings.shadowCascades==1&&QualitySettings.pixelLightCount==1,"Quality apply");
 BoundsCheck(rig);Render(camera,540,960,Path.Combine(Output,"editor-540x960.png"));
 rig.RestoreQuality();Check(Quality()==old,"Edit validation quality restore");
 File.WriteAllText(Path.Combine(Output,"validation.txt"),DateTime.Now.ToString("o")+"\nUnity "+Application.unityVersion+" Built-in\nSaved camera Orthographic, X45 Y45, portrait9:16, near.3 far60 HDRoff Forward\nCamera position="+camera.transform.position+" orthographicSize="+camera.orthographicSize+"\nSingle Directional Light hard shadows, custom1024, strength.65\n120 cube proxies are layout fixtures, NOT purchased rigs or mobile performance evidence\nEdit apply/restore global quality PASS\n");
 EditorSceneManager.CloseScene(scene,true);SceneManager.SetActiveScene(original);
 SessionState.SetString("QuarterOriginal",original.path);SessionState.SetString("QuarterOldQuality",Quality());SessionState.SetBool("QuarterBackground",Application.runInBackground);
 EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);SessionState.SetBool("QuarterPlay",true);Application.runInBackground=true;EditorApplication.EnterPlaymode();
 }
 static Material Material(string name,Color color){
 var shader=Shader.Find("Standard");Check(shader!=null,"Standard shader missing");var m=new Material(shader){name=name,color=color,enableInstancing=true};
 m.SetFloat("_Glossiness",0);m.SetFloat("_Metallic",0);AssetDatabase.CreateAsset(m,Folder+"/Materials/"+name+".mat");return m;
 }
 static GameObject Cube(string name,Transform parent,Vector3 position,Vector3 scale,Material m){
 var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.transform.SetParent(parent,false);go.transform.localPosition=position;go.transform.localScale=scale;
 go.GetComponent<Renderer>().sharedMaterial=m;UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());return go;
 }
 static void BoundsCheck(MobileQuarterView rig){
 var c=rig.GetComponent<Camera>();float minX=1,minY=1,maxX=0,maxY=0;
 foreach(var x in new[]{-1,1})foreach(var y in new[]{-1,1})foreach(var z in new[]{-1,1}){
 var point=rig.boardCenter+Quaternion.Euler(0,rig.boardYaw,0)*Vector3.Scale(rig.boardExtents,new Vector3(x,y,z));
 var v=c.WorldToViewportPoint(point);Check(v.z>c.nearClipPlane&&v.z<c.farClipPlane&&v.x>.01f&&v.x<.99f&&v.y>.01f&&v.y<.99f,"Battlefield clipping");
 minX=Mathf.Min(minX,v.x);maxX=Mathf.Max(maxX,v.x);minY=Mathf.Min(minY,v.y);maxY=Mathf.Max(maxY,v.y);
 }
 File.AppendAllText(Path.Combine(Output,"bounds.txt"),"aspect="+c.aspect+" size="+c.orthographicSize+" viewport=("+minX+","+minY+")..("+maxX+","+maxY+") PASS\n");
 }
 static void Readback(){
 var original=SceneManager.GetActiveScene();Check(!original.isDirty,"Unsaved original scene");
 var scene=EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Additive);
 var camera=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Camera>()).Single();
 var rig=camera.GetComponent<MobileQuarterView>();var lights=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Light>()).ToArray();
 Check(rig!=null&&camera.orthographic&&Mathf.Abs(camera.transform.eulerAngles.x-45)<.01f&&Mathf.Abs(camera.transform.eulerAngles.y-45)<.01f,"Serialized camera mismatch");
 Check(lights.Length==1&&lights[0].shadows==LightShadows.Hard&&lights[0].shadowCustomResolution==1024&&rig.mainLight==lights[0],"Serialized light reference mismatch");
 Check(scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Transform>()).Count(t=>t.name.StartsWith("PROXY_"))==120,"Proxy count");
 rig.Configure(9f/16);BoundsCheck(rig);
 EditorSceneManager.CloseScene(scene,true);SceneManager.SetActiveScene(original);
 File.WriteAllText(Path.Combine(Output,"fresh-readback.txt"),DateTime.Now.ToString("o")+"\nReloaded serialized saved camera/light/rig refs/120proxies/bounds PASS\nOriginal scene="+original.path+" dirty="+original.isDirty+"\n");
 }
 static void Render(Camera camera,int width,int height,string path){
 var rt=new RenderTexture(width,height,24){antiAliasing=2};rt.Create();
 var before=camera.targetTexture;var active=RenderTexture.active;var texture=new Texture2D(width,height,TextureFormat.RGB24,false);
 try{camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;texture.ReadPixels(new Rect(0,0,width,height),0,0);texture.Apply();File.WriteAllBytes(path,texture.EncodeToPNG());}
 finally{camera.targetTexture=before;RenderTexture.active=active;rt.Release();UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(texture);}
 }
}