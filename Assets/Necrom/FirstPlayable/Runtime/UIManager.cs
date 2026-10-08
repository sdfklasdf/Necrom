using System;
using System.IO;
using Necrom.Core.Domain;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Necrom.FirstPlayable.Runtime
{
    public sealed class UIManager : MonoBehaviour
    {
        [Header("Drag scene objects here")]
        [SerializeField] private GameObject offlinePopup;
        [SerializeField] private TMP_Text offlineAmountText;
        [SerializeField] private TMP_Text coinsText;
        [SerializeField] private TMP_Text resultText;
        [SerializeField] private Button claimButton;
        [SerializeField] private Button drawButton;
        [Header("Prototype economy")]
        [SerializeField] private int gachaCost = 100;
        [SerializeField] private int firstLaunchCoins = 1000;
        [SerializeField] private int idleCoinsPerHour = 120;
        private const string CoinsKey = "NECROM_DEMO_COINS_V2";
        private const string CatalogFile = "monster_catalog";
        private OfflineRewardManager offline;
        private MonsterGachaManager gacha;
        private long coins;

        private void Awake()
        {
            coins = long.Parse(PlayerPrefs.GetString(CoinsKey, firstLaunchCoins.ToString()));
            offline = new OfflineRewardManager { CoinsPerHour = Math.Max(0, idleCoinsPerHour) };
            try
            {
                TextAsset asset = Resources.Load<TextAsset>(CatalogFile);
                if (asset == null) throw new FileNotFoundException("Resources/monster_catalog.json is missing.");
                gacha = new MonsterGachaManager(JsonUtility.FromJson<MonsterCatalogData>(asset.text));
            }
            catch (Exception ex)
            {
                Debug.LogError("Gacha initialization failed: " + ex.Message);
            }
            if (claimButton != null) claimButton.onClick.AddListener(ClaimOfflineReward);
            if (drawButton != null) drawButton.onClick.AddListener(DrawMonster);
        }

        private void Start()
        {
            offline.BeginSession();
            Refresh();
            if (offlinePopup != null) offlinePopup.SetActive(offline.PendingCoins > 0);
        }

        public void ClaimOfflineReward()
        {
            if (offline.Claim(false, GrantCoins))
            {
                if (resultText != null) resultText.text = "Offline reward collected!";
            }
            if (offlinePopup != null) offlinePopup.SetActive(false);
            Refresh();
        }

        // Only call from a rewarded-ad SDK's actual reward-completed callback.
        public void ClaimDoubleAfterVerifiedAd()
        {
            if (offline.Claim(true, GrantCoins))
            {
                if (resultText != null) resultText.text = "Double offline reward collected!";
                if (offlinePopup != null) offlinePopup.SetActive(false);
            }
            Refresh();
        }

        public void DrawMonster()
        {
            if (gacha == null)
            {
                if (resultText != null) resultText.text = "Gacha data not loaded.";
                return;
            }
            MonsterEntry reward = gacha.Draw(TrySpend, DrawCurrency.Gold, gachaCost);
            if (resultText != null)
                resultText.text = reward == null ? "Not enough coins." : ("Summoned: " + reward.name);
            Refresh();
        }

        public void ShowOfflinePopup()
        {
            Refresh();
            if (offlinePopup != null) offlinePopup.SetActive(true);
        }

        private void GrantCoins(long amount)
        {
            checked { coins += amount; }
            SaveCoins();
        }

        private bool TrySpend(DrawCurrency currency, int amount)
        {
            if (currency != DrawCurrency.Gold || amount <= 0 || coins < amount) return false;
            coins -= amount;
            SaveCoins();
            return true;
        }

        private void SaveCoins()
        {
            PlayerPrefs.SetString(CoinsKey, coins.ToString());
            PlayerPrefs.Save();
        }

        private void Refresh()
        {
            if (coinsText != null) coinsText.text = "Coins: " + coins;
            if (offlineAmountText != null)
                offlineAmountText.text = "Offline coins: " + (offline == null ? 0 : offline.PendingCoins);
            if (claimButton != null) claimButton.interactable = offline != null && offline.PendingCoins > 0;
            if (drawButton != null) drawButton.interactable = gacha != null && coins >= gachaCost;
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused && offline != null) offline.RecordExit();
        }
        private void OnApplicationQuit()
        {
            if (offline != null) offline.RecordExit();
        }
    }
}