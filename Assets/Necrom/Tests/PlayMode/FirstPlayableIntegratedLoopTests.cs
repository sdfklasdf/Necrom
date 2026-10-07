using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif
namespace Necrom.FirstPlayable.Tests
{
    public sealed class FirstPlayableIntegratedLoopTests
    {
        Component _game;
        [UnitySetUp] public IEnumerator OpenScene()
        {
#if UNITY_EDITOR
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode("Assets/Necrom/FirstPlayable/Scenes/FirstPlayable.unity",new LoadSceneParameters(LoadSceneMode.Single));
#else
            yield return SceneManager.LoadSceneAsync("FirstPlayable");
#endif
            yield return null;
            _game=UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
                .Single(x=>x.GetType().Name=="FirstPlayableGameplayComposition");
            Call("SetAutomaticCombat",false);
        }
        [UnityTearDown] public IEnumerator CloseScene()
        {
            if(_game!=null)UnityEngine.Object.Destroy(_game.gameObject);
            yield return null;
        }
        object Get(string name)=>_game.GetType().GetProperty(name).GetValue(_game);
        object Field(string name)=>_game.GetType().GetField(name,BindingFlags.NonPublic|BindingFlags.Instance).GetValue(_game);
        object Call(string name,params object[] args)=>_game.GetType().GetMethod(name).Invoke(_game,args);
        static object Prop(object obj,string name)=>obj.GetType().GetProperty(name).GetValue(obj);
        int Balance=>Convert.ToInt32(Prop(Field("_account"),"Balance"));
        long ResourceRevision=>Convert.ToInt64(Prop(Field("_account"),"Revision"));
        int Allies=>Convert.ToInt32(Prop(Get("Roster"),"ActiveCount"));
        GameObject RaiseButton=>((GameObject)Prop(Get("HudBinding"),"OverlayHost")).transform
            .Find("SafeAreaMirror/RaiseRenderContainer/PrimaryCta").gameObject;
        GameObject NextButton=>_game.transform.Find("SafeArea/CombatViewport/NextEncounter").gameObject;
        static bool Interactable(GameObject button)=>Convert.ToBoolean(Prop(button.GetComponent("Button"),"interactable"));
        static void Click(GameObject button)
        {
            if(!button.activeInHierarchy)return;
            Canvas.ForceUpdateCanvases();
            var rect=(RectTransform)button.transform;
            var pointer=new PointerEventData(EventSystem.current)
                {button=PointerEventData.InputButton.Left,
                 position=RectTransformUtility.WorldToScreenPoint(null,rect.TransformPoint(rect.rect.center))};
            var hits=new System.Collections.Generic.List<RaycastResult>();
            EventSystem.current.RaycastAll(pointer,hits);
            Assert.That(hits.Count,Is.GreaterThan(0),"Input must hit a rendered graphic.");
            var handler=ExecuteEvents.GetEventHandler<IPointerClickHandler>(hits[0].gameObject);
            Assert.That(handler,Is.SameAs(button),"Topmost graphic must route to the intended button.");
            ExecuteEvents.Execute(handler,pointer,ExecuteEvents.pointerDownHandler);
            ExecuteEvents.Execute(handler,pointer,ExecuteEvents.pointerUpHandler);
            ExecuteEvents.Execute(handler,pointer,ExecuteEvents.pointerClickHandler);
        }
        [UnityTest] public IEnumerator PlayerAndAllyDefeatsGrantOnceAcrossRepeatedAdvance()
        {
            Assert.That(Balance,Is.EqualTo(10));
            Call("AdvanceCombat",.5f);yield return null;
            Assert.That(Balance,Is.EqualTo(10),"Nonlethal hits cannot grant Soul.");

            Call("AdvanceCombat",3f);yield return null;
            Assert.That(Balance,Is.EqualTo(14),"First canonical threat defeat must reach the Soul bridge.");
            Assert.That(ResourceRevision,Is.EqualTo(1));
            Assert.That(Prop(Get("DefenseWave"),"Phase").ToString(),Is.EqualTo("Running"));

            Click(RaiseButton);yield return null;
            Assert.That(Balance,Is.EqualTo(11));
            Assert.That(Allies,Is.EqualTo(1));

            Call("AdvanceCombat",.5f);yield return null;
            Assert.That(Balance,Is.EqualTo(11),"Nonlethal allied contribution cannot grant Soul.");
            Call("AdvanceCombat",.5f);yield return null;
            Assert.That(Balance,Is.EqualTo(15),"Second threat defeat must grant exactly once.");
            Assert.That(Prop(Get("DefenseWave"),"Phase").ToString(),Is.EqualTo("Cleared"));

            Call("AdvanceCombat",3f);yield return null;
            Assert.That(Balance,Is.EqualTo(15),"Resolved wave cannot replay a defeat grant.");
            Assert.That(ResourceRevision,Is.EqualTo(3));
        }
        [UnityTest] public IEnumerator NormalPointerClicksRaiseFiveAlliesWithoutOldProofHidingNextRaise()
        {
            for(int i=1;i<=5;i++)
            {
                var beforeWaveRevision=ResourceRevision;
                var guard=0;
                while(!Interactable(RaiseButton) && guard++<20)
                {
                    Call("AdvanceCombat",.5f);yield return null;
                }
                Assert.That(Interactable(RaiseButton),Is.True,
                    "Each wave must expose one eligible defeated Raise candidate.");

                Click(RaiseButton);yield return null;
                Assert.That(Allies,Is.EqualTo(i));
                var afterRaiseRevision=ResourceRevision;
                Click(RaiseButton);yield return null;
                Assert.That(Allies,Is.EqualTo(i),"Duplicate click must not create a second unit.");
                Assert.That(ResourceRevision,Is.EqualTo(afterRaiseRevision));

                guard=0;
                while(Prop(Get("DefenseWave"),"Phase").ToString()!="Cleared" && guard++<20)
                {
                    Call("AdvanceCombat",.5f);yield return null;
                }
                Assert.That(Prop(Get("DefenseWave"),"Phase").ToString(),Is.EqualTo("Cleared"));
                Assert.That(Balance,Is.EqualTo(10+5*i),
                    "Two defeat grants minus one Raise spend must net +5 per canonical wave.");
                Assert.That(ResourceRevision,Is.EqualTo(beforeWaveRevision+3),
                    "Each canonical wave owns exactly two grants and one Raise spend.");
                Assert.That(Interactable(NextButton),Is.True);
                Click(NextButton);yield return null;
            }

            Assert.That(Allies,Is.EqualTo(5));
            var guardFull=0;
            while(Convert.ToInt32(Prop(Get("DefenseWave"),"ActiveEnemyCount"))==2 && guardFull++<20)
            {
                Call("AdvanceCombat",.5f);yield return null;
            }
            Assert.That(Interactable(RaiseButton),Is.False,
                "Full Formation must block Raise before throwing/mutating.");
            var before=ResourceRevision;
            Click(RaiseButton);yield return null;
            Assert.That(Allies,Is.EqualTo(5));
            Assert.That(ResourceRevision,Is.EqualTo(before));

            guardFull=0;
            while(Prop(Get("DefenseWave"),"Phase").ToString()!="Cleared" && guardFull++<20)
            {
                Call("AdvanceCombat",.5f);yield return null;
            }
            Assert.That(Interactable(NextButton),Is.True,
                "Combat can continue and clear a wave with a full army.");
        }
        [UnityTest] public IEnumerator AutomaticUpdateCombatAndRaycastInputSurviveLifecycleReentry()
        {
            Call("SetAutomaticCombat",true);
            float deadline=Time.realtimeSinceStartup+8f;
            while(!Interactable(RaiseButton) && Time.realtimeSinceStartup<deadline)
                yield return null;
            Assert.That(Interactable(RaiseButton),Is.True,
                "Actual Update combat must expose the first defeated threat before gate pressure expires.");
            Assert.That(Prop(Get("DefenseWave"),"Phase").ToString(),Is.EqualTo("Running"));
            Click(RaiseButton);yield return null;
            Assert.That(Allies,Is.EqualTo(1));
            Assert.That(Balance,Is.EqualTo(11));

            deadline=Time.realtimeSinceStartup+8f;
            while(Prop(Get("Battle"),"Phase").ToString()!="Resolved" && Time.realtimeSinceStartup<deadline)
                yield return null;
            Assert.That(Prop(Get("Battle"),"Phase").ToString(),Is.EqualTo("Resolved"),
                "Timely normal-input Raise must let automatic Update combat beat gate pressure.");
            Assert.That(Prop(Get("DefenseWave"),"Phase").ToString(),Is.EqualTo("Cleared"));
            Assert.That(Balance,Is.EqualTo(15));
            Click(NextButton);
            deadline=Time.realtimeSinceStartup+8f;
            while(Prop(Get("Battle"),"Phase").ToString()!="Resolved" && Time.realtimeSinceStartup<deadline)
                yield return null;
            Assert.That(Prop(Get("Battle"),"Phase").ToString(),Is.EqualTo("Resolved"));
            var presentation=Prop(Get("HudBinding"),"LastPresentation");
            Assert.That(Prop(Prop(presentation,"Army"),"ContentKey").ToString(),Is.EqualTo("ArmyProofObserved"));
            Assert.That(Prop(Prop(presentation,"Raise"),"ContentKey").ToString(),Is.EqualTo("RaiseEligible"));
            Assert.That(Balance,Is.EqualTo(23));
            var revision=ResourceRevision;
            _game.gameObject.SetActive(false);yield return null;
            _game.gameObject.SetActive(true);yield return null;yield return null;
            Assert.That(ResourceRevision,Is.EqualTo(revision),"Re-entry cannot replay a defeat grant.");
            Assert.That(UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None)
                .Count(x=>x.name=="FirstPlayableCombatHudOverlayCanvas"),Is.EqualTo(1));
            Click(RaiseButton);yield return null;
            Assert.That(Allies,Is.EqualTo(2));
            Assert.That(Balance,Is.EqualTo(20));
        }
        [UnityTest] public IEnumerator DisabledRaiseAndNextClicksCannotMutateRunningBattle()
        {
            yield return null;
            var revision=Prop(Get("Battle"),"Revision");
            Assert.That(Interactable(RaiseButton),Is.False);
            Assert.That(Interactable(NextButton),Is.False);
            Click(RaiseButton);Click(NextButton);yield return null;
            Assert.That(Allies,Is.EqualTo(0));
            Assert.That(Balance,Is.EqualTo(10));
            Assert.That(Prop(Get("Battle"),"Revision"),Is.EqualTo(revision));
        }
    }
}
