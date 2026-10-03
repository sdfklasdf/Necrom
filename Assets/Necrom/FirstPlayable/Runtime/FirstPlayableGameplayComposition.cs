using System;
using Necrom.Core.Application;
using Necrom.Core.Domain;
using TMPro;
using EntityId = Necrom.Core.Domain.EntityId;
using UnityEngine;
using UnityEngine.UI;

namespace Necrom.FirstPlayable.Runtime
{
    // Canonical Q2 composition. Serialized font and simple combat markers are review assets,
    // not final typography/art or economy balancing.
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Canvas), typeof(CanvasScaler), typeof(EncounterBoundaryController))]
    public sealed class FirstPlayableGameplayComposition : MonoBehaviour
    {
        public TMP_FontAsset ReviewFont;
        public bool IsInitialized { get; private set; }
        public FirstPlayableCombatHudRuntimeBinding HudBinding { get; private set; }
        public FirstPlayableAlliedRosterController Roster { get; private set; }
        public FirstPlayableBattleRuntimeController Battle { get; private set; }
        public EnemySpawnController Enemies { get; private set; }
        public FirstPlayableCombatHudSession Session { get; private set; }
        Formation _formation;
        FirstPlayableApplicationService _application;
        NecromancerAnchorController _anchor;
        FirstPlayableSoulResourceBridge _soul;
        SoulResourceAccount _account;
        FirstPlayableRaiseActionController _raise;
        FirstPlayableAutoCombatLoop _playerLoop;
        FirstPlayableAlliedAutoCombatLoop _alliedLoop;
        EncounterLayoutConfig _layout;
        EncounterBoundaryController _boundary;
        Button _raiseButton, _nextButton;
        TextMeshProUGUI _enemyText, _armyText;
        Vector2 _observedSize;
        Rect _observedSafe;
        Vector2 _configuredSize;
        Rect _configuredSafe;
        bool _automatic=true;
        int _sequence, _encounter;
        string Id(string prefix) => prefix + ":" + (++_sequence);
        static T GetOrAdd<T>(GameObject host) where T:Component
            => host.GetComponent<T>() ?? host.AddComponent<T>();
        public void ConfigureViewport(Vector2 size, Rect safe)
        {
            _boundary=GetOrAdd<EncounterBoundaryController>(gameObject);
            // Width-based scaling preserves verified 12/20/14 type at every pixel density.
            var scale=size.x/390f;
            if(safe.height/scale<568f) scale=safe.height/776f;
            var logicalSafeHeight=safe.height/scale;
            var logicalWidth=size.x/scale;
            var left=(logicalWidth-342f)/2f/logicalWidth;
            var hud=Mathf.Min(0.82f,464f/logicalSafeHeight);
            _layout=new EncounterLayoutConfig(size,safe,hud,
                new Rect(.12f,.1f,.30f,.25f),new Rect(.58f,.50f,.30f,.28f),
                new Rect(.12f,.37f,.44f,.18f));
            float h=logicalSafeHeight;
            var zones=new EncounterReadabilityConfig(
                new Rect(left,336f/h,342f/logicalWidth,112f/h),
                new Rect(left,144f/h,342f/logicalWidth,176f/h),
                new Rect(left,16f/h,342f/logicalWidth,112f/h),
                new Rect(.02f,hud,.96f,Mathf.Max(.01f,1f-hud-.10f)));
            _boundary.StartBoundaryWithReadability(_layout,zones);
            _configuredSize=size; _configuredSafe=safe;
            var scaler=GetComponent<CanvasScaler>();
            scaler.uiScaleMode=CanvasScaler.ScaleMode.ConstantPixelSize;
            scaler.referenceResolution=new Vector2(390,844);
            scaler.screenMatchMode=CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight=0;
            scaler.scaleFactor=GetComponent<CanvasScaler>().scaleFactor;
            scaler.scaleFactor=scale;
            GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;
            GetComponent<Canvas>().sortingOrder=0;
            if(IsInitialized)
            {
                Battle.Initialize(_application,_boundary,_layout,_anchor,Enemies);
                BindHudIfNeeded();
                if(HudBinding.IsInitialized)
                {
                    HudBinding.OverlayHost.GetComponent<CanvasScaler>().scaleFactor=scale;
                    HudBinding.RefreshNow();
                }
            }
        }
        void Awake()
        {
            if(ReviewFont==null) throw new InvalidOperationException("Canonical review font is unresolved.");
            _observedSize=new Vector2(Screen.width,Screen.height);
            _observedSafe=Screen.safeArea;
            ConfigureViewport(_observedSize,_observedSafe);
            var safe=transform.Find("SafeArea") as RectTransform;
            var combat=safe.Find("CombatViewport") as RectTransform;
            _formation=new Formation();
            var battleState=new BattleStateMachine();
            var encounter=new FirstPlayableEncounter(new RaiseService(),battleState,_formation);
            var application=new FirstPlayableApplicationService(encounter);
            _application=application;
            var progression=new FirstPlayableProgression(application,encounter);
            var anchor=GetOrAdd<NecromancerAnchorController>(combat.gameObject);
            _anchor=anchor;
            var player=anchor.BindNecromancer(
                new Combatant(new EntityId("player:canonical"),"necromancer.prototype",Faction.Player,100),
                combat.Find("NecromancerSpawnZone") as RectTransform);
            player.AttachAutoBehavior(new NecromancerBasicAutoBehavior(player.Model,new BasicAutoBehaviorSpec(2,500)));
            Enemies=GetOrAdd<EnemySpawnController>(combat.gameObject);
            Roster=GetOrAdd<FirstPlayableAlliedRosterController>(combat.gameObject);
            Roster.Initialize(_formation,combat.Find("AlliedSpawnZone") as RectTransform);
            Battle=GetOrAdd<FirstPlayableBattleRuntimeController>(combat.gameObject);
            Battle.Initialize(application,_boundary,_layout,anchor,Enemies);
            Battle.ConfigureAlliedRestartInvariant(Roster);
            var input=GetOrAdd<FirstPlayableRaiseCommandInputHook>(gameObject);
            input.Initialize(progression);
            Session=new FirstPlayableCombatHudSession();
            _account=new SoulResourceAccount(10); // review policy only, no final balance decision
            _soul=new FirstPlayableSoulResourceBridge(_account,_=>4,_=>3,
                source=>"soul:"+source.SourceId.Value);
            _raise=GetOrAdd<FirstPlayableRaiseActionController>(gameObject);
            _raise.Initialize(Enemies,input,source=>new RaiseIntoFormationCommand(
                Id("raise"),source,source.Revision,new EntityId(Id("ally")),7,EmptySlot(),_formation.Revision),
                ()=>Id("raised"),()=>Id("assigned"));
            _raise.ConfigureAlliedActivation(Roster,_=>new BasicAutoBehaviorSpec(4,500));
            _raise.ConfigureHudSession(Session);
            var targeting=GetOrAdd<FirstPlayableTargetingController>(combat.gameObject);
            targeting.Initialize(player,Enemies);
            var pipeline=new FirstPlayableDamageDeathPipeline(()=>Id("damage"),()=>Id("defeat"));
            _playerLoop=GetOrAdd<FirstPlayableAutoCombatLoop>(combat.gameObject);
            _playerLoop.Initialize(Battle,player,targeting,pipeline,id=>new EntityId("source:"+id.Value),()=>Id("resolve"));
            _alliedLoop=GetOrAdd<FirstPlayableAlliedAutoCombatLoop>(combat.gameObject);
            _alliedLoop.Initialize(application,Roster,targeting,pipeline,id=>new EntityId("source:"+id.Value),()=>Id("resolve:ally"));
            _alliedLoop.ConfigureHudSession(Session);
            HudBinding=GetOrAdd<FirstPlayableCombatHudRuntimeBinding>(gameObject);
            SpawnTarget();
            IsInitialized=true;
            BindHudIfNeeded();
            AddReviewCombatPresentation(combat);
            Battle.StartBattle(new StartBattleCommand(Id("start"),Battle.Revision));
        }
        int EmptySlot()
        {
            for(int i=0;i<Formation.Capacity;i++) if(!_formation.GetSlot(i).HasValue) return i;
            throw new InvalidOperationException("Formation is full.");
        }
        void SpawnTarget()
        {
            var combat=transform.Find("SafeArea/CombatViewport");
            Enemies.SpawnEnemy("enemy:canonical:"+ ++_encounter,
                new EnemyArchetypeDefinition("enemy.guard","frontline.guard",10),
                combat.Find("EnemySpawnZone") as RectTransform);
        }
        void BindHudIfNeeded()
        {
            if(!IsInitialized || !HudBinding.enabled || HudBinding.IsInitialized) return;
            HudBinding.Initialize(Battle,Enemies,_soul,_formation,Roster,Session,
                transform.Find("SafeArea") as RectTransform,
                FirstPlayableCombatHudVerifiedDesignContract.Create(),
                new FirstPlayableCombatHudCopyProviderAdapter(ReviewCopy),
                new FirstPlayableCombatHudFontProviderAdapter(()=>ReviewFont));
            // Normal input goes through the resource bridge and domain command, never a state adapter.
            var scaler=HudBinding.OverlayHost.GetComponent<CanvasScaler>();
            scaler.uiScaleMode=CanvasScaler.ScaleMode.ConstantPixelSize;
            scaler.referenceResolution=new Vector2(390,844);
            scaler.matchWidthOrHeight=0;
            scaler.scaleFactor=GetComponent<CanvasScaler>().scaleFactor;
            HudBinding.OverlayHost.GetComponent<Canvas>().sortingOrder=10;
            _raiseButton=HudBinding.OverlayHost.transform.Find("SafeAreaMirror/RaiseRenderContainer/PrimaryCta").GetComponent<Button>();
            _raiseButton.onClick.AddListener(RaiseCurrentTarget);
        }
        FirstPlayableCombatHudCopy ReviewCopy(FirstPlayableCombatHudContentKey key)
        {
            string title=key.ToString().StartsWith("Target")?"TARGET STATUS":
                key.ToString().StartsWith("Raise")?"RAISE STATUS":"ARMY STATUS";
            string detail;
            switch(key)
            {
                case FirstPlayableCombatHudContentKey.TargetNone: detail="Identity / no target";break;
                case FirstPlayableCombatHudContentKey.TargetActive: detail="Guard / HP "+Enemies.CurrentTarget.Model.Health+" / active";break;
                case FirstPlayableCombatHudContentKey.TargetDefeated: detail="Guard / HP 0 / defeated";break;
                case FirstPlayableCombatHudContentKey.RaiseNoTarget: detail="Reason / no target";break;
                case FirstPlayableCombatHudContentKey.RaiseTargetNotReady: detail="Reason / target not ready";break;
                case FirstPlayableCombatHudContentKey.RaiseSourceUnavailableOrConsumed: detail="Reason / source unavailable";break;
                case FirstPlayableCombatHudContentKey.RaiseInsufficientSoul: detail="Soul quote / insufficient Soul";break;
                case FirstPlayableCombatHudContentKey.RaiseEligible: detail="Soul quote / eligible";break;
                case FirstPlayableCombatHudContentKey.RaiseCommittedAwaitingProof: detail="Committed UnitId / awaiting proof";break;
                case FirstPlayableCombatHudContentKey.RaiseProofObserved: detail="Exact raised UnitId contribution / observed";break;
                case FirstPlayableCombatHudContentKey.ArmyEmpty: detail="Formation 5-slot ownership / empty";break;
                case FirstPlayableCombatHudContentKey.ArmyOwned: detail="Formation ownership / runtime auxiliary";break;
                case FirstPlayableCombatHudContentKey.ArmyProofPending: detail="Formation / raised UnitId / proof pending";break;
                default: detail="Formation / exact contribution / observed";break;
            }
            return new FirstPlayableCombatHudCopy(key.ToString(),title,detail,"RAISE");
        }
        public void SetAutomaticCombat(bool enabled)
        {
            _automatic=enabled;
            if(_playerLoop!=null) _playerLoop.enabled=enabled&&isActiveAndEnabled;
            if(_alliedLoop!=null) _alliedLoop.enabled=enabled&&isActiveAndEnabled;
        }
        public void AdvanceCombat(float seconds)
        {
            // Same production loops as Update. Public manual advance supports replay/debugging.
            _alliedLoop.Advance(seconds);
            _playerLoop.Advance(seconds);
            FinalizeResultIfNeeded();
            HudBinding.RefreshNow();
        }
        void FinalizeResultIfNeeded()
        {
            if(Battle.Phase==BattlePhase.Victory || Battle.Phase==BattlePhase.Defeat)
                Battle.FinalizeBattle(new FinalizeBattleCommand(Id("finalize"),Battle.Revision));
        }
        public void RaiseCurrentTarget()
        {
            _soul.ExecuteRaise(Enemies,_raise,_account.Revision);
            HudBinding.RefreshNow();
        }
        public void StartNextEncounter()
        {
            var old=Enemies.CurrentTarget;
            Battle.RestartBattle(new RestartBattleCommand(Id("restart"),Battle.Revision));
            if(old!=null) Destroy(old.gameObject);
            SpawnTarget();
            Battle.StartBattle(new StartBattleCommand(Id("start"),Battle.Revision));
            // Start/RestartBoundary preserves ownership; restore current responsive readability layout.
            ConfigureViewport(_configuredSize,_configuredSafe);
            HudBinding.RefreshNow();
        }
        void Update()
        {
            if(!IsInitialized) return;
            var size=new Vector2(Screen.width,Screen.height);
            if(size!=_observedSize || Screen.safeArea!=_observedSafe)
            {
                _observedSize=size; _observedSafe=Screen.safeArea;
                ConfigureViewport(size,_observedSafe);
            }
            BindHudIfNeeded();
            FinalizeResultIfNeeded();
            if(_nextButton!=null) _nextButton.interactable=Battle.Phase==BattlePhase.Resolved;
            if(_enemyText!=null) _enemyText.text="GUARD\nHP "+Enemies.CurrentTarget.Model.Health;
            if(_armyText!=null) _armyText.text="RAISED ARMY\n"+Roster.ActiveCount+" / 5";
        }
        void OnEnable()
        {
            if(IsInitialized) { BindHudIfNeeded(); SetAutomaticCombat(_automatic); }
        }
        void OnDisable()
        {
            HudBinding?.Shutdown();
            if(_playerLoop!=null) _playerLoop.enabled=false;
            if(_alliedLoop!=null) _alliedLoop.enabled=false;
        }
        void AddReviewCombatPresentation(RectTransform combat)
        {
            var background=GetOrAdd<Image>(combat.gameObject);
            background.color=new Color(.08f,.10f,.14f); background.raycastTarget=false;
            var playerZone=combat.Find("NecromancerSpawnZone") as RectTransform;
            Label(playerZone,"NECROMANCER",new Color(.0314f,.498f,.357f));
            _enemyText=Label(combat.Find("EnemySpawnZone") as RectTransform,"GUARD",new Color(.788f,.165f,.165f));
            _armyText=Label(combat.Find("AlliedSpawnZone") as RectTransform,"RAISED ARMY",new Color(.094f,.392f,.67f));
            var command=new GameObject("NextEncounter",typeof(RectTransform),typeof(Image),typeof(Button));
            command.transform.SetParent(combat,false);
            var r=command.GetComponent<RectTransform>(); r.anchorMin=new Vector2(.45f,.91f);
            r.anchorMax=new Vector2(.98f,.99f);r.offsetMin=r.offsetMax=Vector2.zero;
            command.GetComponent<Image>().color=new Color(.0314f,.498f,.357f);
            _nextButton=command.GetComponent<Button>();_nextButton.onClick.AddListener(StartNextEncounter);
            Label(r,"NEXT ENCOUNTER",Color.clear);
        }
        TextMeshProUGUI Label(RectTransform zone,string copy,Color surface)
        {
            if(surface.a>0){var image=GetOrAdd<Image>(zone.gameObject);image.color=surface;image.raycastTarget=false;}
            var obj=new GameObject("ReviewLabel",typeof(RectTransform),typeof(TextMeshProUGUI));
            obj.transform.SetParent(zone,false);
            var text=obj.GetComponent<TextMeshProUGUI>(); text.font=ReviewFont;
            text.text=copy;text.fontSize=14;text.color=Color.white;text.alignment=TextAlignmentOptions.Center;
            text.raycastTarget=false;var r=text.rectTransform;
            r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;
            return text;
        }
    }
}
