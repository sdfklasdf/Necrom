using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace Necrom.FirstPlayable.Tests
{
    public sealed class FirstPlayableQ3VisualTests
    {
        Component _game;

        [UnitySetUp]
        public IEnumerator OpenScene()
        {
#if UNITY_EDITOR
            yield return EditorSceneManager.LoadSceneAsyncInPlayMode(
                "Assets/Necrom/FirstPlayable/Scenes/FirstPlayable.unity",
                new LoadSceneParameters(LoadSceneMode.Single));
#else
            yield return SceneManager.LoadSceneAsync("FirstPlayable");
#endif
            yield return null;
            _game = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
                .Single(x => x.GetType().Name == "FirstPlayableGameplayComposition");
            Call("SetAutomaticCombat", false);
        }

        [UnityTearDown]
        public IEnumerator CloseScene()
        {
            if (_game != null) UnityEngine.Object.Destroy(_game.gameObject);
            yield return null;
        }

        object Get(string name) => _game.GetType().GetProperty(name).GetValue(_game);
        object Field(string name) => _game.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance).GetValue(_game);
        object Call(string name, params object[] args) => _game.GetType().GetMethod(name).Invoke(_game, args);
        static object Prop(object obj, string name) => obj.GetType().GetProperty(name).GetValue(obj);
        int Balance => Convert.ToInt32(Prop(Field("_account"), "Balance"));
        long ResourceRevision => Convert.ToInt64(Prop(Field("_account"), "Revision"));
        int Allies => Convert.ToInt32(Prop(Get("Roster"), "ActiveCount"));

        GameObject RaiseButton => ((GameObject)Prop(Get("HudBinding"), "OverlayHost")).transform
            .Find("SafeAreaMirror/RaiseRenderContainer/PrimaryCta").gameObject;

        GameObject NextButton => _game.transform.Find("SafeArea/CombatViewport/NextEncounter").gameObject;

        static bool Interactable(GameObject button)
            => Convert.ToBoolean(Prop(button.GetComponent("Button"), "interactable"));

        static void Click(GameObject button)
        {
            if (!button.activeInHierarchy) return;
            Canvas.ForceUpdateCanvases();
            var rect = (RectTransform)button.transform;
            var pointer = new PointerEventData(EventSystem.current)
            {
                button = PointerEventData.InputButton.Left,
                position = RectTransformUtility.WorldToScreenPoint(null, rect.TransformPoint(rect.rect.center))
            };
            var hits = new System.Collections.Generic.List<RaycastResult>();
            EventSystem.current.RaycastAll(pointer, hits);
            Assert.That(hits.Count, Is.GreaterThan(0), "Input must hit a rendered graphic.");
            var handler = ExecuteEvents.GetEventHandler<IPointerClickHandler>(hits[0].gameObject);
            Assert.That(handler, Is.SameAs(button), "Topmost graphic must route to the intended button.");
            ExecuteEvents.Execute(handler, pointer, ExecuteEvents.pointerDownHandler);
            ExecuteEvents.Execute(handler, pointer, ExecuteEvents.pointerUpHandler);
            ExecuteEvents.Execute(handler, pointer, ExecuteEvents.pointerClickHandler);
        }

        Component Visual()
        {
            var visual = _game.GetComponents<MonoBehaviour>()
                .SingleOrDefault(x => x.GetType().Name == "FirstPlayableVisualPresentation");
            Assert.That(visual, Is.Not.Null, "Canonical Q3 sprite presentation must be present.");
            return visual;
        }

        static int Count(Component v, string key) => Convert.ToInt32(Prop(v, key));

        [UnityTest]
        public IEnumerator AuthoredPresentationProfileAndFiveReviewSfxAreLoadedAfterSceneReopen()
        {
            var v = Visual();
            Assert.That(Count(v, "LoadedArtCount"), Is.EqualTo(4));
            Assert.That(Convert.ToBoolean(Prop(v, "HasAuthoredMotion")), Is.True);
            Assert.That(Count(v, "LoadedAudioClipCount"), Is.EqualTo(5));
            Assert.That(UnityEngine.Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None).Length, Is.EqualTo(1),
                "Canonical review scene must have one active listener so runtime SFX are actually audible.");
            yield return null;
        }

        [UnityTest]
        public IEnumerator ProductionCandidateTexturesAreBoundToApprovedPath()
        {
            var v = Visual();
#if UNITY_EDITOR
            const string expectedPrefix = "Assets/Necrom/FirstPlayable/Art/Q3/ProductionCandidate/";
            foreach (var fieldName in new[] { "NecromancerArt", "GuardArt", "RaisedGuardArt", "BackgroundArt" })
            {
                var field = v.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.Instance);
                Assert.That(field, Is.Not.Null, fieldName + " field must exist.");
                var texture = (Texture2D)field.GetValue(v);
                Assert.That(texture, Is.Not.Null, fieldName + " texture must resolve.");
                Assert.That(AssetDatabase.GetAssetPath(texture), Does.StartWith(expectedPrefix),
                    fieldName + " must bind the approved Obsidian Soul ProductionCandidate asset.");
            }
#endif
            yield return null;
        }

        [UnityTest]
        public IEnumerator ProductionFontWeightsAndSemanticIconsAreBound()
        {
            var v = Visual();
            Assert.That(v, Is.Not.Null);

            var regular = _game.GetType().GetField("ReviewFont").GetValue(_game);
            var medium = _game.GetType().GetField("ReviewFontMedium").GetValue(_game);
            var bold = _game.GetType().GetField("ReviewFontBold").GetValue(_game);
            Assert.That(regular, Is.Not.Null);
            Assert.That(medium, Is.Not.Null);
            Assert.That(bold, Is.Not.Null);
#if UNITY_EDITOR
            foreach (var font in new[] { regular, medium, bold })
            {
                var source = Prop(font, "sourceFontFile") as UnityEngine.Object;
                Assert.That(source, Is.Not.Null);
                Assert.That(AssetDatabase.GetAssetPath(source),
                    Does.StartWith("Assets/Necrom/FirstPlayable/Fonts/Production/"));
            }
#endif

            var overlay = (GameObject)Prop(Get("HudBinding"), "OverlayHost");
            var target = overlay.transform.Find("SafeAreaMirror/TargetRenderContainer");
            var raise = overlay.transform.Find("SafeAreaMirror/RaiseRenderContainer");
            var army = overlay.transform.Find("SafeAreaMirror/ArmyRenderContainer");
            Assert.That(target, Is.Not.Null);
            Assert.That(raise, Is.Not.Null);
            Assert.That(army, Is.Not.Null);

            foreach (var section in new[] { target, raise, army })
            {
                var state = section.Find("StateKey").GetComponent("TextMeshProUGUI");
                var primary = section.Find("PrimaryText").GetComponent("TextMeshProUGUI");
                var secondary = section.Find("SecondaryText").GetComponent("TextMeshProUGUI");
                Assert.That(Prop(state, "font"), Is.SameAs(medium));
                Assert.That(Prop(primary, "font"), Is.SameAs(bold));
                Assert.That(Prop(secondary, "font"), Is.SameAs(regular));
                var icon = section.Find("StateAccent").GetComponent<UnityEngine.UI.Image>();
                Assert.That(icon.sprite, Is.Not.Null);
                Assert.That(icon.sprite.name, Does.StartWith("HUD Icon "));
            }

            var targetIcon = target.Find("StateAccent").GetComponent<UnityEngine.UI.Image>().sprite.name;
            var raiseIcon = raise.Find("StateAccent").GetComponent<UnityEngine.UI.Image>().sprite.name;
            var armyIcon = army.Find("StateAccent").GetComponent<UnityEngine.UI.Image>().sprite.name;
            Assert.That(targetIcon, Is.EqualTo("HUD Icon Combat"));
            Assert.That(raiseIcon, Is.EqualTo("HUD Icon Soul"));
            Assert.That(armyIcon, Is.EqualTo("HUD Icon Army"));
            yield return null;
        }

        [UnityTest]
        public IEnumerator ProductionMotionAndAudioCandidatesAreBoundWithDistinctSignatures()
        {
            var v = Visual();
#if UNITY_EDITOR
            const string audioPrefix = "Assets/Necrom/FirstPlayable/Audio/Q3ProductionCandidate/";
            foreach (var fieldName in new[]
                     {
                         "PlayerAttackSfx", "HitSfx", "DefeatSfx",
                         "RaiseSfx", "AlliedContributionSfx"
                     })
            {
                var field = v.GetType().GetField(fieldName, BindingFlags.Public | BindingFlags.Instance);
                Assert.That(field, Is.Not.Null, fieldName + " field must exist.");
                var clip = field.GetValue(v) as AudioClip;
                Assert.That(clip, Is.Not.Null, fieldName + " must resolve.");
                Assert.That(AssetDatabase.GetAssetPath(clip), Does.StartWith(audioPrefix),
                    fieldName + " must bind the production-candidate SFX path.");
            }
#endif
            var attack = (AnimationCurve)v.GetType().GetField("AttackLunge").GetValue(v);
            var hit = (AnimationCurve)v.GetType().GetField("HitFlash").GetValue(v);
            var defeat = (AnimationCurve)v.GetType().GetField("DefeatScaleY").GetValue(v);
            var raise = (AnimationCurve)v.GetType().GetField("RaiseScale").GetValue(v);
            var ally = (AnimationCurve)v.GetType().GetField("AlliedContributionScale").GetValue(v);

            Assert.That(attack.length, Is.GreaterThanOrEqualTo(5));
            Assert.That(hit.length, Is.GreaterThanOrEqualTo(4));
            Assert.That(defeat.length, Is.GreaterThanOrEqualTo(5));
            Assert.That(raise.length, Is.GreaterThanOrEqualTo(5));
            Assert.That(ally.length, Is.GreaterThanOrEqualTo(4));

            Assert.That(attack.Evaluate(.12f), Is.LessThan(0f),
                "Attack must show readable anticipation before the forward snap.");
            Assert.That(attack.Evaluate(.42f), Is.GreaterThan(1f),
                "Attack must peak as a decisive forward snap.");
            Assert.That(raise.Evaluate(.48f), Is.GreaterThan(1.15f),
                "Raise must visibly overshoot during emerald reform.");
            Assert.That(ally.Evaluate(.18f), Is.GreaterThan(1.08f),
                "Exact ally contribution needs a concise local pulse.");

            Assert.That(Convert.ToSingle(v.GetType().GetField("AttackDuration").GetValue(v)), Is.InRange(.20f, .28f));
            Assert.That(Convert.ToSingle(v.GetType().GetField("HitDuration").GetValue(v)), Is.InRange(.13f, .20f));
            Assert.That(Convert.ToSingle(v.GetType().GetField("DefeatDuration").GetValue(v)), Is.InRange(.42f, .60f));
            Assert.That(Convert.ToSingle(v.GetType().GetField("RaiseDuration").GetValue(v)), Is.InRange(.55f, .75f));
            Assert.That(Convert.ToSingle(v.GetType().GetField("AlliedContributionDuration").GetValue(v)), Is.InRange(.16f, .28f));

            var master = Convert.ToSingle(v.GetType().GetField("SfxMasterVolume").GetValue(v));
            var attackGain = Convert.ToSingle(v.GetType().GetField("PlayerAttackSfxGain").GetValue(v));
            var hitGain = Convert.ToSingle(v.GetType().GetField("HitSfxGain").GetValue(v));
            var defeatGain = Convert.ToSingle(v.GetType().GetField("DefeatSfxGain").GetValue(v));
            var raiseGain = Convert.ToSingle(v.GetType().GetField("RaiseSfxGain").GetValue(v));
            var allyGain = Convert.ToSingle(v.GetType().GetField("AlliedContributionSfxGain").GetValue(v));
            Assert.That(master, Is.InRange(.20f, .50f));
            Assert.That(raiseGain, Is.GreaterThan(attackGain));
            Assert.That(defeatGain, Is.GreaterThan(hitGain));
            Assert.That(allyGain, Is.LessThanOrEqualTo(.50f),
                "Repeated allied contribution must stay fatigue-safe under polyphony.");
            yield return null;
        }

        [UnityTest]
        public IEnumerator ActualDamageCreatesAttackHitDefeatFeedbackWithoutDuplicateReplay()
        {
            var v = Visual();
            Assert.That(Count(v, "LoadedArtCount"), Is.EqualTo(4), "All four real textures must resolve after scene reopen.");

            Call("AdvanceCombat", .5f);
            yield return null;
            Assert.That(Count(v, "PlayerAttackCueCount"), Is.EqualTo(1));
            Assert.That(Count(v, "HitCueCount"), Is.EqualTo(1));
            Assert.That(Count(v, "DefeatCueCount"), Is.EqualTo(0));
            Assert.That(Count(v, "ReviewSfxPlaybackCount"), Is.GreaterThanOrEqualTo(2),
                "Actual attack/hit events must request actual review audio playback.");

            Call("AdvanceCombat", 3f);
            yield return null;
            Assert.That(Count(v, "PlayerAttackCueCount"), Is.EqualTo(5));
            Assert.That(Count(v, "HitCueCount"), Is.EqualTo(5));
            Assert.That(Count(v, "DefeatCueCount"), Is.EqualTo(1));
            var sfxAfterResolve = Count(v, "ReviewSfxPlaybackCount");

            Call("AdvanceCombat", 3f);
            yield return null;
            Assert.That(Count(v, "HitCueCount"), Is.EqualTo(5), "Resolved battle cannot replay damage visuals.");
            Assert.That(Count(v, "ReviewSfxPlaybackCount"), Is.EqualTo(sfxAfterResolve),
                "Resolved battle cannot replay review SFX.");
        }

        [UnityTest]
        public IEnumerator NormalRaiseInputCreatesAlliedArtAndExactContributionFeedback()
        {
            var v = Visual();
            Call("AdvanceCombat", 3f);
            yield return null;

            Click(RaiseButton);
            yield return null;
            Assert.That(Count(v, "RaiseCueCount"), Is.EqualTo(1));
            Assert.That(Count(v, "VisibleAllyCount"), Is.EqualTo(1));
            Assert.That(Count(v, "ReviewSfxPlaybackCount"), Is.GreaterThan(0));

            Click(RaiseButton);
            yield return null;
            Assert.That(Count(v, "RaiseCueCount"), Is.EqualTo(1), "Consumed source cannot replay Raise visuals.");

            Click(NextButton);
            yield return null;
            Call("AdvanceCombat", .5f);
            yield return null;
            Assert.That(Count(v, "AlliedContributionCueCount"), Is.GreaterThan(0),
                "Cue must be driven by actual allied damage.");
            Assert.That(Count(v, "VisibleAllyCount"), Is.EqualTo(1));
            Assert.That(Count(v, "DefeatCueCount"), Is.EqualTo(1),
                "Nonlethal contribution cannot masquerade as defeat.");
            Assert.That((string)Prop(v, "LastContributionUnitId"), Is.Not.Empty);
        }

        [UnityTest]
        public IEnumerator ShortPortraitKeepsUnitArtInsideProtectedCombat()
        {
            Visual();
            Call("ConfigureViewport", new Vector2(360, 640), new Rect(0, 25.6f, 360, 588.8f));
            yield return null;
            Canvas.ForceUpdateCanvases();

            var zone = _game.transform.Find("SafeArea/ProtectedCombatReadabilityZone") as RectTransform;
            var bounds = new Vector3[4];
            zone.GetWorldCorners(bounds);
            var art = _game.transform.Find("SafeArea/CombatViewport/Q3VisualPresentation/Guard") as RectTransform;
            var points = new Vector3[4];
            art.GetWorldCorners(points);
            foreach (var point in points)
            {
                Assert.That(point.y, Is.InRange(bounds[0].y - .1f, bounds[2].y + .1f),
                    "Short portrait Guard must remain protected.");
                Assert.That(point.x, Is.InRange(bounds[0].x - .1f, bounds[2].x + .1f));
            }
        }

        [UnityTest]
        public IEnumerator ShortPortraitRaisedAllyHasReadableFloorAndRemainsProtected()
        {
            Visual();
            Call("ConfigureViewport", new Vector2(360, 640), new Rect(0, 25.6f, 360, 588.8f));
            Call("AdvanceCombat", 3f);
            yield return null;
            Click(RaiseButton);
            yield return new WaitForSecondsRealtime(.5f);
            Canvas.ForceUpdateCanvases();

            var ally = _game.transform.Find("SafeArea/CombatViewport/Q3VisualPresentation/RaisedGuardSlot0") as RectTransform;
            var allyCorners = new Vector3[4];
            ally.GetWorldCorners(allyCorners);
            var pixelHeight = allyCorners[1].y - allyCorners[0].y;
            Assert.That(pixelHeight, Is.GreaterThanOrEqualTo(40f),
                "360x640 raised ally must not collapse to the previous ~29px-tall silhouette.");

            var zone = _game.transform.Find("SafeArea/ProtectedCombatReadabilityZone") as RectTransform;
            var bounds = new Vector3[4];
            zone.GetWorldCorners(bounds);
            foreach (var point in allyCorners)
            {
                Assert.That(point.y, Is.InRange(bounds[0].y - .1f, bounds[2].y + .1f));
                Assert.That(point.x, Is.InRange(bounds[0].x - .1f, bounds[2].x + .1f));
            }
        }

        [UnityTest]
        public IEnumerator CanonicalPlayerFacingCopyFitsAllVerifiedTypographyRows()
        {
            var overlay = (GameObject)Prop(Get("HudBinding"), "OverlayHost");
            foreach (var t in overlay.GetComponentsInChildren<MonoBehaviour>()
                         .Where(x => x.GetType().Name == "TextMeshProUGUI"))
            {
                t.GetType().GetMethod("ForceMeshUpdate").Invoke(t, new object[] { false, false });
                Assert.That(Convert.ToBoolean(Prop(t, "isTextOverflowing")), Is.False,
                    "Canonical row overflows: " + t.name);
            }
            yield return null;
        }

        [UnityTest]
        public IEnumerator ReentryPreservesOnePresentationAndNonBlockingProtectedCombatArt()
        {
            var v = Visual();
            _game.gameObject.SetActive(false);
            yield return null;
            _game.gameObject.SetActive(true);
            yield return null;
            yield return null;

            Assert.That(_game.transform.Find("SafeArea/CombatViewport")
                .GetComponentsInChildren<Transform>(true)
                .Count(x => x.name == "Q3VisualPresentation"), Is.EqualTo(1));
            Assert.That(UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None)
                .Count(x => x.name == "FirstPlayableCombatHudOverlayCanvas"), Is.EqualTo(1));
            Assert.That(Count(v, "LoadedArtCount"), Is.EqualTo(4));

            var graphics = _game.transform.Find("SafeArea/CombatViewport/Q3VisualPresentation")
                .GetComponentsInChildren<Component>(true)
                .Where(x => x.GetType().Name == "RawImage");
            Assert.That(graphics.Count(), Is.GreaterThanOrEqualTo(3));
            foreach (var g in graphics)
                Assert.That(Convert.ToBoolean(Prop(g, "raycastTarget")), Is.False,
                    "Art cannot intercept normal input.");

            Call("AdvanceCombat", 3f);
            yield return null;
            Click(RaiseButton);
            yield return null;
            Assert.That(Allies, Is.EqualTo(1));
        }
    }
}
