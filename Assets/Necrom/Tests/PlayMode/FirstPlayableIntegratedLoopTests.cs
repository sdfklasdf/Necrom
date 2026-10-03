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
            Assert.That(Balance,Is.EqualTo(14),"Player lethal result must reach the Soul bridge.");
            Assert.That(ResourceRevision,Is.EqualTo(1));
            Call("AdvanceCombat",3f);yield return null;
            Assert.That(Balance,Is.EqualTo(14));
            Click(RaiseButton);yield return null;
            Assert.That(Balance,Is.EqualTo(11));
            Click(NextButton);yield return null;
            Call("AdvanceCombat",3f);yield return null;
            Assert.That(Balance,Is.EqualTo(15),"Shared ally lethal pipeline must grant exactly once.");
            Call("AdvanceCombat",3f);yield return null;
            Assert.That(Balance,Is.EqualTo(15));
            Assert.That(ResourceRevision,Is.EqualTo(3));
        }
        [UnityTest] public IEnumerator NormalPointerClicksRaiseFiveAlliesWithoutOldProofHidingNextRaise()
        {
            for(int i=1;i<=5;i++)
            {
                Call("AdvanceCombat",3f);yield return null;
                Assert.That(Interactable(RaiseButton),Is.True,"New eligible source must remain actionable after prior proof.");
                Click(RaiseButton);yield return null;
                Assert.That(Allies,Is.EqualTo(i));
                Assert.That(Balance,Is.EqualTo(10+i));
                var revision=ResourceRevision;
                Click(RaiseButton);yield return null;
                Assert.That(Allies,Is.EqualTo(i),"Duplicate click must not create a second unit.");
                Assert.That(ResourceRevision,Is.EqualTo(revision));
                Click(NextButton);yield return null;
            }
            Call("AdvanceCombat",3f);yield return null;
            Assert.That(Allies,Is.EqualTo(5));
            Assert.That(Interactable(RaiseButton),Is.False,"Full Formation must block Raise before throwing/mutating.");
            var before=ResourceRevision;
            Click(RaiseButton);yield return null;
            Assert.That(Allies,Is.EqualTo(5));
            Assert.That(ResourceRevision,Is.EqualTo(before));
            Assert.That(Interactable(NextButton),Is.True,"Combat can continue with a full army.");
        }
        [UnityTest] public IEnumerator AutomaticUpdateCombatAndRaycastInputSurviveLifecycleReentry()
        {
            Call("SetAutomaticCombat",true);
            float deadline=Time.realtimeSinceStartup+8f;
            while(Prop(Get("Battle"),"Phase").ToString()!="Resolved" && Time.realtimeSinceStartup<deadline)
                yield return null;
            Assert.That(Prop(Get("Battle"),"Phase").ToString(),Is.EqualTo("Resolved"),"Actual Update combat must resolve without manual advancement.");
            Assert.That(Balance,Is.EqualTo(14));
            Click(RaiseButton);yield return null;
            Assert.That(Allies,Is.EqualTo(1));
            Assert.That(Balance,Is.EqualTo(11));
            Click(NextButton);
            deadline=Time.realtimeSinceStartup+8f;
            while(Prop(Get("Battle"),"Phase").ToString()!="Resolved" && Time.realtimeSinceStartup<deadline)
                yield return null;
            Assert.That(Prop(Get("Battle"),"Phase").ToString(),Is.EqualTo("Resolved"));
            var presentation=Prop(Get("HudBinding"),"LastPresentation");
            Assert.That(Prop(Prop(presentation,"Army"),"ContentKey").ToString(),Is.EqualTo("ArmyProofObserved"));
            Assert.That(Prop(Prop(presentation,"Raise"),"ContentKey").ToString(),Is.EqualTo("RaiseEligible"));
            Assert.That(Balance,Is.EqualTo(15));
            var revision=ResourceRevision;
            _game.gameObject.SetActive(false);yield return null;
            _game.gameObject.SetActive(true);yield return null;yield return null;
            Assert.That(ResourceRevision,Is.EqualTo(revision),"Re-entry cannot replay a defeat grant.");
            Assert.That(UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None)
                .Count(x=>x.name=="FirstPlayableCombatHudOverlayCanvas"),Is.EqualTo(1));
            Click(RaiseButton);yield return null;
            Assert.That(Allies,Is.EqualTo(2));
            Assert.That(Balance,Is.EqualTo(12));
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
