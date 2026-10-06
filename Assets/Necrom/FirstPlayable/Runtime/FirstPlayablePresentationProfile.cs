using UnityEngine;
using UnityEngine.Serialization;

namespace Necrom.FirstPlayable.Runtime
{
    // Authored Q3 production-candidate motion + SFX playback contract.
    // Final release-rights, device mix and user acceptance remain separate evidence gates.
    [CreateAssetMenu(menuName = "Necrom/First Playable Presentation Profile", fileName = "FirstPlayablePresentationProfile")]
    public sealed class FirstPlayablePresentationProfile : ScriptableObject
    {
        public AnimationCurve AttackLunge;
        public AnimationCurve HitFlash;
        public AnimationCurve DefeatScaleY;
        public AnimationCurve RaiseScale;
        public AnimationCurve AlliedContributionScale;

        public float AttackDuration = .24f;
        public float HitDuration = .16f;
        public float DefeatDuration = .50f;
        public float RaiseDuration = .66f;
        public float AlliedContributionDuration = .20f;

        [FormerlySerializedAs("ReviewSfxVolume")]
        public float SfxMasterVolume = .34f;
        public float PlayerAttackSfxGain = .72f;
        public float HitSfxGain = .48f;
        public float DefeatSfxGain = .82f;
        public float RaiseSfxGain = 1.00f;
        public float AlliedContributionSfxGain = .38f;

        public AudioClip PlayerAttackSfx;
        public AudioClip HitSfx;
        public AudioClip DefeatSfx;
        public AudioClip RaiseSfx;
        public AudioClip AlliedContributionSfx;

        public int LoadedAudioClipCount =>
            (PlayerAttackSfx ? 1 : 0) +
            (HitSfx ? 1 : 0) +
            (DefeatSfx ? 1 : 0) +
            (RaiseSfx ? 1 : 0) +
            (AlliedContributionSfx ? 1 : 0);

        public bool HasAuthoredMotion =>
            AttackLunge != null && AttackLunge.length >= 2 &&
            HitFlash != null && HitFlash.length >= 2 &&
            DefeatScaleY != null && DefeatScaleY.length >= 2 &&
            RaiseScale != null && RaiseScale.length >= 2 &&
            AlliedContributionScale != null && AlliedContributionScale.length >= 2;
    }
}
