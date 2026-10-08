using System;
using System.IO;
using Necrom.Core.Domain;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Necrom.FirstPlayable.Runtime
{
    // Prototype presentation only. No changes to FirstPlayable.unity or canonical combat ownership.
    public sealed class UIManager : MonoBehaviour
    {
        [SerializeField] private GameObject offlinePopup;
        [SerializeField] private TMP_Text offlineAmountText, coinsText, resultText;
        [SerializeField] private Button claimButton, drawButton;
        [SerializeField] private int gachaCost = 100;
        [SerializeField] private int firstLaunchCoins = 1000;
        [SerializeField] private int idleCoinsPerHour = 120;
        private const string CoinsKey = "NECROM_DEMO_COINS_V2";
        private const string DiamondsKey = "NECROM_DEMO_DIAMONDS_V2";
        private static UIManager active;
        public static PermanentMonsterRoster ActiveRoster => active != null && active.enabled ? active.gacha?.Roster : null;
        private OfflineRewardManager offline;
        private MonsterGachaManager gacha;
        private AutoDeckRecommender autoDeck;
        private MonsterEntry[] monsterCatalog;
        private long coins, diamonds;
        private Canvas gachaCanvas;
        private GameObject popupRoot;
        private Button confirmDrawButton;

        private void Awake()
        {
            if(active != null && active != this) { Debug.LogError("Duplicate V2 UIManager refused."); enabled=false; return; }
            active=this;
            if(!long.TryParse(PlayerPrefs.GetString(CoinsKey,firstLaunchCoins.ToString()),out coins) || coins<0) coins=0;
            if(!long.TryParse(PlayerPrefs.GetString(DiamondsKey,"0"),out diamonds) || diamonds<0) diamonds=0;
            offline=new OfflineRewardManager { CoinsPerHour=Math.Max(0,idleCoinsPerHour) };
            try
            {
                var asset=Resources.Load<TextAsset>("monster_catalog");
                if(asset==null)throw new FileNotFoundException("Resources/monster_catalog.json is missing.");
                var catalog=JsonUtility.FromJson<MonsterCatalogData>(asset.text);
                var restored=PermanentRosterRuntime.GetOrCreate(catalog);
                gacha=new MonsterGachaManager(catalog,restored,new System.Random());
                monsterCatalog=catalog.monsters;
                var rulesAsset=Resources.Load<TextAsset>("synergy_rules_v2");
                if(rulesAsset!=null)autoDeck=new AutoDeckRecommender(
                    new SynergyManager(JsonUtility.FromJson<SynergyCatalog>(rulesAsset.text)));
            }
            catch(Exception ex) { Debug.LogError("Gacha initialization failed: "+ex); }
            if(claimButton!=null)claimButton.onClick.AddListener(ClaimOfflineReward);
            if(drawButton!=null)drawButton.onClick.AddListener(ShowGachaPopup);
        }
        private void Start()
        {
            if(!enabled)return;
            offline.BeginSession();
            Refresh();
            if(offlinePopup!=null)offlinePopup.SetActive(offline.PendingCoins>0);
        }
        public void ClaimOfflineReward()
        {
            if(offline!=null && offline.Claim(false,GrantCoins) && resultText!=null)
                resultText.text="Offline gold collected.";
            if(offlinePopup!=null)offlinePopup.SetActive(false);
            Refresh();
        }
        public void ClaimDoubleAfterVerifiedAd()
        {
            if(offline!=null && offline.Claim(true,GrantCoins))
            {
                if(resultText!=null)resultText.text="Double offline gold collected.";
                if(offlinePopup!=null)offlinePopup.SetActive(false);
            }
            Refresh();
        }
        public void ShowGachaPopup()
        {
            if(!enabled || gacha==null)return;
            if(popupRoot==null)BuildOverlay();
            popupRoot.SetActive(true);
            Refresh();
        }
        public void HideGachaPopup() { if(popupRoot!=null)popupRoot.SetActive(false); }
        private void BuildOverlay()
        {
            var root=new GameObject("V2GachaOverlay",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            root.transform.SetParent(transform.root,false);
            gachaCanvas=root.GetComponent<Canvas>();
            gachaCanvas.renderMode=RenderMode.ScreenSpaceOverlay;
            gachaCanvas.overrideSorting=true;
            // Keep V2 independent of the existing combat HUD and display on top.
            gachaCanvas.sortingOrder=30000;
            var rect=root.GetComponent<RectTransform>();
            rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;
            rect.offsetMin=Vector2.zero;rect.offsetMax=Vector2.zero;
            popupRoot=root;
            var shade=new GameObject("DimmedCombatBackdrop",typeof(RectTransform),typeof(Image));
            shade.transform.SetParent(root.transform,false);
            var shadeRect=shade.GetComponent<RectTransform>();
            shadeRect.anchorMin=Vector2.zero;shadeRect.anchorMax=Vector2.one;
            shadeRect.offsetMin=Vector2.zero;shadeRect.offsetMax=Vector2.zero;
            shade.GetComponent<Image>().color=new Color(0.03f,0.04f,0.09f,0.92f);
            CreateOverlayButton(root.transform,"ConfirmDiamondDraw",new Vector2(0,-30),DrawMonster,out confirmDrawButton);
            CreateOverlayButton(root.transform,"CloseGacha",new Vector2(0,-130),HideGachaPopup,out _);
        }
        private static void CreateOverlayButton(Transform parent,string name,Vector2 position,UnityEngine.Events.UnityAction action,out Button button)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(Image),typeof(Button));
            go.transform.SetParent(parent,false);
            var r=go.GetComponent<RectTransform>();
            r.anchorMin=r.anchorMax=new Vector2(.5f,.5f);
            r.sizeDelta=new Vector2(260,70);r.anchoredPosition=position;
            go.GetComponent<Image>().color=new Color(.23f,.35f,.64f,1f);
            button=go.GetComponent<Button>();
            button.onClick.AddListener(action);
            // Temporary click target, not final branded art; replace via Figma-derived prefab later.
            var label=new GameObject("Label",typeof(RectTransform),typeof(Text));
            label.transform.SetParent(go.transform,false);
            var lr=label.GetComponent<RectTransform>();
            lr.anchorMin=Vector2.zero;lr.anchorMax=Vector2.one;
            lr.offsetMin=Vector2.zero;lr.offsetMax=Vector2.zero;
            var text=label.GetComponent<Text>();
            text.text=name=="CloseGacha"?"Close":"Draw (Diamonds)";
            text.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize=22;text.alignment=TextAnchor.MiddleCenter;text.color=Color.white;
        }
        // Public Unity UI hook: attach buttons for power/synergy without editing the canonical scene.
        public void AutoEquipStrongest() => ApplyAutoDeck(AutoDeckGoal.CombatPower);
        public void AutoEquipBestSynergy() => ApplyAutoDeck(AutoDeckGoal.MaximumSynergy);
        private void ApplyAutoDeck(AutoDeckGoal goal)
        {
            if(gacha==null || autoDeck==null || monsterCatalog==null)return;
            try
            {
                var recommendation=autoDeck.Recommend(gacha.Roster,monsterCatalog,goal);
                if(!autoDeck.Apply(gacha.Roster,recommendation))throw new InvalidOperationException("Deck apply failed");
                PermanentRosterSaveManager.Save(gacha.Roster);
                if(resultText!=null)resultText.text="Deck assigned: "+recommendation.MonsterIds.Count+" units ("+goal+")";
            }
            catch(Exception ex){Debug.LogError("Auto-deck failed: "+ex.Message);}
        }
        public void DrawMonster()
        {
            if(gacha==null)return;
            var reward=gacha.DrawDetailed(TrySpend,DrawCurrency.Diamond,gachaCost);
            if(reward!=null)PermanentRosterSaveManager.Save(gacha.Roster);
            if(resultText!=null)resultText.text=reward==null?"Not enough diamonds.":"Summoned: "+reward.Monster.name+" ("+reward.Rarity+")";
            Refresh();
        }
        public void ShowOfflinePopup()
        {
            Refresh();if(offlinePopup!=null)offlinePopup.SetActive(true);
        }
        private void GrantCoins(long amount) { checked { coins+=amount; } Save(); }
        private bool TrySpend(DrawCurrency currency,int amount)
        {
            if(currency!=DrawCurrency.Diamond || amount<=0 || diamonds<amount)return false;
            diamonds-=amount;Save();return true;
        }
        private void Save()
        {
            PlayerPrefs.SetString(CoinsKey,coins.ToString());
            PlayerPrefs.SetString(DiamondsKey,diamonds.ToString());
            PlayerPrefs.Save();
        }
        private void Refresh()
        {
            if(coinsText!=null)coinsText.text="Gold: "+coins+"  Diamonds: "+diamonds;
            if(offlineAmountText!=null)offlineAmountText.text="Offline gold: "+(offline==null?0:offline.PendingCoins);
            if(claimButton!=null)claimButton.interactable=offline!=null && offline.PendingCoins>0;
            if(drawButton!=null)drawButton.interactable=gacha!=null;
            if(confirmDrawButton!=null)confirmDrawButton.interactable=gacha!=null && diamonds>=gachaCost;
        }
        private void OnApplicationPause(bool paused) { if(paused && offline!=null)offline.RecordExit(); }
        private void OnApplicationQuit() { if(offline!=null)offline.RecordExit(); }
        private void OnDestroy()
        {
            if(active==this)active=null;
            if(claimButton!=null)claimButton.onClick.RemoveListener(ClaimOfflineReward);
            if(drawButton!=null)drawButton.onClick.RemoveListener(ShowGachaPopup);
            if(popupRoot!=null)Destroy(popupRoot);
        }
    }
}
