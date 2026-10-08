using System;
using System.Linq;
using Necrom.Core.Domain;
using EntityId = Necrom.Core.Domain.EntityId;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Necrom.FirstPlayable.Runtime
{
    public enum FirstPlayableCharacterVfxIdentity
    {
        None = 0,
        LeafBarrier = 1,
        StarArrow = 2,
        DewHeal = 3,
        PineSlash = 4,
        CloverWind = 5,
        ChestnutStar = 6,
        BerryBurst = 7,
        GrowthRingBarrier = 8,
        SeedlingCall = 9,
        FireflyTrail = 10,
        SkyBulwark = 11
    }

    [Serializable]
    public sealed class FirstPlayableCharacterVisualBinding
    {
        public string ArchetypeId;
        public Texture2D Art;
        public FirstPlayableCharacterVfxIdentity VfxIdentity;
        public Color Accent = Color.white;
    }

    // Q3 production-candidate art + authored presentation profile. No gameplay state adapter or fabricated contribution.
    [DisallowMultipleComponent]
    public sealed class FirstPlayableVisualPresentation : MonoBehaviour
    {
        public Texture2D NecromancerArt, GuardArt, RaisedGuardArt, BackgroundArt;
        [FormerlySerializedAs("ForestFriendVisuals")]
        public FirstPlayableCharacterVisualBinding[] CharacterVisuals =
            Array.Empty<FirstPlayableCharacterVisualBinding>();
        public int LoadedCharacterArtCount =>
            CharacterVisuals == null
                ? 0
                : CharacterVisuals.Count(x =>
                    x != null &&
                    !string.IsNullOrWhiteSpace(x.ArchetypeId) &&
                    x.Art != null);
        public AnimationCurve AttackLunge, HitFlash, DefeatScaleY, RaiseScale, AlliedContributionScale;
        public float AttackDuration = .24f, HitDuration = .16f, DefeatDuration = .50f, RaiseDuration = .66f, AlliedContributionDuration = .20f;

        [FormerlySerializedAs("ReviewSfxVolume")]
        public float SfxMasterVolume = .34f;
        public float PlayerAttackSfxGain = .72f, HitSfxGain = .48f, DefeatSfxGain = .82f, RaiseSfxGain = 1f, AlliedContributionSfxGain = .38f;
        public AudioClip PlayerAttackSfx, HitSfx, DefeatSfx, RaiseSfx, AlliedContributionSfx;

        public int LoadedArtCount => (NecromancerArt ? 1 : 0) + (GuardArt ? 1 : 0) +
                                     (RaisedGuardArt ? 1 : 0) + (BackgroundArt ? 1 : 0);
        public int LoadedAudioClipCount => (PlayerAttackSfx ? 1 : 0) + (HitSfx ? 1 : 0) + (DefeatSfx ? 1 : 0) +
                                           (RaiseSfx ? 1 : 0) + (AlliedContributionSfx ? 1 : 0);
        public bool HasAuthoredMotion => AttackLunge != null && AttackLunge.length >= 2 &&
                                         HitFlash != null && HitFlash.length >= 2 &&
                                         DefeatScaleY != null && DefeatScaleY.length >= 2 &&
                                         RaiseScale != null && RaiseScale.length >= 2 &&
                                         AlliedContributionScale != null && AlliedContributionScale.length >= 2;

        public int PlayerAttackCueCount { get; private set; }
        public string CurrentEnemyArtName => _enemy != null && _enemy.texture != null
            ? _enemy.texture.name
            : null;
        public string CurrentEnemyArchetypeId { get; private set; }
        public int HitCueCount { get; private set; }
        public int DefeatCueCount { get; private set; }
        public int RaiseCueCount { get; private set; }
        public int AlliedContributionCueCount { get; private set; }
        public int ReviewSfxPlaybackCount { get; private set; }
        public int VisibleAllyCount { get; private set; }
        public string LastContributionUnitId { get; private set; }
        public string LastReviewSfxName { get; private set; }

        FirstPlayableGameplayComposition _game;
        FirstPlayableAutoCombatLoop _playerLoop;
        FirstPlayableAlliedAutoCombatLoop _alliedLoop;
        RawImage _player, _enemy, _background;
        Image _screenBackdrop;
        RectTransform _gateObjective;
        readonly Image[] _gateAccentParts = new Image[7];
        Sprite _gateStoneSprite, _gateHaloSprite;
        AudioSource _audio;
        readonly RawImage[] _allies = new RawImage[Formation.Capacity];
        readonly RectTransform[] _allyVfxRoots = new RectTransform[Formation.Capacity];
        readonly Image[,] _allyVfxParts = new Image[Formation.Capacity, 4];
        readonly float[] _allyRaiseRemaining = new float[Formation.Capacity];
        readonly float[] _allyContributionRemaining = new float[Formation.Capacity];
        RectTransform _root;
        string _enemyId;
        float _attackRemaining, _hitRemaining, _defeatRemaining;
        bool _defeated;
        bool _initialized;

        public void Initialize(
            FirstPlayableGameplayComposition game,
            RectTransform combat,
            FirstPlayableAutoCombatLoop playerLoop,
            FirstPlayableAlliedAutoCombatLoop alliedLoop)
        {
            if (_initialized) throw new InvalidOperationException("Visual presentation already initialized.");
            if (LoadedArtCount != 4) throw new InvalidOperationException("Q3 art references unresolved.");
            if (!HasAuthoredMotion) throw new InvalidOperationException("Q3 authored motion profile unresolved.");
            if (LoadedAudioClipCount != 5) throw new InvalidOperationException("Q3 production-candidate audio references unresolved.");

            _game = game;
            _playerLoop = playerLoop;
            _alliedLoop = alliedLoop;

            var backdropObject = new GameObject(
                "CuteScreenBackdrop",
                typeof(RectTransform),
                typeof(Image));
            backdropObject.transform.SetParent(_game.transform, false);
            var backdropRect = (RectTransform)backdropObject.transform;
            backdropRect.anchorMin = Vector2.zero;
            backdropRect.anchorMax = Vector2.one;
            backdropRect.offsetMin = backdropRect.offsetMax = Vector2.zero;
            backdropRect.SetSiblingIndex(0);
            _screenBackdrop = backdropObject.GetComponent<Image>();
            _screenBackdrop.color = new Color(1f, .975f, .91f, 1f);
            _screenBackdrop.raycastTarget = false;
            if (Camera.main != null)
                Camera.main.backgroundColor = new Color(1f, .975f, .91f, 1f);

            var host = new GameObject("Q3VisualPresentation", typeof(RectTransform));
            host.transform.SetParent(combat, false);
            _root = (RectTransform)host.transform;
            _root.anchorMin = Vector2.zero;
            _root.anchorMax = Vector2.one;
            _root.offsetMin = _root.offsetMax = Vector2.zero;

            _audio = host.AddComponent<AudioSource>();
            _audio.playOnAwake = false;
            _audio.loop = false;
            _audio.spatialBlend = 0f;
            _audio.volume = Mathf.Clamp01(SfxMasterVolume);

            _background = Art("Background", BackgroundArt);
            _background.rectTransform.anchorMin = Vector2.zero;
            _background.rectTransform.anchorMax = Vector2.one;
            _background.rectTransform.offsetMin = _background.rectTransform.offsetMax = Vector2.zero;

            var gateObject = new GameObject("GateObjective", typeof(RectTransform));
            gateObject.transform.SetParent(_root, false);
            _gateObjective = (RectTransform)gateObject.transform;
            _gateObjective.anchorMin = _gateObjective.anchorMax = Vector2.zero;
            _gateObjective.pivot = Vector2.zero;
            _gateObjective.sizeDelta = new Vector2(46f, 138f);

            // Cute Spirit Garden objective: rounded cream pillars + mint/coral spirit bars.
            // Defended-objective geometry and gate-pressure endpoint stay unchanged.
            _gateStoneSprite = FirstPlayableCombatHudUnityView.RoundedSprite(4f);
            _gateHaloSprite = FirstPlayableCombatHudUnityView.RoundedSprite(10f);
            GateStone("LeftPylon", 1f, 13f, 10f, 91f);
            GateStone("RightPylon", 35f, 13f, 10f, 91f);
            GateStone("Lintel", 4f, 100f, 38f, 10f);
            GateStone("LeftCap", 0f, 107f, 12f, 9f);
            GateStone("RightCap", 34f, 107f, 12f, 9f);
            GateStone("GateBase", 2f, 5f, 42f, 11f);
            _gateAccentParts[0] = GateAccent("CenterBar", 21f, 23f, 4f, 72f, .58f);
            _gateAccentParts[1] = GateAccent("BarLeft", 14f, 23f, 3f, 68f, .34f);
            _gateAccentParts[2] = GateAccent("BarRight", 29f, 23f, 3f, 68f, .34f);
            _gateAccentParts[3] = GateAccent("SoulSeal", 18f, 120f, 10f, 10f, .96f);
            _gateAccentParts[3].rectTransform.localRotation = Quaternion.Euler(0f, 0f, 45f);
            _gateAccentParts[4] = GateAccent("SoulHalo", 13f, 115f, 20f, 20f, .20f, _gateHaloSprite);
            _gateAccentParts[5] = GateAccent("ThresholdLeft", 5f, 16f, 14f, 2f, .72f);
            _gateAccentParts[6] = GateAccent("ThresholdRight", 27f, 16f, 14f, 2f, .72f);

            _player = Art("Necromancer", NecromancerArt);
            _enemy = Art("Guard", GuardArt);
            for (var i = 0; i < Formation.Capacity; i++)
            {
                _allies[i] = Art("RaisedGuardSlot" + i, RaisedGuardArt);
                _allies[i].gameObject.SetActive(false);

                var vfx = new GameObject(
                    "CharacterVfxSlot" + i,
                    typeof(RectTransform));
                vfx.transform.SetParent(_root, false);
                _allyVfxRoots[i] = (RectTransform)vfx.transform;
                _allyVfxRoots[i].anchorMin = _allyVfxRoots[i].anchorMax = Vector2.zero;
                _allyVfxRoots[i].pivot = Vector2.zero;
                vfx.SetActive(false);

                for (var part = 0; part < 4; part++)
                {
                    var dot = new GameObject(
                        "VfxPart" + part,
                        typeof(RectTransform),
                        typeof(Image));
                    dot.transform.SetParent(_allyVfxRoots[i], false);
                    var image = dot.GetComponent<Image>();
                    image.sprite = _gateHaloSprite;
                    image.type = Image.Type.Sliced;
                    image.raycastTarget = false;
                    _allyVfxParts[i, part] = image;
                }
            }

            _playerLoop.AttackApplied += PlayerAttack;
            _alliedLoop.AttackApplied += AlliedAttack;
            _initialized = true;
            Refresh();
        }

        RawImage Art(string name, Texture texture)
        {
            var o = new GameObject(name, typeof(RectTransform), typeof(RawImage));
            o.transform.SetParent(_root, false);
            var image = o.GetComponent<RawImage>();
            image.texture = texture;
            image.raycastTarget = false;
            image.rectTransform.anchorMin = image.rectTransform.anchorMax = Vector2.zero;
            image.rectTransform.pivot = Vector2.zero;
            return image;
        }

        Image GateStone(string name, float x, float y, float width, float height)
        {
            var image = GatePart(name, x, y, width, height, _gateStoneSprite);
            image.color = new Color(1f, .975f, .91f, .98f);
            return image;
        }

        Image GateAccent(
            string name, float x, float y, float width, float height,
            float alpha, Sprite sprite = null)
        {
            var image = GatePart(name, x, y, width, height, sprite ?? _gateStoneSprite);
            image.color = new Color(79f/255f, 212f/255f, 174f/255f, alpha);
            return image;
        }

        Image GatePart(
            string name, float x, float y, float width, float height, Sprite sprite)
        {
            var o = new GameObject(name, typeof(RectTransform), typeof(Image));
            o.transform.SetParent(_gateObjective, false);
            var image = o.GetComponent<Image>();
            image.sprite = sprite;
            image.type = Image.Type.Sliced;
            image.raycastTarget = false;
            var outline = o.AddComponent<Outline>();
            outline.effectColor = new Color(56f/255f,51f/255f,79f/255f,.86f);
            outline.effectDistance = new Vector2(1.1f,-1.1f);
            outline.useGraphicAlpha = true;
            var rect = image.rectTransform;
            rect.anchorMin = rect.anchorMax = Vector2.zero;
            rect.pivot = Vector2.zero;
            rect.anchoredPosition = new Vector2(x, y);
            rect.sizeDelta = new Vector2(width, height);
            return image;
        }

        void ApplyGateObjectiveState()
        {
            var failed = _game.DefenseWave != null &&
                         _game.DefenseWave.Phase == DefenseWavePhase.Failed;
            var baseColor = failed
                ? new Color(1f, 122f/255f, 120f/255f)
                : new Color(79f/255f, 212f/255f, 174f/255f);
            var alpha = new[] { .58f, .34f, .34f, .96f, .20f, .72f, .72f };
            for (var i = 0; i < _gateAccentParts.Length; i++)
            {
                if (_gateAccentParts[i] != null)
                    _gateAccentParts[i].color =
                        new Color(baseColor.r, baseColor.g, baseColor.b, alpha[i]);
            }
        }

        FirstPlayableCharacterVisualBinding BindingFor(string archetypeId)
        {
            if (CharacterVisuals == null || string.IsNullOrWhiteSpace(archetypeId))
                return null;

            for (var i = 0; i < CharacterVisuals.Length; i++)
            {
                var binding = CharacterVisuals[i];
                if (binding != null &&
                    string.Equals(
                        binding.ArchetypeId,
                        archetypeId,
                        StringComparison.Ordinal))
                    return binding;
            }

            return null;
        }

        Texture2D CharacterArtFor(string archetypeId, bool raised)
        {
            var binding = BindingFor(archetypeId);
            if (binding != null && binding.Art != null)
                return binding.Art;
            return raised ? RaisedGuardArt : GuardArt;
        }

        Color CharacterAccentFor(string archetypeId)
        {
            var binding = BindingFor(archetypeId);
            return binding != null ? binding.Accent : new Color(.31f, .83f, .68f);
        }

        FirstPlayableCharacterVfxIdentity CharacterVfxFor(string archetypeId)
        {
            var binding = BindingFor(archetypeId);
            return binding != null
                ? binding.VfxIdentity
                : FirstPlayableCharacterVfxIdentity.None;
        }

        public string GetAllyArtName(int slot)
        {
            if (slot < 0 || slot >= Formation.Capacity)
                throw new ArgumentOutOfRangeException(nameof(slot));
            var ally = _allies[slot];
            return ally != null && ally.texture != null
                ? ally.texture.name
                : null;
        }

        public string GetAllyArchetypeId(int slot)
        {
            if (slot < 0 || slot >= Formation.Capacity)
                throw new ArgumentOutOfRangeException(nameof(slot));
            return _game?.Roster?.GetSlot(slot)?.Model?.ArchetypeId;
        }

        public FirstPlayableCharacterVfxIdentity GetAllyVfxIdentity(int slot)
            => CharacterVfxFor(GetAllyArchetypeId(slot));

        void PlayerAttack(EntityId actor, DamageDeathResult result)
        {
            if (!result.Changed) return;
            PlayerAttackCueCount++;
            _attackRemaining = AttackDuration;
            PlaySfx(PlayerAttackSfx, PlayerAttackSfxGain);
            Hit(result);
        }

        void AlliedAttack(EntityId actor, DamageDeathResult result)
        {
            if (!result.Changed) return;
            AlliedContributionCueCount++;
            LastContributionUnitId = actor.Value;
            for (var i = 0; i < Formation.Capacity; i++)
            {
                var ally = _game.Roster.GetSlot(i);
                if (ally != null && ally.Model.Id.Equals(actor))
                    _allyContributionRemaining[i] = AlliedContributionDuration;
            }
            PlaySfx(AlliedContributionSfx, AlliedContributionSfxGain);
            Hit(result);
        }

        void Hit(DamageDeathResult result)
        {
            HitCueCount++;
            _hitRemaining = HitDuration;
            PlaySfx(HitSfx, HitSfxGain);
            if (!result.BecameDefeated) return;

            DefeatCueCount++;
            _defeated = true;
            _defeatRemaining = DefeatDuration;
            PlaySfx(DefeatSfx, DefeatSfxGain);
        }

        void PlaySfx(AudioClip clip, float gain)
        {
            if (clip == null || _audio == null) return;
            _audio.PlayOneShot(clip, Mathf.Clamp01(gain));
            // Compatibility counters retain their historical names so older evidence tooling stays valid.
            ReviewSfxPlaybackCount++;
            LastReviewSfxName = clip.name;
        }

        void Update()
        {
            if (!_initialized) return;
            var dt = Time.deltaTime;
            _attackRemaining = Mathf.Max(0f, _attackRemaining - dt);
            _hitRemaining = Mathf.Max(0f, _hitRemaining - dt);
            _defeatRemaining = Mathf.Max(0f, _defeatRemaining - dt);
            for (var i = 0; i < Formation.Capacity; i++)
            {
                _allyRaiseRemaining[i] = Mathf.Max(0f, _allyRaiseRemaining[i] - dt);
                _allyContributionRemaining[i] = Mathf.Max(0f, _allyContributionRemaining[i] - dt);
            }
            Refresh();
        }

        void LateUpdate()
        {
            if (_initialized) Refresh();
        }

        static float NormalizedProgress(float remaining, float duration)
            => duration <= 0f ? 1f : 1f - Mathf.Clamp01(remaining / duration);

        static float Curve(AnimationCurve curve, float remaining, float duration, float settled)
            => remaining > 0f && curve != null
                ? curve.Evaluate(NormalizedProgress(remaining, duration))
                : settled;

        public void Refresh()
        {
            if (!_initialized) return;

            _game.Enemies.TryGetFirstActiveTarget(out var activeTarget);
            var interactionTarget = _game.Enemies.CurrentTarget;
            var preserveDefeat =
                _defeated &&
                _defeatRemaining > 0f &&
                interactionTarget?.Model?.LifeState ==
                    CombatantLifeState.Defeated;
            var target = preserveDefeat
                ? interactionTarget
                : activeTarget ?? interactionTarget;
            if (_game.DefenseWave != null &&
                _game.DefenseWave.Phase == DefenseWavePhase.Failed)
                target = null;
            var id = target?.Model?.Id.Value;
            if (id != _enemyId)
            {
                _enemyId = id;
                CurrentEnemyArchetypeId = target?.Model?.ArchetypeId;
                _defeated = false;
                _defeatRemaining = 0f;
                _hitRemaining = 0f;
                _enemy.texture = CharacterArtFor(
                    CurrentEnemyArchetypeId,
                    false);
            }
            _enemy.gameObject.SetActive(target != null);

            var h = _root.rect.height;
            var w = _root.rect.width;
            var protectedHeight = ((RectTransform)_game.transform.Find("SafeArea/ProtectedCombatReadabilityZone")).rect.height;
            var scale = Mathf.Min(1f, h / 312.48f, protectedHeight / 218f);
            // Small portrait screens previously shrank allies to ~20x29 physical px.
            // Keep the same Formation truth but impose a readability floor without escaping the protected zone.
            var allyScale = Mathf.Max(scale, Mathf.Min(1f, w / 500f));

            var lunge = Curve(AttackLunge, _attackRemaining, AttackDuration, 0f);
            var hit = Curve(HitFlash, _hitRemaining, HitDuration, 0f);
            var pressure = 0f;
            if (activeTarget != null &&
                ReferenceEquals(target, activeTarget) &&
                _game.GatePressure != null)
            {
                _game.GatePressure.TryGetProgress(activeTarget, out pressure);
            }

            var enemyStartX = 246f / 390f * w;
            var enemyGateX = 164f / 390f * w;
            var enemyX = Mathf.Lerp(enemyStartX, enemyGateX, pressure);
            // Preserve the prior objective center/pressure endpoint while replacing only its art.
            _gateObjective.anchoredPosition = new Vector2(128f / 390f * w, 75f * scale);
            _gateObjective.sizeDelta = new Vector2(46f, 138f);
            _gateObjective.localScale = Vector3.one * scale;
            ApplyGateObjectiveState();
            Place(_player, 42f / 390f * w + lunge * 11f * scale, 32f * scale, 92f * scale, 138f * scale);
            Place(_enemy, enemyX + hit * 2.5f * scale, 75f * scale, 90f * scale, 135f * scale);

            var defeatScaleY = _defeated
                ? Curve(DefeatScaleY, _defeatRemaining, DefeatDuration, .35f)
                : 1f;
            _enemy.rectTransform.localScale = new Vector3(1f, defeatScaleY, 1f);

            var enemyAccent = CharacterAccentFor(CurrentEnemyArchetypeId);
            _enemy.color = _defeated && _defeatRemaining <= 0f
                ? new Color(.72f, .74f, .82f, .58f)
                : Color.Lerp(
                    Color.white,
                    new Color(enemyAccent.r, enemyAccent.g, enemyAccent.b, 1f),
                    Mathf.Clamp01(hit));

            var before = VisibleAllyCount;
            VisibleAllyCount = 0;
            for (var i = 0; i < Formation.Capacity; i++)
            {
                var ally = _game.Roster.GetSlot(i);
                var exists = ally != null && ally.Model != null;
                var archetypeId = exists ? ally.Model.ArchetypeId : null;
                _allies[i].gameObject.SetActive(exists);
                if (exists)
                {
                    VisibleAllyCount++;
                    _allies[i].texture =
                        CharacterArtFor(archetypeId, true);
                }

                var contribution = Curve(
                    AlliedContributionScale,
                    _allyContributionRemaining[i],
                    AlliedContributionDuration,
                    1f);
                var raise = Curve(
                    RaiseScale,
                    _allyRaiseRemaining[i],
                    RaiseDuration,
                    1f);
                var contributionActive = _allyContributionRemaining[i] > 0f;
                var raiseActive = _allyRaiseRemaining[i] > 0f;
                var localLift =
                    (raise - 1f) * 18f * scale +
                    (contribution - 1f) * 14f * scale;
                var allyX = (142f + 34f * i) / 390f * w;
                var allyY = 15f * scale + localLift;

                Place(
                    _allies[i],
                    allyX,
                    allyY,
                    42f * allyScale,
                    63f * allyScale);
                Place(
                    _allyVfxRoots[i],
                    allyX - 7f * allyScale,
                    allyY - 5f * allyScale,
                    58f * allyScale,
                    74f * allyScale);

                _allies[i].rectTransform.localScale =
                    Vector3.one * Mathf.Max(raise, contribution);
                if (raiseActive)
                {
                    var progress =
                        NormalizedProgress(
                            _allyRaiseRemaining[i],
                            RaiseDuration);
                    var accent = CharacterAccentFor(archetypeId);
                    _allies[i].color = Color.Lerp(
                        new Color(
                            accent.r,
                            accent.g,
                            accent.b,
                            .50f),
                        Color.white,
                        Mathf.SmoothStep(0f, 1f, progress));
                }
                else
                {
                    var accent = CharacterAccentFor(archetypeId);
                    _allies[i].color = contributionActive
                        ? Color.Lerp(
                            Color.white,
                            new Color(
                                accent.r,
                                accent.g,
                                accent.b,
                                1f),
                            .42f)
                        : Color.white;
                }

                ConfigureAllyVfx(
                    i,
                    archetypeId,
                    contributionActive,
                    raiseActive);
            }

            if (VisibleAllyCount > before)
            {
                for (var i = before; i < VisibleAllyCount; i++)
                    _allyRaiseRemaining[i] = RaiseDuration;
                RaiseCueCount += VisibleAllyCount - before;
                PlaySfx(RaiseSfx, RaiseSfxGain);
            }

            // Same FILL crop as Figma; preserve background aspect ratio.
            var textureAspect = (float)BackgroundArt.width / BackgroundArt.height;
            var viewportAspect = w / Mathf.Max(1f, h);
            _background.uvRect = textureAspect > viewportAspect
                ? new Rect((1f - viewportAspect / textureAspect) / 2f, 0f, viewportAspect / textureAspect, 1f)
                : new Rect(0f, (1f - textureAspect / viewportAspect) / 2f, 1f, textureAspect / viewportAspect);
        }

        void ConfigureAllyVfx(
            int slot,
            string archetypeId,
            bool attackActive,
            bool raiseActive)
        {
            var root = _allyVfxRoots[slot];
            if (root == null)
                return;

            var active = attackActive || raiseActive;
            root.gameObject.SetActive(active);
            if (!active)
                return;

            var accent = CharacterAccentFor(archetypeId);
            if (raiseActive)
            {
                SetVfxPart(slot, 0, new Vector2(8f, 12f), new Vector2(8f, 8f), accent, 45f, .50f);
                SetVfxPart(slot, 1, new Vector2(42f, 14f), new Vector2(8f, 8f), accent, 45f, .42f);
                SetVfxPart(slot, 2, new Vector2(10f, 52f), new Vector2(7f, 7f), accent, 45f, .34f);
                SetVfxPart(slot, 3, new Vector2(40f, 55f), new Vector2(9f, 9f), accent, 45f, .56f);
                return;
            }

            switch (CharacterVfxFor(archetypeId))
            {
                case FirstPlayableCharacterVfxIdentity.LeafBarrier:
                    SetVfxPart(slot, 0, new Vector2(3f, 22f), new Vector2(10f, 18f), accent, -35f, .74f);
                    SetVfxPart(slot, 1, new Vector2(45f, 20f), new Vector2(10f, 18f), accent, 35f, .74f);
                    SetVfxPart(slot, 2, new Vector2(18f, 55f), new Vector2(9f, 16f), accent, -55f, .58f);
                    SetVfxPart(slot, 3, new Vector2(33f, 57f), new Vector2(9f, 16f), accent, 55f, .58f);
                    break;

                case FirstPlayableCharacterVfxIdentity.StarArrow:
                    SetVfxPart(slot, 0, new Vector2(7f, 34f), new Vector2(34f, 4f), accent, 0f, .76f);
                    SetVfxPart(slot, 1, new Vector2(42f, 29f), new Vector2(12f, 12f), accent, 45f, .94f);
                    SetVfxPart(slot, 2, new Vector2(23f, 15f), new Vector2(7f, 7f), new Color(1f,.79f,.33f), 45f, .90f);
                    SetVfxPart(slot, 3, new Vector2(30f, 52f), new Vector2(6f, 6f), new Color(1f,.79f,.33f), 45f, .72f);
                    break;

                case FirstPlayableCharacterVfxIdentity.DewHeal:
                    SetVfxPart(slot, 0, new Vector2(10f, 44f), new Vector2(8f, 13f), accent, 0f, .76f);
                    SetVfxPart(slot, 1, new Vector2(24f, 55f), new Vector2(9f, 14f), accent, 0f, .88f);
                    SetVfxPart(slot, 2, new Vector2(39f, 40f), new Vector2(8f, 13f), accent, 0f, .72f);
                    SetVfxPart(slot, 3, new Vector2(27f, 19f), new Vector2(6f, 10f), accent, 0f, .54f);
                    break;

                case FirstPlayableCharacterVfxIdentity.PineSlash:
                    SetVfxPart(slot, 0, new Vector2(7f, 42f), new Vector2(44f, 5f), accent, -18f, .82f);
                    SetVfxPart(slot, 1, new Vector2(14f, 28f), new Vector2(36f, 4f), new Color(1f,.79f,.33f), -18f, .70f);
                    SetVfxPart(slot, 2, new Vector2(44f, 15f), new Vector2(8f, 8f), accent, 45f, .80f);
                    SetVfxPart(slot, 3, new Vector2(7f, 55f), new Vector2(7f, 7f), new Color(1f,.79f,.33f), 45f, .64f);
                    break;

                case FirstPlayableCharacterVfxIdentity.CloverWind:
                    SetVfxPart(slot, 0, new Vector2(5f, 21f), new Vector2(15f, 10f), accent, -28f, .72f);
                    SetVfxPart(slot, 1, new Vector2(40f, 17f), new Vector2(15f, 10f), accent, 28f, .72f);
                    SetVfxPart(slot, 2, new Vector2(12f, 52f), new Vector2(15f, 10f), accent, 28f, .58f);
                    SetVfxPart(slot, 3, new Vector2(38f, 50f), new Vector2(15f, 10f), accent, -28f, .58f);
                    break;

                case FirstPlayableCharacterVfxIdentity.ChestnutStar:
                    SetVfxPart(slot, 0, new Vector2(8f, 39f), new Vector2(42f, 5f), accent, -18f, .84f);
                    SetVfxPart(slot, 1, new Vector2(39f, 18f), new Vector2(12f, 12f), new Color(1f,.79f,.33f), 45f, .96f);
                    SetVfxPart(slot, 2, new Vector2(18f, 17f), new Vector2(8f, 8f), accent, 45f, .78f);
                    SetVfxPart(slot, 3, new Vector2(46f, 51f), new Vector2(7f, 7f), new Color(1f,.79f,.33f), 45f, .68f);
                    break;

                case FirstPlayableCharacterVfxIdentity.BerryBurst:
                    SetVfxPart(slot, 0, new Vector2(28f, 37f), new Vector2(22f, 22f), accent, 45f, .92f);
                    SetVfxPart(slot, 1, new Vector2(9f, 22f), new Vector2(32f, 5f), new Color(1f,.72f,.50f), -36f, .82f);
                    SetVfxPart(slot, 2, new Vector2(46f, 20f), new Vector2(32f, 5f), new Color(1f,.72f,.50f), 34f, .82f);
                    SetVfxPart(slot, 3, new Vector2(35f, 55f), new Vector2(16f, 8f), new Color(.31f,.83f,.47f), -20f, .86f);
                    break;

                case FirstPlayableCharacterVfxIdentity.GrowthRingBarrier:
                    SetVfxPart(slot, 0, new Vector2(27f, 36f), new Vector2(42f, 42f), accent, 45f, .28f);
                    SetVfxPart(slot, 1, new Vector2(27f, 36f), new Vector2(30f, 30f), new Color(.94f,.66f,.28f), 45f, .42f);
                    SetVfxPart(slot, 2, new Vector2(27f, 36f), new Vector2(18f, 18f), new Color(1f,.82f,.38f), 45f, .58f);
                    SetVfxPart(slot, 3, new Vector2(47f, 54f), new Vector2(10f, 6f), new Color(.46f,.28f,.14f), -18f, .70f);
                    break;

                case FirstPlayableCharacterVfxIdentity.SeedlingCall:
                    SetVfxPart(slot, 0, new Vector2(20f, 34f), new Vector2(12f, 16f), new Color(1f,.82f,.38f), 0f, .90f);
                    SetVfxPart(slot, 1, new Vector2(42f, 28f), new Vector2(10f, 14f), new Color(1f,.82f,.38f), 0f, .76f);
                    SetVfxPart(slot, 2, new Vector2(22f, 54f), new Vector2(7f, 22f), accent, -18f, .84f);
                    SetVfxPart(slot, 3, new Vector2(45f, 52f), new Vector2(7f, 20f), accent, 18f, .72f);
                    break;

                case FirstPlayableCharacterVfxIdentity.FireflyTrail:
                    SetVfxPart(slot, 0, new Vector2(17f, 30f), new Vector2(9f, 9f), new Color(1f,.82f,.38f), 45f, .92f);
                    SetVfxPart(slot, 1, new Vector2(41f, 24f), new Vector2(8f, 8f), new Color(1f,.82f,.38f), 45f, .84f);
                    SetVfxPart(slot, 2, new Vector2(49f, 47f), new Vector2(9f, 9f), new Color(1f,.82f,.38f), 45f, .74f);
                    SetVfxPart(slot, 3, new Vector2(28f, 55f), new Vector2(32f, 5f), accent, -18f, .54f);
                    break;

                case FirstPlayableCharacterVfxIdentity.SkyBulwark:
                    SetVfxPart(slot, 0, new Vector2(27f, 36f), new Vector2(44f, 44f), accent, 45f, .30f);
                    SetVfxPart(slot, 1, new Vector2(27f, 36f), new Vector2(30f, 30f), new Color(.86f,.94f,1f), 45f, .44f);
                    SetVfxPart(slot, 2, new Vector2(27f, 36f), new Vector2(16f, 16f), new Color(1f,.79f,.32f), 45f, .78f);
                    SetVfxPart(slot, 3, new Vector2(30f, 57f), new Vector2(34f, 8f), new Color(.96f,.99f,1f), 0f, .76f);
                    break;

                default:
                    for (var part = 0; part < 4; part++)
                        _allyVfxParts[slot, part].gameObject.SetActive(false);
                    break;
            }
        }

        void SetVfxPart(
            int slot,
            int part,
            Vector2 position,
            Vector2 size,
            Color color,
            float rotation,
            float alpha)
        {
            var image = _allyVfxParts[slot, part];
            image.gameObject.SetActive(true);
            image.color = new Color(color.r, color.g, color.b, alpha);
            var rect = image.rectTransform;
            rect.anchorMin = rect.anchorMax = Vector2.zero;
            rect.pivot = new Vector2(.5f, .5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            rect.localRotation = Quaternion.Euler(0f, 0f, rotation);
        }

        static void Place(RawImage image, float x, float y, float width, float height)
        {
            Place(image.rectTransform, x, y, width, height);
        }

        static void Place(RectTransform rect, float x, float y, float width, float height)
        {
            rect.anchoredPosition = new Vector2(x, y);
            rect.sizeDelta = new Vector2(width, height);
        }

        void OnDestroy()
        {
            if (_playerLoop != null) _playerLoop.AttackApplied -= PlayerAttack;
            if (_alliedLoop != null) _alliedLoop.AttackApplied -= AlliedAttack;
            ReleaseSprite(_gateStoneSprite);
            ReleaseSprite(_gateHaloSprite);
        }

        static void ReleaseSprite(Sprite sprite)
        {
            if (sprite == null) return;
            if (Application.isPlaying)
            {
                Destroy(sprite.texture);
                Destroy(sprite);
            }
            else
            {
                DestroyImmediate(sprite.texture);
                DestroyImmediate(sprite);
            }
        }
    }
}
