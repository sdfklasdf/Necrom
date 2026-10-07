using System;
using System.Linq;
using Necrom.Core.Domain;
using EntityId = Necrom.Core.Domain.EntityId;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Necrom.FirstPlayable.Runtime
{
    // Q3 production-candidate art + authored presentation profile. No gameplay state adapter or fabricated contribution.
    [DisallowMultipleComponent]
    public sealed class FirstPlayableVisualPresentation : MonoBehaviour
    {
        public Texture2D NecromancerArt, GuardArt, RaisedGuardArt, BackgroundArt;
        public Texture2D[] ForestFriendEnemyArts = Array.Empty<Texture2D>();
        public int LoadedForestFriendArtCount =>
            ForestFriendEnemyArts == null ? 0 : ForestFriendEnemyArts.Count(x => x != null);
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

        Texture2D EnemyArtFor(string id)
        {
            if (ForestFriendEnemyArts == null || ForestFriendEnemyArts.Length == 0 || string.IsNullOrEmpty(id))
                return GuardArt;

            var separator = id.LastIndexOf(':');
            if (separator >= 0 &&
                int.TryParse(id.Substring(separator + 1), out var ordinal) &&
                ordinal > 0)
            {
                var index = (ordinal - 1) % ForestFriendEnemyArts.Length;
                return ForestFriendEnemyArts[index] != null ? ForestFriendEnemyArts[index] : GuardArt;
            }

            return ForestFriendEnemyArts[0] != null ? ForestFriendEnemyArts[0] : GuardArt;
        }

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
                _defeated = false;
                _defeatRemaining = 0f;
                _hitRemaining = 0f;
                _enemy.texture = EnemyArtFor(id);
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

            _enemy.color = _defeated && _defeatRemaining <= 0f
                ? new Color(.65f, .75f, .8f, .65f)
                : Color.Lerp(Color.white, new Color(1f, .44f, .44f), Mathf.Clamp01(hit));

            var before = VisibleAllyCount;
            VisibleAllyCount = 0;
            for (var i = 0; i < Formation.Capacity; i++)
            {
                var exists = _game.Roster.GetSlot(i) != null;
                _allies[i].gameObject.SetActive(exists);
                if (exists) VisibleAllyCount++;

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

                Place(
                    _allies[i],
                    (142f + 34f * i) / 390f * w,
                    15f * scale + localLift,
                    42f * allyScale,
                    63f * allyScale);

                _allies[i].rectTransform.localScale = Vector3.one * Mathf.Max(raise, contribution);
                if (raiseActive)
                {
                    var progress = NormalizedProgress(_allyRaiseRemaining[i], RaiseDuration);
                    _allies[i].color = Color.Lerp(
                        new Color(.34f, 1f, .66f, .55f),
                        Color.white,
                        Mathf.SmoothStep(0f, 1f, progress));
                }
                else
                {
                    _allies[i].color = contributionActive
                        ? new Color(.58f, 1f, .72f)
                        : Color.white;
                }
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
