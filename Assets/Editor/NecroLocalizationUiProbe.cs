using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Necrom.Core.Domain;
using Necrom.FirstPlayable.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
[InitializeOnLoad] public static class NecroLocalizationUiProbe {
 const BindingFlags F=BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Static;
 static double next;
 static string Root=>Path.GetFullPath(Path.Combine(Application.dataPath,".."));
 static string Dir=>Path.Combine(Root,"docs/evidence/I18n-UI-20261011");
 static NecroLocalizationUiProbe(){EditorApplication.update+=Tick;}
 static void Assert(bool value,string message){if(!value)throw new Exception(message);}
 static void Tick(){
  if(EditorApplication.isCompiling||EditorApplication.isUpdating)return;
  int step=SessionState.GetInt("I18nUi.Step",0);
  if(step==0){
   var job=Path.Combine(Root,"Library/i18n-ui-job.txt");if(!File.Exists(job)||EditorApplication.isPlayingOrWillChangePlaymode)return;
   File.Delete(job);Directory.CreateDirectory(Dir);
   File.WriteAllText(Path.Combine(Dir,"unity-validation.txt"),DateTimeOffset.Now.ToString("o")+"\nUnity "+Application.unityVersion+"\n");
   SessionState.SetInt("I18nUi.Step",1);EditorApplication.isPlaying=true;next=EditorApplication.timeSinceStartup+5;return;
  }
  if(EditorApplication.timeSinceStartup<next)return;
  try{
   if(step==1){
    if(!EditorApplication.isPlaying)return;
    Assert(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name=="FirstPlayable","Unexpected scene");
    SessionState.SetFloat("I18nUi.TimeScale",Time.timeScale);Time.timeScale=0;
    SessionState.SetBool("I18nUi.Background",Application.runInBackground);Application.runInBackground=true;
    var popup=GameObject.Find("WelcomeBackRewardPopup");if(popup!=null)popup.SetActive(false);
    SetSize(390,844);SessionState.SetInt("I18nUi.Step",2);next=EditorApplication.timeSinceStartup+3;return;
   }
   if(step==2||step==4||step==6){
    Assert(Screen.width==390&&Screen.height==844,"Expected 390x844, got "+Screen.width+"x"+Screen.height);
    string lang=step==4?"en":"ko";
    Assert(LocalizationRuntime.SetLanguage(lang)||LocalizationRuntime.Manager.Language==lang,"Switch rejected");
    if(step==4){
     var button=UnityEngine.Object.FindObjectsByType<LocalizedTextBinding>(FindObjectsSortMode.None).First();
     button.enabled=false;LocalizationRuntime.SetLanguage("ko");button.enabled=true;
     Assert(button.GetComponent<Text>().text==LocalizationRuntime.Manager.Get(button.Key),"Disabled language switch refresh");
     var probe=new GameObject("I18nLifecycleQA",typeof(RectTransform),typeof(Text));
     probe.SetActive(false);var text=probe.GetComponent<Text>();
     var binding=LocalizedTextBinding.Attach(text,"ui.summon");
     LocalizedTextBinding.Attach(text,"ui.skills");probe.SetActive(true);
     Assert(text.text==LocalizationRuntime.Manager.Get("ui.skills"),"Inactive attachment/rebinding");
     UnityEngine.Object.DestroyImmediate(probe);
     LocalizationRuntime.SetLanguage("en");
     Assert(button.GetComponent<Text>().text==LocalizationRuntime.Manager.Get(button.Key),"Resubscribed language switch");
     File.AppendAllText(Path.Combine(Dir,"unity-validation.txt"),"PASS binding disabled -> language switch -> enabled; inactive attachment; different-key rebind; destroyed subscriber then switch\n");
    }
    Canvas.ForceUpdateCanvases();
    var texts=UnityEngine.Object.FindObjectsByType<Text>(FindObjectsSortMode.None);
    foreach(var key in new[]{"ui.summon","ui.formation","ui.skills"}){
     var binding=UnityEngine.Object.FindObjectsByType<LocalizedTextBinding>(FindObjectsSortMode.None).Single(x=>x.Key==key);
     Assert(binding.GetComponent<Text>().text==LocalizationRuntime.Manager.Get(key),"UI mismatch "+key);
     binding.enabled=false;binding.enabled=true;
     Assert(binding.GetComponent<Text>().text==LocalizationRuntime.Manager.Get(key),"Reenable mismatch "+key);
    }
    var exp=texts.Single(x=>x.name=="AccountExpLabel");
    Assert(exp.text.StartsWith(lang=="en"?"Lv. ":"레벨 "),"EXP language mismatch "+exp.text);
    Assert(!exp.text.Contains("[ui."),"Missing EXP key");
    var resolver=new LocalizedContentNames(LocalizationRuntime.Manager);
    foreach(var trait in TftTaxonomy.Create())Assert(resolver.ResolveTraitName(trait)==LocalizationRuntime.Manager.Get(trait.localizationKey),"Trait mismatch "+trait.id);
    File.AppendAllText(Path.Combine(Dir,"unity-validation.txt"),"PASS "+lang+" "+Screen.width+"x"+Screen.height+"; 3 menu labels; EXP="+exp.text+"; 18 trait names; binding disable/reenable\n");
    ScreenCapture.CaptureScreenshot(Path.Combine(Dir,step==4?"en-390x844.png":step==2?"ko-390x844.png":"ko-return-390x844.png"));
    SessionState.SetInt("I18nUi.Step",step+1);next=EditorApplication.timeSinceStartup+2;return;
   }
   if(step==3||step==5){SessionState.SetInt("I18nUi.Step",step+1);return;}
   if(step==7){
    foreach(var file in new[]{"ko-390x844.png","en-390x844.png","ko-return-390x844.png"})Assert(File.Exists(Path.Combine(Dir,file)),"Screenshot missing "+file);
    File.AppendAllText(Path.Combine(Dir,"unity-validation.txt"),"PASS native PlayMode ko -> en -> ko; screenshots captured. No settings UI/persisted language/120 full translations.\n");
    Finish();Debug.Log("[I18N-UI] PASS");
   }
  }catch(Exception e){File.AppendAllText(Path.Combine(Dir,"unity-validation.txt"),"FAIL "+e+"\n");Finish();Debug.LogException(e);}
 }
 static void Finish(){
  Time.timeScale=SessionState.GetFloat("I18nUi.TimeScale",1);
  Application.runInBackground=SessionState.GetBool("I18nUi.Background",false);
  SessionState.SetInt("I18nUi.Step",0);EditorApplication.isPlaying=false;
 }
 static void SetSize(int width,int height){
  var assembly=typeof(Editor).Assembly;
  var sizesType=assembly.GetType("UnityEditor.GameViewSizes");
  var single=typeof(ScriptableSingleton<>).MakeGenericType(sizesType);
  var instance=single.GetProperty("instance",F).GetValue(null);
  var group=sizesType.GetProperty("currentGroup",F).GetValue(instance);
  var sizeType=assembly.GetType("UnityEditor.GameViewSize");
  var enumType=assembly.GetType("UnityEditor.GameViewSizeType");
  var entry=Activator.CreateInstance(sizeType,F,null,new object[]{Enum.Parse(enumType,"FixedResolution"),width,height,"I18N-QA-"+width+"x"+height},null);
  int index=(int)group.GetType().GetMethod("GetTotalCount",F).Invoke(group,null);
  group.GetType().GetMethod("AddCustomSize",F).Invoke(group,new[]{entry});
  var viewType=assembly.GetType("UnityEditor.GameView");
  var view=EditorWindow.GetWindow(viewType);
  viewType.GetMethod("SizeSelectionCallback",F).Invoke(view,new object[]{index,null});
  var min=viewType.GetProperty("minScale",F);var snap=viewType.GetMethod("SnapZoom",F);
  if(min!=null&&snap!=null)snap.Invoke(view,new[]{min.GetValue(view)});view.Repaint();
 }
}
