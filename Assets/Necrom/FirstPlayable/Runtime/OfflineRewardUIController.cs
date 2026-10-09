using System;
using Necrom.Core.Domain;
using UnityEngine;
using UnityEngine.UI;

namespace Necrom.FirstPlayable.Runtime
{
    // Active FirstPlayable HUD owns this presenter; legacy UIManager's demo object is disabled.
    [DisallowMultipleComponent]
    public sealed class OfflineRewardUIController : MonoBehaviour
    {
        private const string GoldKey="NECROM_DEMO_COINS_V2";
        private const string DiamondsKey="NECROM_DEMO_DIAMONDS_V2";
        private OfflineRewardManager rewards;
        private GameObject panel;
        private Canvas canvas;
        private Text summary;
        private Button claim;
        private bool closing;

        private void Start()
        {
            try
            {
                rewards=new OfflineRewardManager();
                rewards.BeginSession();
                BuildUI();
                ShowPending();
            }
            catch(Exception e){Debug.LogError("[AWU-18] Offline reward initialization failed: "+e);enabled=false;}
        }
        private static Text MakeText(Transform parent,string value,int fontSize,Vector2 position,Vector2 size)
        {
            var go=new GameObject("Label",typeof(RectTransform),typeof(Text));
            go.transform.SetParent(parent,false);
            var rect=go.GetComponent<RectTransform>();
            rect.anchorMin=rect.anchorMax=new Vector2(.5f,.5f);
            rect.anchoredPosition=position;rect.sizeDelta=size;
            var t=go.GetComponent<Text>();
            t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.text=value;t.fontSize=fontSize;t.alignment=TextAnchor.MiddleCenter;
            t.color=Color.white;t.raycastTarget=false;
            return t;
        }
        private void BuildUI()
        {
            var root=new GameObject("OfflineRewardsCanvas",typeof(RectTransform),typeof(Canvas),
                typeof(CanvasScaler),typeof(GraphicRaycaster));
            root.transform.SetParent(transform,false);
            canvas=root.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;
            canvas.overrideSorting=true;canvas.sortingOrder=30050;
            var scaler=root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution=new Vector2(390,844);
            var rect=root.GetComponent<RectTransform>();
            rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;
            rect.offsetMin=rect.offsetMax=Vector2.zero;
            panel=new GameObject("WelcomeBackRewardPopup",typeof(RectTransform),typeof(Image));
            panel.transform.SetParent(root.transform,false);
            var pane=panel.GetComponent<RectTransform>();
            pane.anchorMin=Vector2.zero;pane.anchorMax=Vector2.one;
            pane.offsetMin=pane.offsetMax=Vector2.zero;
            panel.GetComponent<Image>().color=new Color(.04f,.05f,.12f,.97f);
            summary=MakeText(panel.transform,"방치 보상 정산",23,new Vector2(0,90),new Vector2(360,300));
            var buttonObj=new GameObject("ClaimOfflineRewards",typeof(RectTransform),typeof(Image),typeof(Button));
            buttonObj.transform.SetParent(panel.transform,false);
            var buttonRect=buttonObj.GetComponent<RectTransform>();
            buttonRect.anchorMin=buttonRect.anchorMax=new Vector2(.5f,.5f);
            buttonRect.anchoredPosition=new Vector2(0,-135);
            buttonRect.sizeDelta=new Vector2(235,58);
            buttonObj.GetComponent<Image>().color=new Color(.20f,.67f,.48f,1);
            claim=buttonObj.GetComponent<Button>();
            claim.onClick.AddListener(Claim);
            MakeText(buttonObj.transform,"보상 수령",20,Vector2.zero,new Vector2(225,54));
            panel.SetActive(false);
        }
        private void ShowPending()
        {
            if(panel==null || rewards==null || rewards.PendingSeconds<OfflineRewardManager.MinimumRewardSeconds ||
                (rewards.PendingCoins<=0 && rewards.PendingDiamonds<=0))return;
            summary.text="방치 보상 정산\n\n오프라인 "+(rewards.PendingSeconds/3600)+"시간 "+
                ((rewards.PendingSeconds%3600)/60)+"분 "+(rewards.PendingSeconds%60)+"초\n골드 +"+
                rewards.PendingCoins.ToString("N0")+"\n다이아 +"+
                rewards.PendingDiamonds.ToString("N0");
            closing=false;claim.interactable=true;
            panel.SetActive(true);
        }
        private void Claim()
        {
            if(closing || rewards==null)return;
            closing=true;claim.interactable=false;
            try
            {
                bool claimed=rewards.Claim((gold,diamond)=>{
                    long currentGold=ReadWallet(GoldKey);
                    long currentDiamonds=ReadWallet(DiamondsKey);
                    PlayerPrefs.SetString(GoldKey,checked(currentGold+gold).ToString());
                    PlayerPrefs.SetString(DiamondsKey,checked(currentDiamonds+diamond).ToString());
                    PlayerPrefs.Save();
                });
                if(claimed)Debug.Log("[AWU-18] Offline gold and diamonds granted.");
                panel.SetActive(false);
            }
            catch(Exception e)
            {
                Debug.LogError("[AWU-18] Claim failed; pending rewards retained: "+e);
                closing=false;claim.interactable=true;
            }
        }
        private static long ReadWallet(string key) => long.TryParse(PlayerPrefs.GetString(key,"0"),out var v) && v>=0?v:0;
        private void OnApplicationPause(bool paused)
        {
            if(rewards==null)return;
            if(paused)rewards.RecordExit();
            else { rewards.BeginSession();ShowPending(); }
        }
        private void OnApplicationQuit(){if(rewards!=null)rewards.RecordExit();}
        private void OnDestroy(){if(canvas!=null)Destroy(canvas.gameObject);}
    }
}
