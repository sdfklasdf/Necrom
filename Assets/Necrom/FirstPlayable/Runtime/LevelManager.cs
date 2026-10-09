using System;
using Necrom.Core.Domain;
using UnityEngine;

namespace Necrom.FirstPlayable.Runtime
{
    [DisallowMultipleComponent]
    public sealed class LevelManager : MonoBehaviour
    {
        private const string LevelKey = "NECROM_ACCOUNT_LEVEL_V1";
        private const string ExpKey = "NECROM_ACCOUNT_EXP_V1";
        private const string PendingSpKey = "NECROM_ACCOUNT_PENDING_SP_V1";
        private AccountProgression progression;
        private SkillTreeUIController skillTree;
        public int Level => progression.Level;
        public long Experience => progression.Experience;
        public long RequiredExperience => AccountProgression.RequiredExperience(Level);
        public float Progress => (float)progression.Percent;
        public event Action Changed;

        private void Awake()
        {
            int level = PlayerPrefs.GetInt(LevelKey, 1);
            long exp;
            if (!long.TryParse(PlayerPrefs.GetString(ExpKey, "0"), out exp)) exp = -1;
            try { progression = new AccountProgression(level, exp); }
            catch (Exception exception)
            {
                Debug.LogError("[AWU-20] Invalid account save; original values preserved: " + exception);
                enabled = false;
                return;
            }
        }
        public void BindSkillTree(SkillTreeUIController tree)
        {
            skillTree = tree ?? throw new ArgumentNullException(nameof(tree));
            FlushPendingSp();
        }
        private void FlushPendingSp()
        {
            if (skillTree == null || !skillTree.IsReady) return;
            int pending = PlayerPrefs.GetInt(PendingSpKey, 0);
            if (pending <= 0) return;
            skillTree.GrantLevelSp(pending);
            PlayerPrefs.SetInt(PendingSpKey, 0);
            PlayerPrefs.Save();
        }
        public void RecordEnemyDefeat(int waveNumber)
        {
            if (!enabled || progression == null) return;
            int gained = progression.GrantExperience(AccountProgression.ExperienceForKill(waveNumber));
            if (gained > 0)
            {
                int pending = PlayerPrefs.GetInt(PendingSpKey, 0);
                PlayerPrefs.SetInt(PendingSpKey, checked(pending + gained));
            }
            Save();
            FlushPendingSp();
            Changed?.Invoke();
        }
        private void Save()
        {
            PlayerPrefs.SetInt(LevelKey, Level);
            PlayerPrefs.SetString(ExpKey, Experience.ToString(System.Globalization.CultureInfo.InvariantCulture));
            PlayerPrefs.Save();
        }
        private void OnApplicationPause(bool paused) { if (paused && progression != null) Save(); }
        private void OnApplicationQuit() { if (progression != null) Save(); }
    }
}
