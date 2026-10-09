using System;
using UnityEngine;

namespace Necrom.Core.Domain
{
    // Prototype-local wall clock; server authority and anti-clock-cheat ledger required before release.
    public sealed class OfflineRewardManager
    {
        private const string LastUtcKey = "NECROM_OFFLINE_LAST_UTC_V2";
        private const string PendingGoldKey = "NECROM_OFFLINE_PENDING_V2";
        private const string PendingDiamondsKey = "NECROM_OFFLINE_PENDING_DIAMONDS_V1";
        private const string PendingSecondsKey = "NECROM_OFFLINE_PENDING_SECONDS_V1";
        public const int MaxOfflineSeconds = 28800;
        public const int MinimumRewardSeconds = 60;
        public const long GoldPerMinute = 100;
        public const long DiamondsPerMinute = 10;
        public long CoinsPerHour { get; set; } = GoldPerMinute * 60; // legacy compatibility
        public long PendingCoins { get; private set; }
        public long PendingDiamonds { get; private set; }
        public int EarnedSeconds { get; private set; }
        public long PendingSeconds { get; private set; }
        private bool initialized;

        public static int CappedElapsed(long lastUtc,long nowUtc)
        {
            return OfflineRewardMath.Elapsed(lastUtc,nowUtc);
        }
        public static long EarnedGold(int seconds) => OfflineRewardMath.Gold(seconds);
        public static long EarnedDiamonds(int seconds) => OfflineRewardMath.Diamonds(seconds);

        private static long ReadNonNegative(string key)
        {
            return long.TryParse(PlayerPrefs.GetString(key,"0"),out var number) && number>=0 ? number : 0;
        }
        public void BeginSession() => ResumeAt(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        private void ResumeAt(long now)
        {
            if(initialized) return;
            initialized=true;
            PendingCoins=ReadNonNegative(PendingGoldKey);
            PendingDiamonds=ReadNonNegative(PendingDiamondsKey);
            PendingSeconds=ReadNonNegative(PendingSecondsKey);
            long last;
            if(!long.TryParse(PlayerPrefs.GetString(LastUtcKey,""),out last)) last=now;
            EarnedSeconds=CappedElapsed(last,now);
            if(EarnedSeconds>=MinimumRewardSeconds)
            {
                PendingSeconds=checked(PendingSeconds+EarnedSeconds);
                PendingCoins=checked(PendingCoins+EarnedGold(EarnedSeconds));
                PendingDiamonds=checked(PendingDiamonds+EarnedDiamonds(EarnedSeconds));
            }
            SavePending();
            SaveClock(now);
        }
        public void RecordExit()
        {
            initialized=false;
            SaveClock(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        }
        // Local-only; client clock manipulation is possible and not suitable for live economy.
        public bool Claim(Action<long,long> grant)
        {
            if(grant==null)throw new ArgumentNullException(nameof(grant));
            if(PendingCoins<=0 && PendingDiamonds<=0)return false;
            grant(PendingCoins,PendingDiamonds);
            PendingCoins=0;PendingDiamonds=0;PendingSeconds=0;
            SavePending();
            return true;
        }
        // Preserves the legacy demo UI surface, but production uses the two-currency claim.
        public bool Claim(bool adRewardVerified, Action<long> grantCoins)
        {
            if(grantCoins==null)throw new ArgumentNullException(nameof(grantCoins));
            return Claim((gold,diamonds)=>grantCoins(checked(gold*(adRewardVerified?2L:1L))));
        }
        private void SavePending()
        {
            PlayerPrefs.SetString(PendingGoldKey,PendingCoins.ToString());
            PlayerPrefs.SetString(PendingDiamondsKey,PendingDiamonds.ToString());
            PlayerPrefs.SetString(PendingSecondsKey,PendingSeconds.ToString());
            PlayerPrefs.Save();
        }
        private static void SaveClock(long now)
        {
            PlayerPrefs.SetString(LastUtcKey,now.ToString());
            PlayerPrefs.Save();
        }
    }
}
