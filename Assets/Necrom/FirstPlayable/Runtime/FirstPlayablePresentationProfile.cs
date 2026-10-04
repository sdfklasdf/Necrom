using UnityEngine;

namespace Necrom.FirstPlayable.Runtime
{
    // Authored Q3 presentation timing + review SFX references.
    // The asset is a review-quality playback contract, not approved production sound design.
    [CreateAssetMenu(menuName = "Necrom/First Playable Presentation Profile", fileName = "FirstPlayablePresentationProfile")]
    public sealed class FirstPlayablePresentationProfile : ScriptableObject
    {
        public AnimationCurve AttackLunge;
        public AnimationCurve HitFlash;
        public AnimationCurve DefeatScaleY;
        public AnimationCurve RaiseScale;
        public AnimationCurve AlliedContributionScale;

        public float AttackDuration = .18f;
        public float HitDuration = .12f;
        public float DefeatDuration = .34f;
        public float RaiseDuration = .42f;
        public float AlliedContributionDuration = .24f;
        public float ReviewSfxVolume = .28f;

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
