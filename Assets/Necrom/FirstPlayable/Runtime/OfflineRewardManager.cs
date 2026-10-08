using System;
using UnityEngine;
namespace Necrom.Core.Domain
{
    // Local prototype. Server-authoritative time and grant ledger are needed before release.
    public sealed class OfflineRewardManager
    {
        const string LastUtcKey = "NECROM_OFFLINE_LAST_UTC_V2";
        const string PendingKey = "NECROM_OFFLINE_PENDING_V2";
        public const int MaxOfflineSeconds = 8 * 3600;
        public long CoinsPerHour { get; set; } = 120;
        public long PendingCoins { get; private set; }
        public int EarnedSeconds { get; private set; }
        public void BeginSession()
        {
            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            long last;
            string stored = PlayerPrefs.GetString(LastUtcKey, "");
            if (!long.TryParse(stored, out last)) last = now;
            long elapsed = Math.Max(0, Math.Min(MaxOfflineSeconds, now - last));
            EarnedSeconds = (int)elapsed;
            long earned = checked(elapsed * Math.Max(0, CoinsPerHour) / 3600);
            // A previously calculated, unclaimed reward survives a restart.
            long pending;
            if (!long.TryParse(PlayerPrefs.GetString(PendingKey, "0"), out pending)) pending = 0;
            PendingCoins = Math.Max(0, pending);
            PendingCoins = checked(PendingCoins + earned);
            PlayerPrefs.SetString(PendingKey, PendingCoins.ToString());
            SaveClock(now);
        }
        public void RecordExit()
        {
            SaveClock(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        }
        // Invoke only after a verified rewarded-ad completion callback.
        public bool Claim(bool adRewardVerified, Action<long> grantCoins)
        {
            if (grantCoins == null) throw new ArgumentNullException(nameof(grantCoins));
            if (PendingCoins <= 0) return false;
            long payout = checked(PendingCoins * (adRewardVerified ? 2L : 1L));
            grantCoins(payout); // must be idempotent + persisted atomically in production
            PendingCoins = 0;
            PlayerPrefs.SetString(PendingKey, "0");
            PlayerPrefs.Save();
            return true;
        }
        void SaveClock(long timestamp)
        {
            PlayerPrefs.SetString(LastUtcKey, timestamp.ToString());
            PlayerPrefs.Save();
        }
    }
}