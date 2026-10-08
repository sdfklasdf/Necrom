using System;
using System.Collections.Generic;
using System.Linq;
using Necrom.Core.Domain;
using UnityEngine;
using UnityEngine.UI;

namespace Necrom.FirstPlayable.Runtime
{
    // Independent overlay; never touches the Raise Formation (five temporary allies).
    [DisallowMultipleComponent]
    public sealed class DeckFormationUIController : MonoBehaviour
    {
        private PermanentMonsterRoster roster;
        private MonsterCatalogData catalog;
        private AutoDeckRecommender recommender;
        private PermanentDeckCombatSpawner spawner;
        private Canvas canvas;
        private GameObject popup;
        private RectTransform slotsHost, ownedHost;
        private Text notice;
        private int selectedSlot;
        private bool dirty;
        private bool ready;
        private readonly List<GameObject> items=new List<GameObject>();

        private void Start()
        {
            try
            {
                var monsters=Resources.Load<TextAsset>("monster_catalog");
                var synergies=Resources.Load<TextAsset>("synergy_rules_v2");
                if(monsters==null || synergies==null)throw new InvalidOperationException("Missing formation catalogs");
                catalog=JsonUtility.FromJson<MonsterCatalogData>(monsters.text);
                if(catalog?.monsters==null || catalog.monsters.Length!=120)throw new InvalidOperationException("Invalid monster catalog");
                var rules=JsonUtility.FromJson<SynergyCatalog>(synergies.text);
                recommender=new AutoDeckRecommender(new SynergyManager(rules));
                roster=PermanentRosterRuntime.GetOrCreate(catalog);
                spawner=GetComponent<PermanentDeckCombatSpawner>();
                if(spawner==null)throw new InvalidOperationException("PermanentDeckCombatSpawner missing");
                BuildUI();ready=true;
            }
            catch(Exception e){Debug.LogError("[NECRO AWU-12] Deck UI disabled: "+e);enabled=false;}
        }
        private static Text Label(Transform parent,string value,int size,Vector2 anchor,Vector2 pos,Vector2 dimensions)
        {
            var go=new GameObject("Label",typeof(RectTransform),typeof(Text));go.transform.SetParent(parent,false);
            var r=go.GetComponent<RectTransform>();r.anchorMin=r.anchorMax=anchor;r.anchoredPosition=pos;r.sizeDelta=dimensions;
            var t=go.GetComponent<Text>();t.text=value;t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.fontSize=size;t.alignment=TextAnchor.MiddleCenter;t.color=Color.white;t.raycastTarget=false;return t;
        }
        private static Button Button(Transform parent,string name,string title,Vector2 anchor,Vector2 pos,Vector2 size,Action click)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(Image),typeof(Button));go.transform.SetParent(parent,false);
            var r=go.GetComponent<RectTransform>();r.anchorMin=r.anchorMax=anchor;r.anchoredPosition=pos;r.sizeDelta=size;
            go.GetComponent<Image>().color=new Color(.17f,.36f,.62f,.96f);
            var b=go.GetComponent<Button>();b.onClick.AddListener(()=>click());
            Label(go.transform,title,14,new Vector2(.5f,.5f),Vector2.zero,size-new Vector2(6,6));
            return b;
        }
        private void BuildUI()
        {
            var root=new GameObject("DeckFormationCanvas",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            root.transform.SetParent(transform,false);
            canvas=root.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;
            canvas.overrideSorting=true;canvas.sortingOrder=30010;
            var rect=root.GetComponent<RectTransform>();rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;
            Button(root.transform,"OpenDeckFormation","덱 편성",new Vector2(1,1),new Vector2(-95,-120),new Vector2(160,42),Open);
            popup=new GameObject("DeckFormationFullscreen",typeof(RectTransform),typeof(Image));
            popup.transform.SetParent(root.transform,false);
            var panel=popup.GetComponent<RectTransform>();panel.anchorMin=Vector2.zero;panel.anchorMax=Vector2.one;panel.offsetMin=panel.offsetMax=Vector2.zero;
            popup.GetComponent<Image>().color=new Color(.02f,.04f,.09f,.96f);
            Label(popup.transform,"영구 몬스터 덱 편성 (Raise와 별개)",18,new Vector2(.5f,1),new Vector2(0,-32),new Vector2(460,45));
            slotsHost=new GameObject("ActiveSlots",typeof(RectTransform)).GetComponent<RectTransform>();
            slotsHost.SetParent(popup.transform,false);slotsHost.anchorMin=slotsHost.anchorMax=new Vector2(.5f,.78f);slotsHost.sizeDelta=new Vector2(350,128);
            var viewport=new GameObject("OwnedViewport",typeof(RectTransform),typeof(Image),typeof(Mask),typeof(ScrollRect));
            viewport.transform.SetParent(popup.transform,false);
            var viewRect=viewport.GetComponent<RectTransform>();viewRect.anchorMin=viewRect.anchorMax=new Vector2(.5f,.43f);
            viewRect.sizeDelta=new Vector2(350,245);
            viewport.GetComponent<Image>().color=new Color(.10f,.14f,.22f,.6f);
            viewport.GetComponent<Mask>().showMaskGraphic=true;
            ownedHost=new GameObject("OwnedListContent",typeof(RectTransform)).GetComponent<RectTransform>();
            ownedHost.SetParent(viewport.transform,false);
            ownedHost.anchorMin=new Vector2(.5f,1);ownedHost.anchorMax=new Vector2(.5f,1);
            ownedHost.pivot=new Vector2(.5f,1);ownedHost.anchoredPosition=Vector2.zero;
            ownedHost.sizeDelta=new Vector2(340,245);
            var scroll=viewport.GetComponent<ScrollRect>();scroll.viewport=viewRect;scroll.content=ownedHost;
            scroll.horizontal=false;scroll.vertical=true;scroll.movementType=ScrollRect.MovementType.Clamped;
            notice=Label(popup.transform,"슬롯을 선택한 뒤 보유 몬스터를 누르세요",13,new Vector2(.5f,.20f),Vector2.zero,new Vector2(520,44));
            Button(popup.transform,"AutoPower","전투력 자동 편성",new Vector2(.5f,.15f),new Vector2(-102,0),new Vector2(190,45),AutoEquipStrongest);
            Button(popup.transform,"AutoSynergy","시너지 자동 편성",new Vector2(.5f,.15f),new Vector2(102,0),new Vector2(190,45),AutoEquipBestSynergy);
            Button(popup.transform,"CloseDeck","저장 후 전투 복귀",new Vector2(.5f,.05f),Vector2.zero,new Vector2(260,45),Close);
            popup.SetActive(false);
        }
        public void Open(){if(!ready)return;popup.SetActive(true);Rebuild();}
        private void Rebuild()
        {
            foreach(var go in items)if(go!=null)Destroy(go);
            items.Clear();
            selectedSlot=Mathf.Clamp(selectedSlot,0,roster.UnlockedSlots-1);
            for(int i=0;i<6;i++)
            {
                int slot=i;string id=roster.GetDeckSlot(i);
                var title=i>=roster.UnlockedSlots?"잠김":string.IsNullOrEmpty(id)?"비어 있음":id.Replace("MON_","#");
                var b=Button(slotsHost,"Slot"+i,(i+1)+"\n"+title,new Vector2(.5f,.5f),new Vector2((i%3-1)*108,30-(i/3)*62),new Vector2(100,54),()=>SelectSlot(slot));
                b.interactable=i<roster.UnlockedSlots;
                if(i==selectedSlot)b.GetComponent<Image>().color=new Color(.10f,.65f,.55f,1);
                items.Add(b.gameObject);
            }
            var ordered=roster.OwnedIds.OrderBy(x=>x,StringComparer.Ordinal).ToArray();
            Debug.Log("[NECRO AWU-12] Deck editor owned count="+ordered.Length);
            var nameById=catalog.monsters.ToDictionary(m=>m.id,m=>m.name,StringComparer.Ordinal);
            ownedHost.sizeDelta=new Vector2(340,Mathf.Max(245,Mathf.CeilToInt(ordered.Length/2f)*49+16));
            for(int i=0;i<ordered.Length;i++)
            {
                string id=ordered[i];
                string name=nameById.TryGetValue(id,out var value)?value:id;
                var b=Button(ownedHost,"Owned"+i,name+" ★"+roster.GetStars(id),new Vector2(.5f,1f),
                    new Vector2((i%2-.5f)*168,-26-(i/2)*49),new Vector2(160,42),()=>Assign(id));
                items.Add(b.gameObject);
            }
            notice.text="보유 "+ordered.Length+"종 | 출전 "+Enumerable.Range(0,roster.UnlockedSlots).Count(i=>roster.GetDeckSlot(i)!=null)+" / "+roster.UnlockedSlots+
                " | 선택 슬롯 "+(selectedSlot+1);
        }
        private void SelectSlot(int slot){if(slot>=roster.UnlockedSlots)return;selectedSlot=slot;Rebuild();}
        private void Assign(string id)
        {
            if(!roster.Owns(id))return;
            for(int i=0;i<6;i++)if(i!=selectedSlot&&roster.GetDeckSlot(i)==id)roster.Clear(i);
            if(roster.Assign(selectedSlot,id)){dirty=true;Rebuild();}
        }
        public void AutoEquipStrongest()=>AutoEquip(AutoDeckGoal.CombatPower);
        public void AutoEquipBestSynergy()=>AutoEquip(AutoDeckGoal.MaximumSynergy);
        private void AutoEquip(AutoDeckGoal goal)
        {
            if(!ready)return;
            try
            {
                var recommendation=recommender.Recommend(roster,catalog.monsters,goal);
                if(!recommender.Apply(roster,recommendation))throw new InvalidOperationException("Invalid recommendation");
                dirty=true;Rebuild();
            }
            catch(Exception ex){Debug.LogError("[NECRO AWU-12] Auto equip: "+ex.Message);notice.text="편성 실패: "+ex.Message;}
        }
        public void Close()
        {
            if(!ready || popup==null)return;
            if(dirty)
            {
                try
                {
                    PermanentRosterSaveManager.Save(roster);
                    spawner.RefreshPermanentFormation();
                    dirty=false;
                    Debug.Log("[NECRO AWU-12] Formation committed and spawned");
                }
                catch(Exception ex){Debug.LogError("[NECRO AWU-12] Commit failed: "+ex);return;}
            }
            popup.SetActive(false);
        }
        private void OnDestroy(){if(canvas!=null)Destroy(canvas.gameObject);}
    }
}
