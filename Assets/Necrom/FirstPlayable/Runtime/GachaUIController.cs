using System;
using System.Collections;
using System.Collections.Generic;
using Necrom.Core.Domain;
using UnityEngine;
using UnityEngine.UI;

namespace Necrom.FirstPlayable.Runtime
{
    // V2 standalone presenter: safe when legacy NecromDemoGameSystems/UIManager is inactive.
    [DisallowMultipleComponent]
    public sealed class GachaUIController : MonoBehaviour
    {
        private const string DiamondKey="NECROM_DEMO_DIAMONDS_V2";
        private const string PityKey="NECROM_GACHA_PITY_V1";
        [SerializeField] private int singleDrawCost=100;
        [SerializeField] private float coffinDropSeconds=1.5f;
        private MonsterGachaManager gacha;
        private long diamonds;
        private Canvas canvas;
        private GameObject popup;
        private Text status,diamondText;
        private Button single,ten,skip,open;
        private Coroutine sequence;
        private IReadOnlyList<GachaResult> pending;
        private bool revealing;
        private bool ready;

        private void Start()
        {
            try
            {
                var source=Resources.Load<TextAsset>("monster_catalog");
                if(source==null)throw new InvalidOperationException("120-monster catalog missing");
                var data=JsonUtility.FromJson<MonsterCatalogData>(source.text);
                if(data?.monsters==null || data.monsters.Length!=120)
                    throw new InvalidOperationException("Unexpected gacha pool size");
                gacha=new MonsterGachaManager(data,PermanentRosterRuntime.GetOrCreate(data),new System.Random());
                diamonds=long.TryParse(PlayerPrefs.GetString(DiamondKey,"0"),out var saved)&&saved>=0?saved:0;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                // Development-only funding. Never issue free premium currency in production.
                if(diamonds<=10000)
                {
                    diamonds=100000;
                    PlayerPrefs.SetString(DiamondKey,diamonds.ToString());
                    PlayerPrefs.Save();
                    Debug.Log("[NECRO DEBUG] Test diamonds set to 100000");
                }
#endif
                int pity=PlayerPrefs.GetInt(PityKey,0);
                gacha.RestorePityCounter(pity);
                BuildOverlay();
                ready=true;
            }
            catch(Exception e){Debug.LogError("Gacha UI initialization refused: "+e);}
        }
        private bool Spend(DrawCurrency currency,int total)
        {
            if(currency!=DrawCurrency.Diamond || total<=0 || diamonds<total)return false;
            diamonds-=total;
            PlayerPrefs.SetString(DiamondKey,diamonds.ToString());
            PlayerPrefs.Save();
            return true;
        }
        public void Show() { if(ready && popup!=null){popup.SetActive(true);Refresh();} }
        public void Hide() { if(popup!=null && !revealing)popup.SetActive(false); }
        public void DrawOnce()=>Request(1);
        public void DrawTen()=>Request(10);
        private void Request(int count)
        {
            if(!ready || revealing || gacha==null || (count!=1 && count!=10))return;
            try
            {
                var results=gacha.DrawBatch(Spend,DrawCurrency.Diamond,singleDrawCost,count);
                if(results==null){SetStatus("다이아가 부족합니다");Refresh();return;}
                // Persist award and pity before visual delay/skip/closing the app.
                PermanentRosterSaveManager.Save(gacha.Roster);
                PlayerPrefs.SetInt(PityKey,gacha.DrawsSinceLegendary);
                PlayerPrefs.Save();
                pending=results;
                revealing=true;
                SetStatus("관짝 낙하 중...");
                Debug.Log("[연출: 관짝 낙하 중...]");
                Refresh();
                sequence=StartCoroutine(RevealAfterDelay());
            }
            catch(Exception e){Debug.LogError("Gacha draw interrupted: "+e);revealing=false;Refresh();}
        }
        private IEnumerator RevealAfterDelay()
        {
            yield return new WaitForSecondsRealtime(Mathf.Max(.1f,coffinDropSeconds));
            Reveal();
        }
        public void SkipReveal()
        {
            if(!revealing)return;
            if(sequence!=null)StopCoroutine(sequence);
            sequence=null;
            Reveal();
        }
        private void Reveal()
        {
            if(!revealing)return;
            revealing=false;
            sequence=null;
            Debug.Log("[연출: 뚜껑 열림! 결과 출력]");
            if(pending!=null)
            {
                var lines=new List<string>();
                foreach(var r in pending)
                    lines.Add(r.Monster.name+" ["+r.Rarity+"] "+(r.FirstAcquisition?"신규 획득":"중복 → 조각 +1"));
                SetStatus(string.Join("\n",lines));
            }
            pending=null;Refresh();
        }
        private void SetStatus(string s){if(status!=null)status.text=s;}
        private static Text AddText(Transform parent,string value,int size,Vector2 anchor,Vector2 position,Vector2 bounds)
        {
            var o=new GameObject("Text",typeof(RectTransform),typeof(Text));o.transform.SetParent(parent,false);
            var r=o.GetComponent<RectTransform>();r.anchorMin=r.anchorMax=anchor;r.anchoredPosition=position;r.sizeDelta=bounds;
            var t=o.GetComponent<Text>();t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.text=value;t.fontSize=size;t.color=Color.white;t.alignment=TextAnchor.MiddleCenter;return t;
        }
        private static Button AddButton(Transform parent,string name,string title,Vector2 anchor,Vector2 position,Action action)
        {
            var o=new GameObject(name,typeof(RectTransform),typeof(Image),typeof(Button));o.transform.SetParent(parent,false);
            var r=o.GetComponent<RectTransform>();r.anchorMin=r.anchorMax=anchor;r.anchoredPosition=position;r.sizeDelta=new Vector2(160,44);
            o.GetComponent<Image>().color=new Color(.24f,.39f,.62f,.97f);
            var b=o.GetComponent<Button>();b.onClick.AddListener(()=>action());
            AddText(o.transform,title,16,new Vector2(.5f,.5f),Vector2.zero,new Vector2(152,40));return b;
        }
        private void BuildOverlay()
        {
            var parent=GetComponentInParent<Canvas>();
            if(parent==null)parent=FindObjectOfType<Canvas>();
            var go=new GameObject("GachaV2Canvas",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            go.transform.SetParent(parent!=null?parent.transform:transform,false);
            canvas=go.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.overrideSorting=true;canvas.sortingOrder=30000;
            var root=go.GetComponent<RectTransform>();root.anchorMin=Vector2.zero;root.anchorMax=Vector2.one;root.offsetMin=root.offsetMax=Vector2.zero;
            open=AddButton(go.transform,"OpenGacha","가챠",new Vector2(1,1),new Vector2(-95,-65),Show);
            popup=new GameObject("CoffinGachaPopup",typeof(RectTransform),typeof(Image));
            popup.transform.SetParent(go.transform,false);
            var panel=popup.GetComponent<RectTransform>();panel.anchorMin=Vector2.zero;panel.anchorMax=Vector2.one;panel.offsetMin=panel.offsetMax=Vector2.zero;
            popup.GetComponent<Image>().color=new Color(.04f,.04f,.10f,.94f);
            diamondText=AddText(popup.transform,"다이아: 0",19,new Vector2(.5f,.5f),new Vector2(0,190),new Vector2(340,45));
            status=AddText(popup.transform,"관짝 소환",16,new Vector2(.5f,.5f),new Vector2(0,45),new Vector2(390,255));
            single=AddButton(popup.transform,"SingleDraw","1회 뽑기",new Vector2(.5f,.5f),new Vector2(-90,-120),DrawOnce);
            ten=AddButton(popup.transform,"TenDraw","10회 뽑기",new Vector2(.5f,.5f),new Vector2(90,-120),DrawTen);
            skip=AddButton(popup.transform,"SkipReveal","스킵",new Vector2(.5f,.5f),new Vector2(0,-175),SkipReveal);
            AddButton(popup.transform,"Close","닫기",new Vector2(.5f,.5f),new Vector2(0,-235),Hide);
            popup.SetActive(false);
        }
        private void Refresh()
        {
            if(diamondText!=null)diamondText.text="다이아: "+diamonds.ToString("N0")+"  |  전설 천장 "+(gacha?.DrawsSinceLegendary??0)+"/200";
            if(single!=null)single.interactable=!revealing&&diamonds>=singleDrawCost;
            if(ten!=null)ten.interactable=!revealing&&diamonds>=10L*singleDrawCost;
            if(skip!=null)skip.interactable=revealing;
        }
        private void OnDisable(){if(revealing)SkipReveal();}
        private void OnDestroy(){if(canvas!=null)Destroy(canvas.gameObject);}
    }
}
