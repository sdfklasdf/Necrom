using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

// Editor-only acceptance probe. Opt-in via Library/exp-ui-job.txt; never runs in a player.
[InitializeOnLoad]
public static class NecroExpUiProbe
{
    const BindingFlags F = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
    static readonly Vector2Int[] Sizes = { new Vector2Int(390,844), new Vector2Int(360,640), new Vector2Int(1080,1920), new Vector2Int(844,390), new Vector2Int(768,1024), new Vector2Int(390,844) };
    static double next;
    static double sizeDeadline;
    static string Root => Path.GetFullPath(Path.Combine(Application.dataPath,".."));
    static string Job => Path.Combine(Root,"Library/exp-ui-job.txt");
    static string Out => Path.Combine(Root,"docs/evidence/EXP-UI-20261009");
    static NecroExpUiProbe() { EditorApplication.update += Tick; Application.logMessageReceived += (message,stack,type) => { if(SessionState.GetBool("exp-probe-active",false) && (type==LogType.Error || type==LogType.Exception || type==LogType.Assert)) SessionState.SetInt("exp-probe-errors",SessionState.GetInt("exp-probe-errors",0)+1); }; }
    static void Tick()
    {
        if (EditorApplication.isPlaying && SessionState.GetBool("exp-probe-active",false)) { Application.runInBackground=true; EditorApplication.QueuePlayerLoopUpdate(); }
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.timeSinceStartup < next) return;
        try
        {
            if (!SessionState.GetBool("exp-probe-active",false))
            {
                if (!File.Exists(Job)) return;
                string mode=File.ReadAllText(Job).Trim(); File.Delete(Job);
                SessionState.SetString("exp-probe-mode",mode); SessionState.SetInt("exp-probe-index",0);
                SessionState.SetBool("exp-probe-active",true);
                SessionState.SetInt("exp-probe-errors",0);
                SessionState.SetBool("exp-probe-sized",false);
                SessionState.SetBool("exp-probe-capture",false);
                SessionState.SetBool("exp-probe-background",Application.runInBackground);
                Directory.CreateDirectory(Out);
                File.WriteAllText(Path.Combine(Out,mode+".txt"),"Actual Editor PlayMode acceptance; "+DateTime.Now.ToString("o")+"\n");
                if (!EditorApplication.isPlaying) EditorApplication.isPlaying=true;
                next=EditorApplication.timeSinceStartup+8;
                return;
            }
            if (!EditorApplication.isPlaying) return;
            int index=SessionState.GetInt("exp-probe-index",0);
            if (index>=Sizes.Length)
            {
                File.AppendAllText(Path.Combine(Out,SessionState.GetString("exp-probe-mode","")+".txt"),"ERROR_COUNT="+SessionState.GetInt("exp-probe-errors",0)+"\n");
                SessionState.SetBool("exp-probe-active",false);
                Application.runInBackground=SessionState.GetBool("exp-probe-background",false);
                EditorApplication.isPlaying=false;
                return;
            }
            if (!SessionState.GetBool("exp-probe-sized",false))
            {
                SetSize(Sizes[index]); SessionState.SetBool("exp-probe-sized",true);
                next=EditorApplication.timeSinceStartup+4; sizeDeadline=EditorApplication.timeSinceStartup+20; return;
            }
            if ((Screen.width != Sizes[index].x || Screen.height != Sizes[index].y) && EditorApplication.timeSinceStartup < sizeDeadline) { next=EditorApplication.timeSinceStartup+1; return; }
            if (SessionState.GetBool("exp-probe-capture",false))
            {
                string path=SessionState.GetString("exp-probe-capture-path","");
                if (!File.Exists(path)) { next=EditorApplication.timeSinceStartup+1; return; }
                SessionState.SetBool("exp-probe-capture",false);
                SessionState.SetInt("exp-probe-index",index+1); SessionState.SetBool("exp-probe-sized",false);
                next=EditorApplication.timeSinceStartup+1; return;
            }
            string modeNow=SessionState.GetString("exp-probe-mode","");
            Canvas.ForceUpdateCanvases();
            // Dismiss presentation only; don't grant or delete saved offline rewards.
            var popup=GameObject.Find("WelcomeBackRewardPopup"); if(popup!=null) { popup.SetActive(false); next=EditorApplication.timeSinceStartup+1; return; }
            var label=Resources.FindObjectsOfTypeAll<Text>().FirstOrDefault(t=>t.gameObject.scene.IsValid() && t.name=="AccountExpLabel");
            var slider=Resources.FindObjectsOfTypeAll<Slider>().FirstOrDefault(t=>t.gameObject.scene.IsValid() && t.name=="AccountExpSlider");
            var obstacles=Resources.FindObjectsOfTypeAll<RectTransform>().Where(r=>r.gameObject.scene.IsValid() && r.gameObject.activeInHierarchy && new[]{"OpenGacha","OpenDeckFormation","OpenSkillTree","DefenseWaveRenderContainer"}.Contains(r.name)).ToArray();
            bool pass=Screen.width==Sizes[index].x && Screen.height==Sizes[index].y && label!=null && slider!=null && label.gameObject.activeInHierarchy && label.text.Contains("EXP");
            string line="size="+Screen.width+"x"+Screen.height+" safe="+Screen.safeArea;
            if(label!=null)
            {
                Rect bounds=Bounds(label.rectTransform);
                pass &= Contains(Screen.safeArea,bounds);
                line+=" label="+label.text+" bounds="+bounds+" color="+label.color;
                foreach(var obstacle in obstacles)
                {
                    Rect ob=Bounds(obstacle); bool overlaps=bounds.Overlaps(ob);
                    pass &= !overlaps;
                    if(slider!=null)pass &= !Bounds(slider.GetComponent<RectTransform>()).Overlaps(ob);
                    line+=" "+obstacle.name+"="+ob+" overlap="+overlaps;
                }
                // A contrasting backing is required; white text on the white defense HUD is the baseline failure.
                pass &= label.color.r < .6f || label.GetComponentInParent<Image>()!=null;
            }
            line+=" RESULT="+(pass?"PASS":"FAIL");
            File.AppendAllText(Path.Combine(Out,modeNow+".txt"),line+"\n");
            if(index==Sizes.Length-1 && label!=null)
            {
                var ui=UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).FirstOrDefault(m=>m.GetType().Name=="AccountExperienceBarUI");
                var method=ui?.GetType().GetMethod("ApplyLayout",F);
                if(method!=null)
                {
                    foreach(var safe in new[]{new Rect(16,34,Screen.width-32,Screen.height-78),new Rect(30,60,Screen.width-60,Screen.height-120)})
                    {
                        method.Invoke(ui,new object[]{safe}); Canvas.ForceUpdateCanvases();
                        var bounds=Bounds(label.rectTransform); bool inside=Contains(safe,bounds);
                        File.AppendAllText(Path.Combine(Out,modeNow+".txt"),"SIMULATED_SAFE_AREA="+safe+" bounds="+bounds+" RESULT="+(inside?"PASS":"FAIL")+"\n");
                    }
                    method.Invoke(ui,new object[]{Screen.safeArea}); Canvas.ForceUpdateCanvases();
                }
            }
            string capture=Path.Combine(Out,modeNow+"-"+index+"-"+Screen.width+"x"+Screen.height+".png");
            ScreenCapture.CaptureScreenshot(capture);
            SessionState.SetString("exp-probe-capture-path",capture);
            SessionState.SetBool("exp-probe-capture",true);
            Debug.Log("[EXP-UI-PROBE] "+line);
            next=EditorApplication.timeSinceStartup+1;
        }
        catch(Exception e)
        {
            SessionState.SetBool("exp-probe-active",false);
            File.AppendAllText(Path.Combine(Out,SessionState.GetString("exp-probe-mode","error")+".txt"),"PROBE_ERROR "+e+"\n");
            Debug.LogException(e);
            EditorApplication.isPlaying=false;
        }
    }
    static Rect Bounds(RectTransform r)
    {
        var c=new Vector3[4];r.GetWorldCorners(c);
        return Rect.MinMaxRect(c.Min(v=>v.x),c.Min(v=>v.y),c.Max(v=>v.x),c.Max(v=>v.y));
    }
    static bool Contains(Rect outer,Rect inner) => inner.xMin>=outer.xMin-1 && inner.yMin>=outer.yMin-1 && inner.xMax<=outer.xMax+1 && inner.yMax<=outer.yMax+1;
    static void SetSize(Vector2Int size)
    {
        var assembly=typeof(Editor).Assembly;
        var sizesType=assembly.GetType("UnityEditor.GameViewSizes");
        var single=typeof(ScriptableSingleton<>).MakeGenericType(sizesType);
        var instance=single.GetProperty("instance",F).GetValue(null);
        var group=sizesType.GetProperty("currentGroup",F).GetValue(instance);
        var sizeType=assembly.GetType("UnityEditor.GameViewSize");
        var enumType=assembly.GetType("UnityEditor.GameViewSizeType");
        var entry=Activator.CreateInstance(sizeType,F,null,new object[]{Enum.Parse(enumType,"FixedResolution"),size.x,size.y,"EXP-QA-"+size.x+"x"+size.y},null);
        int index=(int)group.GetType().GetMethod("GetTotalCount",F).Invoke(group,null);
        group.GetType().GetMethod("AddCustomSize",F).Invoke(group,new[]{entry});
        var viewType=assembly.GetType("UnityEditor.GameView");
        var view=EditorWindow.GetWindow(viewType);
        viewType.GetMethod("SizeSelectionCallback",F).Invoke(view,new object[]{index,null});
        var min=viewType.GetProperty("minScale",F);
        var snap=viewType.GetMethod("SnapZoom",F);
        if(min!=null && snap!=null)snap.Invoke(view,new[]{min.GetValue(view)});
        view.Repaint();
    }
}
