using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Cryptography;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Necrom.FirstPlayable.Tests
{
    public sealed class FirstPlayableCombatHudRendererFoundationTests
    {
        private const string RuntimeNs = "Necrom.FirstPlayable.Runtime.";
        private static ScriptableObject _fontAsset;
        private static Material _fixtureFontMaterial;
        private static Texture2D _fixtureFontAtlas;

        [UnityTest]
        public IEnumerator ConcreteRendererFoundationContractsExistAndMatchVerifiedArtifacts()
        {
            var rendererType = RequireType(RuntimeNs + "FirstPlayableCombatHudUnityView");
            var contractType = RequireType(RuntimeNs + "FirstPlayableCombatHudVerifiedDesignContract");
            Assert.That(RequireType(RuntimeNs + "IFirstPlayableCombatHudCopyProvider"), Is.Not.Null);
            Assert.That(RequireType(RuntimeNs + "IFirstPlayableCombatHudFontProvider"), Is.Not.Null);
            Assert.That(RequireType(RuntimeNs + "FirstPlayableCombatHudCopyProviderAdapter"), Is.Not.Null);
            Assert.That(RequireType(RuntimeNs + "FirstPlayableCombatHudFontProviderAdapter"), Is.Not.Null);
            Assert.That(rendererType.GetInterfaces().Any(i => i.FullName == RuntimeNs + "IFirstPlayableCombatHudView"), Is.True);

            var contract = contractType.GetMethod("Create", BindingFlags.Public | BindingFlags.Static)
                .Invoke(null, null);
            Assert.That(ReadString(contract, "FigmaFileKey"), Is.EqualTo("eXqKU1qHXsn52SJfIGltZo"));
            Assert.That(ReadInt(contract, "TokenCount"), Is.EqualTo(40));
            Assert.That(ReadInt(contract, "SemanticStateCount"), Is.EqualTo(14));

            var insufficient = GetState(contract, "RaiseInsufficientSoul");
            Assert.That(ReadString(insufficient, "ComponentSetId"), Is.EqualTo("11:63"));
            Assert.That(ReadString(insufficient, "VariantId"), Is.EqualTo("11:39"));
            var armyEmpty = GetState(contract, "ArmyEmpty");
            Assert.That(ReadString(armyEmpty, "ComponentSetId"), Is.EqualTo("11:88"));
            Assert.That(ReadString(armyEmpty, "VariantId"), Is.EqualTo("11:64"));

            var projectRoot = Directory.GetCurrentDirectory();
            Assert.That(ReadString(contract, "TokenArtifactSha256"),
                Is.EqualTo(Sha256(Path.Combine(projectRoot, "contracts", "design-tokens.figma.verified.json"))));
            Assert.That(ReadString(contract, "ComponentArtifactSha256"),
                Is.EqualTo(Sha256(Path.Combine(projectRoot, "contracts", "figma-hud-component-state-map.verified.json"))));
            yield return null;
        }

        [UnityTest]
        public IEnumerator RenderCreatesIsolatedOverlayMirrorsGeometryAndPreservesSourceZones()
        {
            var fixture = NewFixture();
            var before = Snapshot(fixture.SafeArea, fixture.Target, fixture.Raise, fixture.Army, fixture.Protected);
            var renderer = NewRenderer(copyMode: "fixture", fontAvailable: true);
            var presentation = NewPresentation("TargetActive", "RaiseEligible", "ArmyEmpty");

            Invoke(renderer, "Render", presentation, fixture.Binding);
            yield return null;

            AssertLayoutUnchanged(before, fixture.SafeArea, fixture.Target, fixture.Raise, fixture.Army, fixture.Protected);
            var overlayHost = (GameObject)Read(renderer, "OverlayHost");
            Assert.That(overlayHost, Is.Not.Null);
            Assert.That(overlayHost.transform.parent, Is.Null);
            Assert.That(overlayHost.GetComponent<Canvas>(), Is.Not.Null);
            Assert.That(overlayHost.GetComponent<Canvas>().renderMode, Is.EqualTo(RenderMode.ScreenSpaceOverlay));

            var safeMirror = overlayHost.transform.Find("SafeAreaMirror") as RectTransform;
            Assert.That(safeMirror, Is.Not.Null);
            AssertSameRectLayout(fixture.SafeArea, safeMirror);
            AssertSameRectLayout(fixture.Target, safeMirror.Find("TargetRenderContainer") as RectTransform);
            AssertSameRectLayout(fixture.Raise, safeMirror.Find("RaiseRenderContainer") as RectTransform);
            AssertSameRectLayout(fixture.Army, safeMirror.Find("ArmyRenderContainer") as RectTransform);
            AssertSameRectLayout(fixture.Protected, safeMirror.Find("ProtectedCombatReadabilityZoneMirror") as RectTransform);

            var childCount = safeMirror.childCount;
            Invoke(renderer, "Render", presentation, fixture.Binding);
            yield return null;
            Assert.That(safeMirror.childCount, Is.EqualTo(childCount));
            AssertSameRectLayout(fixture.Target, safeMirror.Find("TargetRenderContainer") as RectTransform);
            AssertLayoutUnchanged(before, fixture.SafeArea, fixture.Target, fixture.Raise, fixture.Army, fixture.Protected);

            Invoke(renderer, "Dispose");
            UnityEngine.Object.Destroy(fixture.Root);
            yield return null;
        }

        [UnityTest]
        public IEnumerator RenderMapsFourteenSemanticStatesAndCtaTruthWithoutFakeLoading()
        {
            var fixture = NewFixture();
            var renderer = NewRenderer(copyMode: "fixture", fontAvailable: true);
            var keys = new[]
            {
                "TargetNone","TargetActive","TargetDefeated",
                "RaiseNoTarget","RaiseTargetNotReady","RaiseSourceUnavailableOrConsumed",
                "RaiseInsufficientSoul","RaiseEligible","RaiseCommittedAwaitingProof","RaiseProofObserved",
                "ArmyEmpty","ArmyOwned","ArmyProofPending","ArmyProofObserved"
            };
            var contract = RequireType(RuntimeNs + "FirstPlayableCombatHudVerifiedDesignContract")
                .GetMethod("Create", BindingFlags.Public | BindingFlags.Static).Invoke(null, null);
            foreach (var key in keys)
                Assert.That(GetState(contract, key), Is.Not.Null, key);

            Invoke(renderer, "Render",
                NewPresentation("TargetActive", "RaiseEligible", "ArmyEmpty"), fixture.Binding);
            yield return null;
            Assert.That(Read(renderer, "LastRaiseCtaState").ToString(), Is.EqualTo("Default"));
            Assert.That(ReadBool(Read(renderer, "PrimaryCtaButton"), "interactable"), Is.True);
            Assert.That(((Component)Read(renderer, "PrimaryCtaButton")).gameObject.activeSelf, Is.True);

            Invoke(renderer, "Render",
                NewPresentation("TargetDefeated", "RaiseInsufficientSoul", "ArmyOwned"), fixture.Binding);
            yield return null;
            Assert.That(Read(renderer, "LastRaiseCtaState").ToString(), Is.EqualTo("Disabled"));
            Assert.That(ReadBool(Read(renderer, "PrimaryCtaButton"), "interactable"), Is.False);
            Assert.That(((Component)Read(renderer, "PrimaryCtaButton")).gameObject.activeSelf, Is.True);
            Assert.That(ReadTmpText(((GameObject)Read(renderer, "OverlayHost")).transform, "RaiseRenderContainer/PrimaryText"),
                Is.EqualTo("Primary:RaiseInsufficientSoul"));

            Invoke(renderer, "Render",
                NewPresentation("TargetNone", "RaiseNoTarget", "ArmyEmpty"), fixture.Binding);
            yield return null;
            Assert.That(Read(renderer, "LastRaiseCtaState").ToString(), Is.EqualTo("Hidden"));
            Assert.That(((Component)Read(renderer, "PrimaryCtaButton")).gameObject.activeSelf, Is.False);
            Assert.That(AllDescendants(((GameObject)Read(renderer, "OverlayHost")).transform)
                .Any(t => t.name.IndexOf("Loading", StringComparison.OrdinalIgnoreCase) >= 0 ||
                          t.name.IndexOf("Spinner", StringComparison.OrdinalIgnoreCase) >= 0), Is.False);

            Invoke(renderer, "Dispose");
            UnityEngine.Object.Destroy(fixture.Root);
            yield return null;
        }

        [UnityTest]
        public IEnumerator MissingCopyOrFontFailsBeforeAnyOverlayOrSourceMutation()
        {
            foreach (var mode in new[] { "missing-copy", "missing-font" })
            {
                var fixture = NewFixture();
                var before = Snapshot(fixture.SafeArea, fixture.Target, fixture.Raise, fixture.Army, fixture.Protected);
                var renderer = NewRenderer(copyMode: mode, fontAvailable: mode != "missing-font");
                var ex = Assert.Throws<TargetInvocationException>(() =>
                    Invoke(renderer, "Render",
                        NewPresentation("TargetActive", "RaiseEligible", "ArmyEmpty"), fixture.Binding));
                Assert.That(ex.InnerException, Is.TypeOf<InvalidOperationException>());
                Assert.That(Read(renderer, "OverlayHost"), Is.Null);
                AssertLayoutUnchanged(before, fixture.SafeArea, fixture.Target, fixture.Raise, fixture.Army, fixture.Protected);
                UnityEngine.Object.Destroy(fixture.Root);
            }
            yield return null;
        }

        private static object NewRenderer(string copyMode, bool fontAvailable)
        {
            var contract = RequireType(RuntimeNs + "FirstPlayableCombatHudVerifiedDesignContract")
                .GetMethod("Create", BindingFlags.Public | BindingFlags.Static).Invoke(null, null);
            var copyAdapterType = RequireType(RuntimeNs + "FirstPlayableCombatHudCopyProviderAdapter");
            var copyDelegateType = copyAdapterType.GetConstructors().Single().GetParameters()[0].ParameterType;
            _copyMode = copyMode;
            var copyProvider = Activator.CreateInstance(copyAdapterType,
                BuildUnaryDelegate(copyDelegateType, nameof(ResolveCopyObject)));

            var fontAdapterType = RequireType(RuntimeNs + "FirstPlayableCombatHudFontProviderAdapter");
            var fontDelegateType = fontAdapterType.GetConstructors().Single().GetParameters()[0].ParameterType;
            _fontAvailable = fontAvailable;
            var fontProvider = Activator.CreateInstance(fontAdapterType,
                BuildZeroDelegate(fontDelegateType, nameof(ResolveFontObject)));

            return Activator.CreateInstance(
                RequireType(RuntimeNs + "FirstPlayableCombatHudUnityView"),
                contract, copyProvider, fontProvider);
        }

        private static string _copyMode = "fixture";
        private static bool _fontAvailable = true;

        private static object ResolveCopyObject(object key)
        {
            if (_copyMode == "missing-copy") return null;
            var keyText = key.ToString();
            return Activator.CreateInstance(
                RequireType(RuntimeNs + "FirstPlayableCombatHudCopy"),
                "State:" + keyText,
                "Primary:" + keyText,
                "Secondary:" + keyText,
                "CTA:" + keyText);
        }

        private static object ResolveFontObject()
        {
            if (!_fontAvailable) return null;
            if (_fontAsset != null) return _fontAsset;

            var settingsType = RequireType("TMPro.TMP_Settings");
            var settingsField = settingsType.GetField(
                "s_Instance",
                BindingFlags.Static | BindingFlags.NonPublic);
            Assert.That(settingsField, Is.Not.Null);
            if (settingsField.GetValue(null) == null)
                settingsField.SetValue(null, ScriptableObject.CreateInstance(settingsType));

            var fontAssetType = RequireType("TMPro.TMP_FontAsset");
            _fontAsset = ScriptableObject.CreateInstance(fontAssetType);
            Assert.That(_fontAsset, Is.Not.Null);
            _fontAsset.name = "TEST-ONLY Unresolved Font Boundary Fixture";
            var versionField = fontAssetType.GetField(
                "m_Version",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(versionField, Is.Not.Null);
            versionField.SetValue(_fontAsset, "1.1.0");

            var shader = Shader.Find("UI/Default");
            Assert.That(shader, Is.Not.Null, "Built-in UI/Default shader is required for the test-only font fixture.");
            _fixtureFontAtlas = new Texture2D(2, 2);
            _fixtureFontAtlas.name = "TEST-ONLY Font Atlas";
            _fixtureFontMaterial = new Material(shader);
            _fixtureFontMaterial.name = "TEST-ONLY Font Material";
            _fixtureFontMaterial.mainTexture = _fixtureFontAtlas;

            var materialProperty = fontAssetType.GetProperty("material");
            var atlasTexturesProperty = fontAssetType.GetProperty("atlasTextures");
            Assert.That(materialProperty, Is.Not.Null);
            Assert.That(atlasTexturesProperty, Is.Not.Null);
            materialProperty.SetValue(_fontAsset, _fixtureFontMaterial);
            atlasTexturesProperty.SetValue(_fontAsset, new[] { _fixtureFontAtlas });

            return _fontAsset;
        }

        private static object NewPresentation(string targetKey, string raiseKey, string armyKey)
        {
            var keyType = RequireType(RuntimeNs + "FirstPlayableCombatHudContentKey");
            var targetSection = Activator.CreateInstance(
                RequireType(RuntimeNs + "FirstPlayableCombatHudTargetSection"),
                Enum.Parse(keyType, targetKey),
                Enum.Parse(RequireType("Necrom.Core.Domain.BattlePhase"), "Ready"),
                4L,
                null);

            var raiseReason = raiseKey == "RaiseEligible" ? "Eligible" :
                raiseKey == "RaiseInsufficientSoul" ? "InsufficientSoul" :
                raiseKey == "RaiseSourceUnavailableOrConsumed" ? "SourceUnavailableOrConsumed" :
                raiseKey == "RaiseTargetNotReady" ? "TargetNotRaiseReady" : "NoTarget";
            var raiseSection = Activator.CreateInstance(
                RequireType(RuntimeNs + "FirstPlayableCombatHudRaiseSection"),
                Enum.Parse(keyType, raiseKey),
                Enum.Parse(RequireType(RuntimeNs + "FirstPlayableRaiseAvailabilityReason"), raiseReason),
                null, null,
                Enum.Parse(RequireType(RuntimeNs + "FirstPlayableProofStatus"), "None"),
                null,
                false);

            var slotType = RequireType(RuntimeNs + "FirstPlayableCombatHudFormationSlotState");
            var slots = Array.CreateInstance(slotType, 5);
            var armySection = Activator.CreateInstance(
                RequireType(RuntimeNs + "FirstPlayableCombatHudArmySection"),
                Enum.Parse(keyType, armyKey),
                slots,
                Enum.Parse(RequireType(RuntimeNs + "FirstPlayableProofStatus"), "None"),
                null);

            return Activator.CreateInstance(
                RequireType(RuntimeNs + "FirstPlayableCombatHudPresentation"),
                targetSection, raiseSection, armySection);
        }

        private static Fixture NewFixture()
        {
            var root = new GameObject("RendererFoundationFixture", typeof(RectTransform));
            var rootRect = (RectTransform)root.transform;
            rootRect.sizeDelta = new Vector2(390f, 844f);
            var safe = NewRect(root.transform, "SafeArea", new Vector2(0f, 34f / 844f), new Vector2(1f, 810f / 844f));
            var target = NewRect(safe, "TargetStatusReadabilityZone", new Vector2(0.02f, 0.02f), new Vector2(0.30f, 0.10f));
            var raise = NewRect(safe, "RaiseActionStatusReadabilityZone", new Vector2(0.36f, 0.02f), new Vector2(0.64f, 0.10f));
            var army = NewRect(safe, "ArmyStatusReadabilityZone", new Vector2(0.70f, 0.02f), new Vector2(0.98f, 0.10f));
            var protectedZone = NewRect(safe, "ProtectedCombatReadabilityZone", new Vector2(0.05f, 0.28f), new Vector2(0.95f, 0.93f));
            var binding = RequireType(RuntimeNs + "FirstPlayableCombatHudZoneBinding")
                .GetMethod("Bind", BindingFlags.Public | BindingFlags.Static)
                .Invoke(null, new object[] { safe });
            return new Fixture(root, safe, target, raise, army, protectedZone, binding);
        }

        private static RectTransform NewRect(Transform parent, string name, Vector2 min, Vector2 max)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.localScale = Vector3.one;
            rect.localRotation = Quaternion.identity;
            return rect;
        }

        private static List<RectSnapshot> Snapshot(params RectTransform[] rects)
            => rects.Select(RectSnapshot.Capture).ToList();

        private static void AssertLayoutUnchanged(
            IList<RectSnapshot> expected,
            params RectTransform[] actual)
        {
            Assert.That(actual.Length, Is.EqualTo(expected.Count));
            for (var i = 0; i < actual.Length; i++)
                expected[i].AssertMatches(actual[i]);
        }

        private static void AssertSameRectLayout(RectTransform source, RectTransform mirror)
        {
            Assert.That(mirror, Is.Not.Null);
            Assert.That(mirror.anchorMin, Is.EqualTo(source.anchorMin));
            Assert.That(mirror.anchorMax, Is.EqualTo(source.anchorMax));
            Assert.That(mirror.pivot, Is.EqualTo(source.pivot));
            Assert.That(mirror.offsetMin, Is.EqualTo(source.offsetMin));
            Assert.That(mirror.offsetMax, Is.EqualTo(source.offsetMax));
        }

        private static object GetState(object contract, string key)
        {
            var keyType = RequireType(RuntimeNs + "FirstPlayableCombatHudContentKey");
            return Invoke(contract, "GetState", Enum.Parse(keyType, key));
        }

        private static string ReadTmpText(Transform root, string relativePath)
        {
            var t = root.Find("SafeAreaMirror/" + relativePath);
            Assert.That(t, Is.Not.Null, relativePath);
            var tmp = t.GetComponent(RequireType("TMPro.TMP_Text"));
            Assert.That(tmp, Is.Not.Null);
            return (string)tmp.GetType().GetProperty("text").GetValue(tmp);
        }

        private static IEnumerable<Transform> AllDescendants(Transform root)
        {
            foreach (Transform child in root)
            {
                yield return child;
                foreach (var nested in AllDescendants(child))
                    yield return nested;
            }
        }

        private static Delegate BuildZeroDelegate(Type delegateType, string helperName)
        {
            var invoke = delegateType.GetMethod("Invoke");
            var helper = typeof(FirstPlayableCombatHudRendererFoundationTests)
                .GetMethod(helperName, BindingFlags.Static | BindingFlags.NonPublic);
            return Expression.Lambda(delegateType,
                Expression.Convert(Expression.Call(helper), invoke.ReturnType)).Compile();
        }

        private static Delegate BuildUnaryDelegate(Type delegateType, string helperName)
        {
            var invoke = delegateType.GetMethod("Invoke");
            var parameter = Expression.Parameter(invoke.GetParameters()[0].ParameterType, "key");
            var helper = typeof(FirstPlayableCombatHudRendererFoundationTests)
                .GetMethod(helperName, BindingFlags.Static | BindingFlags.NonPublic);
            var call = Expression.Call(helper, Expression.Convert(parameter, typeof(object)));
            return Expression.Lambda(delegateType, Expression.Convert(call, invoke.ReturnType), parameter).Compile();
        }

        private static object Invoke(object target, string method, params object[] args)
        {
            var candidates = target.GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .Where(m => m.Name == method && m.GetParameters().Length == args.Length)
                .ToArray();
            Assert.That(candidates.Length, Is.EqualTo(1), target.GetType().Name + "." + method);
            return candidates[0].Invoke(target, args);
        }

        private static object Read(object target, string property)
        {
            if (target == null) return null;
            var info = target.GetType().GetProperty(property, BindingFlags.Instance | BindingFlags.Public);
            Assert.That(info, Is.Not.Null, target.GetType().Name + "." + property);
            return info.GetValue(target);
        }

        private static string ReadString(object target, string property) => (string)Read(target, property);
        private static int ReadInt(object target, string property) => Convert.ToInt32(Read(target, property));
        private static bool ReadBool(object target, string property) => Convert.ToBoolean(Read(target, property));

        private static string Sha256(string path)
        {
            using (var stream = File.OpenRead(path))
            using (var sha = SHA256.Create())
                return string.Concat(sha.ComputeHash(stream).Select(b => b.ToString("x2")));
        }

        private static Type RequireType(string fullName)
        {
            var type = AppDomain.CurrentDomain.GetAssemblies()
                .Select(a => a.GetType(fullName))
                .FirstOrDefault(t => t != null);
            Assert.That(type, Is.Not.Null, fullName + " must exist.");
            return type;
        }

        private sealed class RectSnapshot
        {
            public Transform Parent { get; private set; }
            public Vector2 AnchorMin { get; private set; }
            public Vector2 AnchorMax { get; private set; }
            public Vector2 Pivot { get; private set; }
            public Vector2 OffsetMin { get; private set; }
            public Vector2 OffsetMax { get; private set; }
            public Vector3 Scale { get; private set; }
            public Quaternion Rotation { get; private set; }

            public static RectSnapshot Capture(RectTransform rect) => new RectSnapshot
            {
                Parent = rect.parent,
                AnchorMin = rect.anchorMin,
                AnchorMax = rect.anchorMax,
                Pivot = rect.pivot,
                OffsetMin = rect.offsetMin,
                OffsetMax = rect.offsetMax,
                Scale = rect.localScale,
                Rotation = rect.localRotation
            };

            public void AssertMatches(RectTransform rect)
            {
                Assert.That(rect.parent, Is.SameAs(Parent));
                Assert.That(rect.anchorMin, Is.EqualTo(AnchorMin));
                Assert.That(rect.anchorMax, Is.EqualTo(AnchorMax));
                Assert.That(rect.pivot, Is.EqualTo(Pivot));
                Assert.That(rect.offsetMin, Is.EqualTo(OffsetMin));
                Assert.That(rect.offsetMax, Is.EqualTo(OffsetMax));
                Assert.That(rect.localScale, Is.EqualTo(Scale));
                Assert.That(rect.localRotation, Is.EqualTo(Rotation));
            }
        }

        private sealed class Fixture
        {
            public GameObject Root { get; }
            public RectTransform SafeArea { get; }
            public RectTransform Target { get; }
            public RectTransform Raise { get; }
            public RectTransform Army { get; }
            public RectTransform Protected { get; }
            public object Binding { get; }

            public Fixture(GameObject root, RectTransform safeArea, RectTransform target,
                RectTransform raise, RectTransform army, RectTransform protectedZone, object binding)
            {
                Root = root;
                SafeArea = safeArea;
                Target = target;
                Raise = raise;
                Army = army;
                Protected = protectedZone;
                Binding = binding;
            }
        }
    }
}
