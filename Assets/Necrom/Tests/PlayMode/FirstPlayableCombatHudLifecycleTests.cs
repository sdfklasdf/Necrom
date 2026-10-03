using System;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Necrom.FirstPlayable.Tests
{
    public sealed class FirstPlayableCombatHudLifecycleTests
    {
        private const string RuntimeNs = "Necrom.FirstPlayable.Runtime.";
        private static object _state;
        private static ScriptableObject _fontAsset;
        private static Material _fontMaterial;
        private static Texture2D _fontAtlas;

        [UnityTest]
        public IEnumerator LifecycleOwnsOneOverlayAndCleansUpAcrossShutdownAndReentry()
        {
            var root = NewHudRoot();
            var safeArea = root.transform.Find("SafeArea") as RectTransform;
            Assert.That(safeArea, Is.Not.Null);

            var lifecycleType = RequireType(RuntimeNs + "FirstPlayableCombatHudLifecycle");
            var lifecycle = root.AddComponent(lifecycleType);
            var dependencies = NewDependencies();

            Invoke(
                lifecycle,
                "Initialize",
                dependencies.StateSource,
                safeArea,
                dependencies.DesignContract,
                dependencies.CopyProvider,
                dependencies.FontProvider);

            Assert.That(ReadBool(lifecycle, "IsInitialized"), Is.True);
            Assert.That(Read(lifecycle, "OverlayHost"), Is.Null);

            Invoke(lifecycle, "Refresh");
            yield return null;

            var firstOverlay = (GameObject)Read(lifecycle, "OverlayHost");
            Assert.That(firstOverlay, Is.Not.Null);
            Assert.That(firstOverlay.name, Is.EqualTo("FirstPlayableCombatHudOverlayCanvas"));

            Invoke(lifecycle, "Refresh");
            yield return null;
            Assert.That(Read(lifecycle, "OverlayHost"), Is.SameAs(firstOverlay));

            var duplicate = Assert.Throws<TargetInvocationException>(() =>
                Invoke(
                    lifecycle,
                    "Initialize",
                    dependencies.StateSource,
                    safeArea,
                    dependencies.DesignContract,
                    dependencies.CopyProvider,
                    dependencies.FontProvider));
            Assert.That(duplicate.InnerException, Is.TypeOf<InvalidOperationException>());
            Assert.That(Read(lifecycle, "OverlayHost"), Is.SameAs(firstOverlay));

            Invoke(lifecycle, "Shutdown");
            Assert.That(ReadBool(lifecycle, "IsInitialized"), Is.False);
            Assert.That(Read(lifecycle, "OverlayHost"), Is.Null);
            yield return null;
            Assert.That(firstOverlay == null, Is.True);

            // Shutdown is deliberately idempotent so scene teardown/restart
            // paths can call it safely more than once.
            Assert.DoesNotThrow(() => Invoke(lifecycle, "Shutdown"));

            dependencies = NewDependencies();
            Invoke(
                lifecycle,
                "Initialize",
                dependencies.StateSource,
                safeArea,
                dependencies.DesignContract,
                dependencies.CopyProvider,
                dependencies.FontProvider);
            Invoke(lifecycle, "Refresh");
            yield return null;

            var secondOverlay = (GameObject)Read(lifecycle, "OverlayHost");
            Assert.That(secondOverlay, Is.Not.Null);
            Assert.That(secondOverlay, Is.Not.SameAs(firstOverlay));

            UnityEngine.Object.Destroy(root);
            yield return null;
            Assert.That(secondOverlay == null, Is.True);
        }

        [UnityTest]
        public IEnumerator InvalidSceneBindingFailsBeforeOverlayOwnership()
        {
            var root = new GameObject("HudLifecycleInvalidRoot", typeof(RectTransform));
            var safeArea = NewRect(root.transform, "SafeArea");
            NewRect(safeArea, "TargetStatusReadabilityZone");
            NewRect(safeArea, "RaiseActionStatusReadabilityZone");
            // ArmyStatusReadabilityZone intentionally absent.
            NewRect(safeArea, "ProtectedCombatReadabilityZone");

            var lifecycleType = RequireType(RuntimeNs + "FirstPlayableCombatHudLifecycle");
            var lifecycle = root.AddComponent(lifecycleType);
            var dependencies = NewDependencies();

            var failure = Assert.Throws<TargetInvocationException>(() =>
                Invoke(
                    lifecycle,
                    "Initialize",
                    dependencies.StateSource,
                    safeArea,
                    dependencies.DesignContract,
                    dependencies.CopyProvider,
                    dependencies.FontProvider));

            Assert.That(failure.InnerException, Is.TypeOf<InvalidOperationException>());
            Assert.That(ReadBool(lifecycle, "IsInitialized"), Is.False);
            Assert.That(Read(lifecycle, "OverlayHost"), Is.Null);

            UnityEngine.Object.Destroy(root);
            yield return null;
        }

        private static Dependencies NewDependencies()
        {
            _state = NewMinimalState();

            var stateAdapterType = RequireType(RuntimeNs + "FirstPlayableCombatHudStateSourceAdapter");
            var stateDelegateType = stateAdapterType.GetConstructors().Single()
                .GetParameters()[0].ParameterType;
            var stateSource = Activator.CreateInstance(
                stateAdapterType,
                BuildZeroDelegate(stateDelegateType, nameof(CaptureStateObject)));

            var contract = RequireType(RuntimeNs + "FirstPlayableCombatHudVerifiedDesignContract")
                .GetMethod("Create", BindingFlags.Public | BindingFlags.Static)
                .Invoke(null, null);

            var copyAdapterType = RequireType(RuntimeNs + "FirstPlayableCombatHudCopyProviderAdapter");
            var copyDelegateType = copyAdapterType.GetConstructors().Single()
                .GetParameters()[0].ParameterType;
            var copyProvider = Activator.CreateInstance(
                copyAdapterType,
                BuildUnaryDelegate(copyDelegateType, nameof(ResolveCopyObject)));

            var fontAdapterType = RequireType(RuntimeNs + "FirstPlayableCombatHudFontProviderAdapter");
            var fontDelegateType = fontAdapterType.GetConstructors().Single()
                .GetParameters()[0].ParameterType;
            var fontProvider = Activator.CreateInstance(
                fontAdapterType,
                BuildZeroDelegate(fontDelegateType, nameof(ResolveFontObject)));

            return new Dependencies(
                stateSource,
                contract,
                copyProvider,
                fontProvider);
        }

        private static GameObject NewHudRoot()
        {
            var root = new GameObject("HudLifecycleRoot", typeof(RectTransform));
            var rootRect = (RectTransform)root.transform;
            rootRect.sizeDelta = new Vector2(390f, 844f);

            var safe = NewRect(root.transform, "SafeArea");
            safe.anchorMin = new Vector2(0f, 34f / 844f);
            safe.anchorMax = new Vector2(1f, 810f / 844f);

            NewRect(safe, "TargetStatusReadabilityZone");
            NewRect(safe, "RaiseActionStatusReadabilityZone");
            NewRect(safe, "ArmyStatusReadabilityZone");
            NewRect(safe, "ProtectedCombatReadabilityZone");
            return root;
        }

        private static RectTransform NewRect(Transform parent, string name)
        {
            var rect = new GameObject(name, typeof(RectTransform))
                .GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return rect;
        }

        private static object NewMinimalState()
        {
            var slotType = RequireType(RuntimeNs + "FirstPlayableCombatHudFormationSlotState");
            var slots = Array.CreateInstance(slotType, 5);
            for (var i = 0; i < 5; i++)
            {
                slots.SetValue(
                    Activator.CreateInstance(
                        slotType,
                        i,
                        null,
                        null,
                        null,
                        false),
                    i);
            }

            return Activator.CreateInstance(
                RequireType(RuntimeNs + "FirstPlayableCombatHudState"),
                Enum.Parse(RequireType("Necrom.Core.Domain.BattlePhase"), "Ready"),
                0L,
                null,
                Enum.Parse(
                    RequireType(RuntimeNs + "FirstPlayableRaiseAvailabilityReason"),
                    "NoTarget"),
                null,
                slots,
                null,
                Enum.Parse(
                    RequireType(RuntimeNs + "FirstPlayableProofStatus"),
                    "None"),
                null);
        }

        private static object CaptureStateObject()
            => _state;

        private static object ResolveCopyObject(object key)
        {
            var keyText = key.ToString();
            return Activator.CreateInstance(
                RequireType(RuntimeNs + "FirstPlayableCombatHudCopy"),
                keyText,
                "Primary:" + keyText,
                "Secondary:" + keyText,
                "CTA:" + keyText);
        }

        private static object ResolveFontObject()
        {
            if (_fontAsset != null)
                return _fontAsset;

            var settingsType = RequireType("TMPro.TMP_Settings");
            var settingsField = settingsType.GetField(
                "s_Instance",
                BindingFlags.Static | BindingFlags.NonPublic);
            Assert.That(settingsField, Is.Not.Null);
            if (settingsField.GetValue(null) == null)
            {
                settingsField.SetValue(
                    null,
                    ScriptableObject.CreateInstance(settingsType));
            }

            var fontAssetType = RequireType("TMPro.TMP_FontAsset");
            _fontAsset = ScriptableObject.CreateInstance(fontAssetType);
            _fontAsset.name = "TEST-ONLY Lifecycle Font";
            var versionField = fontAssetType.GetField(
                "m_Version",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(versionField, Is.Not.Null);
            versionField.SetValue(_fontAsset, "1.1.0");

            var shader = Shader.Find("TextMeshPro/Distance Field");
            Assert.That(shader, Is.Not.Null);
            _fontAtlas = new Texture2D(2, 2);
            _fontMaterial = new Material(shader);
            _fontMaterial.mainTexture = _fontAtlas;

            fontAssetType.GetProperty("material")
                .SetValue(_fontAsset, _fontMaterial);
            fontAssetType.GetProperty("atlasTextures")
                .SetValue(_fontAsset, new[] { _fontAtlas });

            return _fontAsset;
        }

        private static Delegate BuildZeroDelegate(
            Type delegateType,
            string helperName)
        {
            var invoke = delegateType.GetMethod("Invoke");
            var helper = typeof(FirstPlayableCombatHudLifecycleTests)
                .GetMethod(helperName, BindingFlags.Static | BindingFlags.NonPublic);
            return Expression.Lambda(
                    delegateType,
                    Expression.Convert(
                        Expression.Call(helper),
                        invoke.ReturnType))
                .Compile();
        }

        private static Delegate BuildUnaryDelegate(
            Type delegateType,
            string helperName)
        {
            var invoke = delegateType.GetMethod("Invoke");
            var parameter = Expression.Parameter(
                invoke.GetParameters()[0].ParameterType,
                "value");
            var helper = typeof(FirstPlayableCombatHudLifecycleTests)
                .GetMethod(helperName, BindingFlags.Static | BindingFlags.NonPublic);
            var call = Expression.Call(
                helper,
                Expression.Convert(parameter, typeof(object)));
            return Expression.Lambda(
                    delegateType,
                    Expression.Convert(call, invoke.ReturnType),
                    parameter)
                .Compile();
        }

        private static object Invoke(
            object target,
            string method,
            params object[] args)
        {
            var candidate = target.GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .Single(m =>
                    m.Name == method &&
                    m.GetParameters().Length == args.Length);
            return candidate.Invoke(target, args);
        }

        private static object Read(object target, string property)
        {
            var info = target.GetType().GetProperty(
                property,
                BindingFlags.Instance | BindingFlags.Public);
            Assert.That(info, Is.Not.Null, property);
            return info.GetValue(target);
        }

        private static bool ReadBool(object target, string property)
            => Convert.ToBoolean(Read(target, property));

        private static Type RequireType(string fullName)
        {
            var type = AppDomain.CurrentDomain.GetAssemblies()
                .Select(assembly => assembly.GetType(fullName))
                .FirstOrDefault(candidate => candidate != null);
            Assert.That(type, Is.Not.Null, fullName + " must exist.");
            return type;
        }

        private sealed class Dependencies
        {
            public object StateSource { get; }
            public object DesignContract { get; }
            public object CopyProvider { get; }
            public object FontProvider { get; }

            public Dependencies(
                object stateSource,
                object designContract,
                object copyProvider,
                object fontProvider)
            {
                StateSource = stateSource;
                DesignContract = designContract;
                CopyProvider = copyProvider;
                FontProvider = fontProvider;
            }
        }
    }
}
