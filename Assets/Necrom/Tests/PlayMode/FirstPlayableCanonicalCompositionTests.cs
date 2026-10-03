using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif
namespace Necrom.FirstPlayable.Tests
{
    public sealed class FirstPlayableCanonicalCompositionTests
    {
        const string ScenePath = "Assets/Necrom/FirstPlayable/Scenes/FirstPlayable.unity";
        Component _game;
        [UnitySetUp] public IEnumerator OpenCanonicalScene()
        {
            Assert.That(File.Exists(ScenePath), Is.True, "Canonical gameplay scene must be a saved project artifact.");
#if UNITY_EDITOR
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode(ScenePath, new LoadSceneParameters(LoadSceneMode.Single));
#else
            yield return SceneManager.LoadSceneAsync("FirstPlayable");
#endif
            yield return null;
            _game = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
                .Single(x => x.GetType().Name == "FirstPlayableGameplayComposition");
            Assert.That(Get("IsInitialized"), Is.EqualTo(true));
        }
        [UnityTearDown] public IEnumerator CloseScene()
        {
            if (_game != null) UnityEngine.Object.Destroy(_game.gameObject);
            yield return null;
        }
        object Get(string name) => _game.GetType().GetProperty(name).GetValue(_game);
        object Call(string name, params object[] args) => _game.GetType().GetMethod(name).Invoke(_game,args);
        static object Prop(object obj,string name) => obj.GetType().GetProperty(name).GetValue(obj);
        void Keys(string target,string raise,string army)
        {
            var p = Prop(Get("HudBinding"), "LastPresentation");
            Assert.That(Prop(Prop(p,"Target"),"ContentKey").ToString(),Is.EqualTo(target));
            Assert.That(Prop(Prop(p,"Raise"),"ContentKey").ToString(),Is.EqualTo(raise));
            Assert.That(Prop(Prop(p,"Army"),"ContentKey").ToString(),Is.EqualTo(army));
        }
        [UnityTest] public IEnumerator NormalSceneRunsCombatRaiseAndRealAllyContribution()
        {
            Call("SetAutomaticCombat",false);
            Keys("TargetActive","RaiseTargetNotReady","ArmyEmpty");
            Call("AdvanceCombat",3f); yield return null;
            Keys("TargetDefeated","RaiseEligible","ArmyEmpty");
            var overlay = Prop(Get("HudBinding"),"OverlayHost");
            Call("RaiseCurrentTarget"); yield return null;
            Keys("TargetDefeated","RaiseCommittedAwaitingProof","ArmyProofPending");
            Assert.That(Convert.ToInt32(Prop(Get("Roster"),"ActiveCount")),Is.EqualTo(1));
            Call("StartNextEncounter"); Call("AdvanceCombat",0.6f); yield return null;
            Assert.That(Prop(Prop(Prop(Get("HudBinding"),"LastPresentation"),"Army"),"ContentKey").ToString(),
                Is.EqualTo("ArmyProofObserved"),"Proof must come from the real ally combat loop.");
            Assert.That(Prop(Get("HudBinding"),"OverlayHost"),Is.SameAs(overlay));
        }
        [UnityTest] public IEnumerator CanonicalDisableReentryAndDestroyLeaveOneOrZeroOverlay()
        {
            var binding = (Behaviour)Get("HudBinding");
            var old = (GameObject)Prop(binding,"OverlayHost");
            binding.enabled=false; yield return null;
            Assert.That(old == null,Is.True);
            binding.enabled=true; yield return null; yield return null;
            Assert.That(Prop(binding,"IsInitialized"),Is.EqualTo(true));
            Assert.That(UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None)
                .Count(x=>x.name=="FirstPlayableCombatHudOverlayCanvas"),Is.EqualTo(1));
            _game.gameObject.SetActive(false); yield return null;
            Assert.That(UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None)
                .Count(x=>x.name=="FirstPlayableCombatHudOverlayCanvas"),Is.EqualTo(0));
            _game.gameObject.SetActive(true); yield return null; yield return null;
            Assert.That(UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None)
                .Count(x=>x.name=="FirstPlayableCombatHudOverlayCanvas"),Is.EqualTo(1));
            UnityEngine.Object.Destroy(_game.gameObject); yield return null;
            Assert.That(UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None)
                .Count(x=>x.name=="FirstPlayableCombatHudOverlayCanvas"),Is.EqualTo(0));
        }
        [UnityTest] public IEnumerator CanonicalPanelsUseRoundedSurfaceAndSemanticStateAccent()
        {
            Call("SetAutomaticCombat",false);
            var overlay=(GameObject)Prop(Get("HudBinding"),"OverlayHost");
            var panel=overlay.transform.Find("SafeAreaMirror/TargetRenderContainer");
            Assert.That(Prop(panel.GetComponent("Image"),"sprite"),Is.Not.Null,"Figma radius18 must be rendered.");
            var accent=panel.Find("StateAccent");
            Assert.That(accent,Is.Not.Null,"Figma semantic dot must be visible.");
            var color=(Color)Prop(accent.GetComponent("Image"),"color");
            Assert.That(color.r,Is.EqualTo(24f/255f).Within(.001f));
            Assert.That(color.g,Is.EqualTo(100f/255f).Within(.001f));
            yield return null;
        }
        [UnityTest] public IEnumerator RaiseCtaDisabledAndEligibleHaveDistinctVerifiedVisuals()
        {
            Call("SetAutomaticCombat",false);
            var overlay=(GameObject)Prop(Get("HudBinding"),"OverlayHost");
            var cta=overlay.transform.Find("SafeAreaMirror/RaiseRenderContainer/PrimaryCta");
            var color=(Color)Prop(cta.GetComponent("Image"),"color");
            Assert.That(color.r,Is.EqualTo(77f/255f).Within(.001f));
            Assert.That(Prop(cta.GetComponent("CanvasGroup"),"alpha"),Is.EqualTo(.55f));
            Assert.That(Prop(cta.GetComponent("Button"),"interactable"),Is.EqualTo(false));
            Call("AdvanceCombat",3f);yield return null;
            color=(Color)Prop(cta.GetComponent("Image"),"color");
            Assert.That(color.g,Is.EqualTo(127f/255f).Within(.001f));
            Assert.That(Prop(cta.GetComponent("CanvasGroup"),"alpha"),Is.EqualTo(1f));
            Assert.That(Prop(cta.GetComponent("Button"),"interactable"),Is.EqualTo(true));
        }
        [UnityTest] public IEnumerator ConfiguredSafeAreaSurvivesNextEncounterRestart()
        {
            Call("SetAutomaticCombat",false);
            Call("ConfigureViewport",new Vector2(390,844),new Rect(0,33.76f,390,776.48f));
            var safe = _game.transform.Find("SafeArea") as RectTransform;
            var beforeMin=safe.anchorMin;var beforeMax=safe.anchorMax;
            Call("AdvanceCombat",3f);yield return null;
            Call("StartNextEncounter");yield return null;
            Assert.That(safe.anchorMin,Is.EqualTo(beforeMin));
            Assert.That(safe.anchorMax,Is.EqualTo(beforeMax));
        }
        [UnityTest] public IEnumerator PortraitSafeAreaKeepsHudZonesOutsideProtectedCombat()
        {
            Call("SetAutomaticCombat",false);
            var sizes = new[]{new Vector2(390,844),new Vector2(1080,2400),new Vector2(360,640)};
            foreach(var size in sizes)
            {
                var inset = size.y*0.04f;
                Call("ConfigureViewport",size,new Rect(0,inset,size.x,size.y-2*inset));
                yield return null;
                var overlay=(GameObject)Prop(Get("HudBinding"),"OverlayHost");
                Assert.That(Prop(overlay.GetComponent("CanvasScaler"),"scaleFactor"),
                    Is.EqualTo(Prop(_game.GetComponent("CanvasScaler"),"scaleFactor")));
                var safe = _game.transform.Find("SafeArea") as RectTransform;
                Assert.That(safe.anchorMin.y,Is.EqualTo(0.04f).Within(0.0001f));
                Assert.That(safe.anchorMax.y,Is.EqualTo(0.96f).Within(0.0001f));
                var combat = safe.Find("ProtectedCombatReadabilityZone") as RectTransform;
                foreach(var name in new[]{"TargetStatusReadabilityZone","RaiseActionStatusReadabilityZone","ArmyStatusReadabilityZone"})
                {
                    var zone = safe.Find(name) as RectTransform;
                    Assert.That(zone,Is.Not.Null);
                    Assert.That(zone.anchorMax.y,Is.LessThanOrEqualTo(combat.anchorMin.y));
                    Assert.That((zone.anchorMax.y-zone.anchorMin.y)*(size.y*0.92f*390f/size.x),Is.GreaterThan(100f));
                    Assert.That((zone.anchorMax.x-zone.anchorMin.x)*390f,Is.GreaterThan(300f));
                }
            }
        }
    }
}
