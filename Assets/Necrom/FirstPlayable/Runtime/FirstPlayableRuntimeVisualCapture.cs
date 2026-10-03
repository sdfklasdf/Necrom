using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

namespace Necrom.FirstPlayable.Runtime
{
    // Opt-in development-player evidence collector. Does not replace gameplay state.
    public sealed class FirstPlayableRuntimeVisualCapture : MonoBehaviour
    {
        static bool _running;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void StartCaptureIfRequested()
        {
            if(Environment.GetCommandLineArgs().Contains("--necro-visual-qa"))
                new GameObject("DevelopmentVisualCapture").AddComponent<FirstPlayableRuntimeVisualCapture>();
        }
        IEnumerator Start()
        {
            if(!Environment.GetCommandLineArgs().Contains("--necro-visual-qa")) { Destroy(gameObject); yield break; }
            if(_running) { Destroy(gameObject); yield break; }
            _running=true;
            DontDestroyOnLoad(gameObject);
            var root=Path.Combine(Application.dataPath,"../VisualEvidence");
            Directory.CreateDirectory(root);
            var run=DateTime.UtcNow.ToString("yyyyMMddTHHmmssfff")+"-"+Guid.NewGuid().ToString("N");
            var output=Path.Combine(root,run);
            Directory.CreateDirectory(output);
            File.WriteAllText(Path.Combine(root,"latest-run.txt"),run);
            File.WriteAllText(Path.Combine(output,"run.txt"),"run="+run+"\n"+"buildGUID="+Application.buildGUID+"\n"+"physicalDevice=NOT RUN\n");
            if(Environment.GetCommandLineArgs().Contains("--necro-native-input"))
            {
                yield return CaptureNativeInput(output);
                File.WriteAllText(Path.Combine(output,"complete.txt"),"NATIVE INPUT + AUTOMATIC UPDATE CAPTURE COMPLETE. Physical device NOT RUN.");
                Application.Quit(0);
                yield break;
            }
            foreach(var size in new[]{new Vector2Int(390,844),new Vector2Int(768,1024),new Vector2Int(360,640)})
            {
                Screen.SetResolution(size.x,size.y,FullScreenMode.Windowed);
                yield return new WaitForSecondsRealtime(.3f);
                yield return SceneManager.LoadSceneAsync("FirstPlayable");
                var game=UnityEngine.Object.FindFirstObjectByType<FirstPlayableGameplayComposition>();
                game.SetAutomaticCombat(false);
                yield return null;
                var actual=new Vector2(Screen.width,Screen.height);
                // Explicit simulated notch insets are QA inputs, separate from physical-device SafeArea.
                game.ConfigureViewport(actual,new Rect(0,actual.y*.04f,actual.x,actual.y*.92f));
                yield return Capture(game,output,size,"active");
                game.AdvanceCombat(3f);
                yield return Capture(game,output,size,"eligible");
                game.RaiseCurrentTarget();
                yield return Capture(game,output,size,"raised");
                var safeRect=game.transform.Find("SafeArea") as RectTransform;
                var min=safeRect.anchorMin; var max=safeRect.anchorMax;
                game.StartNextEncounter();
                if(safeRect.anchorMin!=min || safeRect.anchorMax!=max)
                    throw new InvalidOperationException("Restart changed configured SafeArea.");
                game.AdvanceCombat(.6f);
                yield return Capture(game,output,size,"proof");
            }
            File.WriteAllText(Path.Combine(output,"complete.txt"),"DEVELOPMENT PLAYER CAPTURE COMPLETE. Physical device NOT RUN.");
            Application.Quit(0);
        }
        int _requestSequence;
        IEnumerator CaptureNativeInput(string output)
        {
            var size=new Vector2Int(390,844);
            Screen.SetResolution(size.x,size.y,FullScreenMode.Windowed);
            yield return new WaitForSecondsRealtime(.3f);
            yield return SceneManager.LoadSceneAsync("FirstPlayable");
            var game=UnityEngine.Object.FindFirstObjectByType<FirstPlayableGameplayComposition>();
            // Automatic Update stays enabled. The external driver supplies real OS clicks.
            game.ConfigureViewport(new Vector2(Screen.width,Screen.height),
                new Rect(0,Screen.height*.04f,Screen.width,Screen.height*.92f));
            for(int i=1;i<=5;i++)
            {
                yield return WaitFor(()=>game.Battle.Phase==Necrom.Core.Domain.BattlePhase.Resolved,8f);
                game.HudBinding.RefreshNow();
                if(game.SoulBalance!=13+i)throw new InvalidOperationException("Defeat grant/previous spend mismatch.");
                if(!game.HudBinding.OverlayHost.transform.Find("SafeAreaMirror/RaiseRenderContainer/PrimaryCta")
                    .GetComponent<UnityEngine.UI.Button>().interactable)
                    throw new InvalidOperationException("Eligible Raise is not actionable.");
                if(i==2 && game.HudBinding.LastPresentation.Army.ContentKey!=FirstPlayableCombatHudContentKey.ArmyProofObserved)
                    throw new InvalidOperationException("First raised ally did not produce actual contribution.");
                yield return Capture(game,output,size,"native-"+i+"-eligible");
                var button=game.HudBinding.OverlayHost.transform.Find("SafeAreaMirror/RaiseRenderContainer/PrimaryCta") as RectTransform;
                RequestClick(output,button);
                yield return WaitFor(()=>game.Roster.ActiveCount==i,5f);
                if(game.SoulBalance!=10+i)throw new InvalidOperationException("Raise did not spend exactly once.");
                yield return Capture(game,output,size,"native-"+i+"-raised");
                RequestClick(output,game.transform.Find("SafeArea/CombatViewport/NextEncounter") as RectTransform);
                yield return WaitFor(()=>game.Battle.Phase==Necrom.Core.Domain.BattlePhase.Running,5f);
            }
            yield return WaitFor(()=>game.Battle.Phase==Necrom.Core.Domain.BattlePhase.Resolved,8f);
            game.HudBinding.RefreshNow();
            if(game.SoulBalance!=19 || game.Roster.ActiveCount!=5 ||
                game.HudBinding.OverlayHost.transform.Find("SafeAreaMirror/RaiseRenderContainer/PrimaryCta")
                    .GetComponent<UnityEngine.UI.Button>().interactable)
                throw new InvalidOperationException("Full army input boundary mismatch.");
            yield return Capture(game,output,size,"native-full-army");
        }
        IEnumerator WaitFor(Func<bool> condition,float seconds)
        {
            float end=Time.realtimeSinceStartup+seconds;
            while(!condition() && Time.realtimeSinceStartup<end)yield return null;
            if(!condition())throw new InvalidOperationException("Native-input evidence timed out.");
        }
        void RequestClick(string output,RectTransform button)
        {
            Canvas.ForceUpdateCanvases();
            var point=RectTransformUtility.WorldToScreenPoint(null,button.TransformPoint(button.rect.center));
            var sequence=++_requestSequence;
            var pending=Path.Combine(output,"input-request-"+sequence+".tmp");
            File.WriteAllText(pending,
                sequence+"|"+Mathf.RoundToInt(point.x)+"|"+Mathf.RoundToInt(point.y)+"|"+Screen.width+"|"+Screen.height);
            // Publish an immutable complete request by same-directory rename.
            File.Move(pending,Path.Combine(output,"input-request-"+sequence+".txt"));
        }
        IEnumerator Capture(FirstPlayableGameplayComposition game,string output,Vector2Int requested,string stage)
        {
            yield return null;
            yield return new WaitForEndOfFrame();
            if(Screen.width!=requested.x || Screen.height!=requested.y)
                throw new InvalidOperationException("Requested pixel resolution was not executed.");
            var prefix=Screen.width+"x"+Screen.height+"-"+stage;
            var tex=ScreenCapture.CaptureScreenshotAsTexture();
            File.WriteAllBytes(Path.Combine(output,prefix+".png"),tex.EncodeToPNG());
            Destroy(tex);
            var safe=game.transform.Find("SafeArea");
            var lines=new System.Collections.Generic.List<string>{
                "requested="+requested,"actual="+Screen.width+"x"+Screen.height,
                "safeAreaEvidence=SIMULATED_4_PERCENT_VERTICAL_INSETS",
                "soulBalance="+game.SoulBalance,
                "alliedCount="+game.Roster.ActiveCount,
                "overlayCount="+UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None)
                    .Count(x=>x.name=="FirstPlayableCombatHudOverlayCanvas"),
                "target="+game.HudBinding.LastPresentation.Target.ContentKey,
                "raise="+game.HudBinding.LastPresentation.Raise.ContentKey,
                "army="+game.HudBinding.LastPresentation.Army.ContentKey};
            foreach(var name in new[]{"TargetStatusReadabilityZone","RaiseActionStatusReadabilityZone","ArmyStatusReadabilityZone","ProtectedCombatReadabilityZone","CombatViewport"})
            {
                var rect=safe.Find(name) as RectTransform;
                var corners=new Vector3[4];rect.GetWorldCorners(corners);
                lines.Add(name+"="+string.Join(";",corners.Select(x=>x.ToString("F2"))));
            }
            foreach(var text in game.HudBinding.OverlayHost.GetComponentsInChildren<TMP_Text>())
            {
                text.ForceMeshUpdate();
                lines.Add(text.name+" font="+text.fontSize+" overflow="+text.isTextOverflowing+" content="+text.text);
            }
            File.WriteAllLines(Path.Combine(output,prefix+".txt"),lines);
        }
    }
}
