using System;
using Necrom.Core.Application;
using Necrom.Core.Domain;
using TMPro;
using EntityId = Necrom.Core.Domain.EntityId;
using UnityEngine;
using UnityEngine.UI;

namespace Necrom.FirstPlayable.Runtime
{
    // Canonical Q3 composition. Production-candidate art and functional UI typography are serialized;
    // economy values remain review policy and are not final balance decisions.
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Canvas), typeof(CanvasScaler), typeof(EncounterBoundaryController))]
    public sealed class FirstPlayableGameplayComposition : MonoBehaviour
    {
        public TMP_FontAsset ReviewFont;
        public TMP_FontAsset ReviewFontMedium;
        public TMP_FontAsset ReviewFontBold;
        public int SoulBalance => _account?.Balance ?? 0;
        public bool IsInitialized { get; private set; }
        public FirstPlayableCombatHudRuntimeBinding HudBinding { get; private set; }
        public FirstPlayableAlliedRosterController Roster { get; private set; }
        public FirstPlayableBattleRuntimeController Battle { get; private set; }
        public EnemySpawnController Enemies { get; private set; }
        public FirstPlayableDefenseWaveRuntimeController DefenseWave { get; private set; }
        public FirstPlayableGatePressureController GatePressure { get; private set; }
        public FirstPlayableDefenseWaveHudRuntimeBinding DefenseHudBinding { get; private set; }
        public FirstPlayableCommercialHudRuntimePolish CommercialHudPolish { get; private set; }
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
        Sprite _nextSprite;
        TextMeshProUGUI _enemyText, _armyText;
        Vector2 _observedSize;
        Rect _observedSafe;
        Vector2 _configuredSize;
        Rect _configuredSafe;
        bool _automatic=true;
        int _sequence, _encounter;
        const int CanonicalThreatsPerWave = 2;
        const int CanonicalGateIntegrity = 10;
        // Representative vertical-slice pressure inputs, not final balance.
        const float CanonicalGateTravelSeconds = 4.8f;
        const int CanonicalGateBreachDamage = 10;
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
                    DefenseHudBinding?.RefreshNow();
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
            pipeline.Defeated += result => _soul.ApplyDefeatGrant(result,_account.Revision);
            _playerLoop=GetOrAdd<FirstPlayableAutoCombatLoop>(combat.gameObject);
            _playerLoop.Initialize(Battle,player,targeting,pipeline,id=>new EntityId("source:"+id.Value),()=>Id("resolve"));
            _alliedLoop=GetOrAdd<FirstPlayableAlliedAutoCombatLoop>(combat.gameObject);
            _alliedLoop.Initialize(application,Roster,targeting,pipeline,id=>new EntityId("source:"+id.Value),()=>Id("resolve:ally"));
            _alliedLoop.ConfigureHudSession(Session);
            DefenseWave=GetOrAdd<FirstPlayableDefenseWaveRuntimeController>(gameObject);
            DefenseWave.Initialize(application,Enemies,CanonicalGateIntegrity,()=>Id("resolve:wave"));
            GatePressure=GetOrAdd<FirstPlayableGatePressureController>(gameObject);
            GatePressure.Initialize(
                DefenseWave,
                Enemies,
                CanonicalGateTravelSeconds,
                CanonicalGateBreachDamage);
            _playerLoop.ConfigureDefenseWave(DefenseWave);
            _alliedLoop.ConfigureDefenseWave(DefenseWave);
            HudBinding=GetOrAdd<FirstPlayableCombatHudRuntimeBinding>(gameObject);
            DefenseHudBinding=GetOrAdd<FirstPlayableDefenseWaveHudRuntimeBinding>(gameObject);
            StartCanonicalWave(1);
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
        EnemyRuntimeEntity SpawnTarget()
        {
            var combat=transform.Find("SafeArea/CombatViewport");
            var ordinal=++_encounter;
            var archetype=(ordinal%3) switch
            {
                1 => "forest.chr001",
                2 => "forest.chr003",
                _ => "forest.chr006"
            };
            return Enemies.SpawnEnemy("enemy:canonical:"+ordinal,
                new EnemyArchetypeDefinition(archetype,"frontline.guard",10),
                combat.Find("EnemySpawnZone") as RectTransform);
        }
        void StartCanonicalWave(int waveNumber)
        {
            GatePressure.ResetForWave();
            DefenseWave.StartWave(waveNumber,CanonicalThreatsPerWave);
            for(var i=0;i<CanonicalThreatsPerWave;i++)
            {
                var threat=SpawnTarget();
                DefenseWave.RegisterSpawnedThreat(threat);
                GatePressure.RegisterThreat(threat);
            }
        }
        void BindHudIfNeeded()
        {
            if(!IsInitialized || !HudBinding.enabled || HudBinding.IsInitialized) return;
            var productionPresentation = GetComponent<FirstPlayableVisualPresentation>() != null;
            if (productionPresentation && (ReviewFontMedium == null || ReviewFontBold == null))
                throw new InvalidOperationException("Q3 production font set requires Regular / Medium / Bold.");

            HudBinding.Initialize(Battle,Enemies,_soul,_formation,Roster,Session,
                transform.Find("SafeArea") as RectTransform,
                FirstPlayableCombatHudVerifiedDesignContract.Create(),
                new FirstPlayableCombatHudCopyProviderAdapter(productionPresentation?ProductionCopy:ReviewCopy),
                FirstPlayableCombatHudFontProviderAdapter.WithWeights(
                    ()=>ReviewFont,
                    ()=>productionPresentation?ReviewFontMedium:ReviewFont,
                    ()=>productionPresentation?ReviewFontBold:ReviewFont));
            HudBinding.ConfigureDefenseWave(DefenseWave);
            // Normal input goes through the resource bridge and domain command, never a state adapter.
            var scaler=HudBinding.OverlayHost.GetComponent<CanvasScaler>();
            scaler.uiScaleMode=CanvasScaler.ScaleMode.ConstantPixelSize;
            scaler.referenceResolution=new Vector2(390,844);
            scaler.matchWidthOrHeight=0;
            scaler.scaleFactor=GetComponent<CanvasScaler>().scaleFactor;
            HudBinding.OverlayHost.GetComponent<Canvas>().sortingOrder=10;
            _raiseButton=HudBinding.OverlayHost.transform.Find("SafeAreaMirror/RaiseRenderContainer/PrimaryCta").GetComponent<Button>();
            _raiseButton.onClick.AddListener(RaiseCurrentTarget);
            if(!DefenseHudBinding.IsInitialized)
                DefenseHudBinding.Initialize(DefenseWave,HudBinding,
                    ReviewFont,productionPresentation?ReviewFontMedium:ReviewFont,
                    productionPresentation?ReviewFontBold:ReviewFont);
            DefenseHudBinding.RefreshNow();
            CommercialHudPolish=GetOrAdd<FirstPlayableCommercialHudRuntimePolish>(gameObject);
            if(!CommercialHudPolish.IsInitialized)
                CommercialHudPolish.Initialize(HudBinding,DefenseWave);
            CommercialHudPolish.RefreshNow();
        }
        FirstPlayableCombatHudCopy ReviewCopy(FirstPlayableCombatHudContentKey key)
        {
            string title=key.ToString().StartsWith("Target")?"TARGET STATUS":
                key.ToString().StartsWith("Raise")?"RAISE STATUS":"ARMY STATUS";
            string detail;
            switch(key)
            {
                case FirstPlayableCombatHudContentKey.TargetNone: detail="Identity / no target";break;
                case FirstPlayableCombatHudContentKey.TargetActive: detail="Guard / HP "+ActiveTargetHealth()+" / active";break;
                case FirstPlayableCombatHudContentKey.TargetDefeated: detail="Guard / HP 0 / defeated";break;
                case FirstPlayableCombatHudContentKey.RaiseNoTarget: detail="Reason / no target";break;
                case FirstPlayableCombatHudContentKey.RaiseTargetNotReady: detail="Reason / target not ready";break;
                case FirstPlayableCombatHudContentKey.RaiseSourceUnavailableOrConsumed: detail="Reason / source unavailable";break;
                case FirstPlayableCombatHudContentKey.RaiseInsufficientSoul: detail="Soul quote / insufficient Soul";break;
                case FirstPlayableCombatHudContentKey.RaiseEligible: detail="Soul "+SoulBalance+" / cost 3 / eligible";break;
                case FirstPlayableCombatHudContentKey.RaiseCommittedAwaitingProof: detail="Committed UnitId / awaiting proof";break;
                case FirstPlayableCombatHudContentKey.RaiseProofObserved: detail="Exact raised UnitId contribution / observed";break;
                case FirstPlayableCombatHudContentKey.ArmyEmpty: detail="Formation 5-slot ownership / empty";break;
                case FirstPlayableCombatHudContentKey.ArmyOwned: detail="Formation ownership / runtime auxiliary";break;
                case FirstPlayableCombatHudContentKey.ArmyProofPending: detail="Formation / raised UnitId / proof pending";break;
                default: detail="Formation / exact contribution / observed";break;
            }
            var full=key==FirstPlayableCombatHudContentKey.RaiseEligible && Roster.ActiveCount>=Formation.Capacity;
            if(full) detail="Formation full / continue combat";
            return new FirstPlayableCombatHudCopy(key.ToString(),title,detail,full?"ARMY FULL":"RAISE");
        }
        FirstPlayableCombatHudCopy ProductionCopy(FirstPlayableCombatHudContentKey key)
        {
            var family=key.ToString().StartsWith("Target")?"전투":key.ToString().StartsWith("Raise")?"영혼 친구":"친구";
            var failed=DefenseWave!=null&&DefenseWave.Phase==DefenseWavePhase.Failed;
            if(failed&&key==FirstPlayableCombatHudContentKey.TargetNone)
                return new FirstPlayableCombatHudCopy(family,"방어 실패","정원이 무너졌어요 · 다시 준비해요","");
            if(failed&&key==FirstPlayableCombatHudContentKey.RaiseNoTarget)
                return new FirstPlayableCombatHudCopy(family,"지금은 쉬어가기","다음 전투를 준비해요","");
            var activeCharacter=ActiveTargetDisplayName();
            var raiseCharacter=RaiseCandidateDisplayName();
            var title=family;var detail="";
            switch(key)
            {
                case FirstPlayableCombatHudContentKey.TargetNone: title="새 친구를 기다려요";detail="다음 전투가 곧 시작돼요";break;
                case FirstPlayableCombatHudContentKey.TargetActive: title=activeCharacter;detail="체력 "+ActiveTargetHealth()+" / 10 · 자동 전투 중";break;
                case FirstPlayableCombatHudContentKey.TargetDefeated: title=raiseCharacter+" 격파";detail="영혼 친구로 만들 준비 완료";break;
                case FirstPlayableCombatHudContentKey.RaiseNoTarget: title="친구로 만들 대상이 없어요";detail="격파한 적은 영혼 친구가 될 수 있어요";break;
                case FirstPlayableCombatHudContentKey.RaiseTargetNotReady: title=activeCharacter+"를 격파하면 친구가 돼요";detail="영혼을 모아 동료를 늘려요";break;
                case FirstPlayableCombatHudContentKey.RaiseSourceUnavailableOrConsumed: title="이미 친구가 된 대상이에요";detail="다음 전투에서 새 친구를 만나보세요";break;
                case FirstPlayableCombatHudContentKey.RaiseInsufficientSoul: title="영혼이 조금 부족해요";detail="영혼 "+SoulBalance+" · 친구 만들기 3";break;
                case FirstPlayableCombatHudContentKey.RaiseEligible: title=raiseCharacter+"를 친구로!";detail="영혼 "+SoulBalance+" · 친구 만들기 3";break;
                case FirstPlayableCombatHudContentKey.RaiseCommittedAwaitingProof: title="새 친구가 합류했어요";detail="친구의 첫 공격을 기다려요";break;
                case FirstPlayableCombatHudContentKey.RaiseProofObserved: title="친구의 첫 공격 성공!";detail="영혼 친구가 실제 피해를 입혔어요";break;
                case FirstPlayableCombatHudContentKey.ArmyEmpty: title="친구 0 / 5";detail="격파한 적과 친구가 되어 팀을 채워요";break;
                case FirstPlayableCombatHudContentKey.ArmyOwned: title="친구 "+Roster.ActiveCount+" / 5";detail="영혼 친구가 함께 싸워요";break;
                case FirstPlayableCombatHudContentKey.ArmyProofPending: title="친구 "+Roster.ActiveCount+" / 5";detail="새 친구의 첫 공격을 기다려요";break;
                case FirstPlayableCombatHudContentKey.ArmyProofObserved: title="친구 "+Roster.ActiveCount+" / 5";detail="영혼 친구의 공격을 확인했어요";break;
            }
            var full=key==FirstPlayableCombatHudContentKey.RaiseEligible&&Roster.ActiveCount>=Formation.Capacity;
            if(full){title="친구 자리가 가득 찼어요";detail="5 / 5 · 다음 전투에서 함께 싸워요";}
            return new FirstPlayableCombatHudCopy(family,title,detail,full?"친구 가득":"친구 만들기");
        }
        int ActiveTargetHealth()
        {
            return Enemies.TryGetFirstActiveTarget(out var target) && target.Model!=null
                ? target.Model.Health
                : Enemies.CurrentTarget?.Model?.Health ?? 0;
        }
        string ActiveTargetDisplayName()
        {
            if(Enemies.TryGetFirstActiveTarget(out var target) && target?.Model!=null)
                return CharacterDisplayName(target.Model.ArchetypeId);
            return CharacterDisplayName(Enemies.CurrentTarget?.Model?.ArchetypeId);
        }
        string RaiseCandidateDisplayName()
            => CharacterDisplayName(Enemies.CurrentTarget?.Model?.ArchetypeId);
        static string CharacterDisplayName(string archetypeId)
        {
            return archetypeId switch
            {
                "forest.chr001" => "도토리 방패병",
                "forest.chr002" => "솔방울 검객",
                "forest.chr003" => "민들레 궁수",
                "forest.chr004" => "클로버 우편부",
                "forest.chr005" => "밤톨 마법사",
                "forest.chr006" => "이슬 치료사",
                "forest.chr007" => "딸기 폭탄꾼",
                "forest.chr008" => "나무껍질 수호자",
                _ => "숲 친구"
            };
        }
        public void SetAutomaticCombat(bool enabled)
        {
            _automatic=enabled;
            if(_playerLoop!=null) _playerLoop.enabled=enabled&&isActiveAndEnabled;
            if(_alliedLoop!=null) _alliedLoop.enabled=enabled&&isActiveAndEnabled;
            if(GatePressure!=null) GatePressure.enabled=enabled&&isActiveAndEnabled;
        }
        public void AdvanceCombat(float seconds)
        {
            // Same production systems as Update. Public manual advance supports replay/debugging.
            _alliedLoop.Advance(seconds);
            _playerLoop.Advance(seconds);
            GatePressure.Advance(seconds);
            FinalizeResultIfNeeded();
            HudBinding.RefreshNow();
            DefenseHudBinding.RefreshNow();
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
            DefenseHudBinding.RefreshNow();
        }
        public void StartNextEncounter()
        {
            if(DefenseWave.Phase!=DefenseWavePhase.Cleared)
                throw new InvalidOperationException("A canonical next wave requires a cleared defense wave.");
            var nextWave=DefenseWave.WaveNumber+1;
            Battle.RestartBattle(new RestartBattleCommand(Id("restart"),Battle.Revision));
            Enemies.ClearEncounterTargets();
            DefenseWave.PrepareNextWave();
            StartCanonicalWave(nextWave);
            Battle.StartBattle(new StartBattleCommand(Id("start"),Battle.Revision));
            // Start/RestartBoundary preserves ownership; restore current responsive readability layout.
            ConfigureViewport(_configuredSize,_configuredSafe);
            HudBinding.RefreshNow();
            DefenseHudBinding.RefreshNow();
        }
        public void ResolveFirstThreatAtGate(int integrityDamage)
        {
            if(!Enemies.TryGetFirstActiveTarget(out var target))
                throw new InvalidOperationException("No active canonical threat can reach the gate.");
            DefenseWave.RecordGateBreach(target,integrityDamage);
            FinalizeResultIfNeeded();
            HudBinding.RefreshNow();
            DefenseHudBinding.RefreshNow();
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
            if(_nextButton!=null) {
                _nextButton.interactable=Battle.Phase==BattlePhase.Resolved && DefenseWave.Phase==DefenseWavePhase.Cleared;
                var combat=_nextButton.transform.parent as RectTransform;
                var rect=_nextButton.transform as RectTransform;
                rect.anchorMin=new Vector2(.68f,.02f);
                rect.anchorMax=new Vector2(.94f,.16f);
                rect.offsetMin=rect.offsetMax=Vector2.zero;
                var nextImage=_nextButton.GetComponent<Image>();
                nextImage.raycastTarget=true;
                nextImage.color=_nextButton.interactable
                    ? new Color(79f/255f,212f/255f,174f/255f,1f)
                    : new Color(1f,1f,1f,0f);
                var label=_nextButton.GetComponentInChildren<TextMeshProUGUI>();
                if(label!=null) label.color=_nextButton.interactable
                    ? new Color(56f/255f,51f/255f,79f/255f,1f)
                    : new Color(56f/255f,51f/255f,79f/255f,0f);
            }
            if(_enemyText!=null) _enemyText.text="GUARD\nHP "+ActiveTargetHealth();
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
            if(GatePressure!=null) GatePressure.enabled=false;
        }
        void OnDestroy()
        { if(_nextSprite!=null){Destroy(_nextSprite.texture);Destroy(_nextSprite);} }
        void AddReviewCombatPresentation(RectTransform combat)
        {
            var background=GetOrAdd<Image>(combat.gameObject);
            background.color=new Color(.08f,.10f,.14f); background.raycastTarget=false;
            var visual=GetComponent<FirstPlayableVisualPresentation>();
            if(visual!=null) visual.Initialize(this,combat,_playerLoop,_alliedLoop);
            else {
            var playerZone=combat.Find("NecromancerSpawnZone") as RectTransform;
            Label(playerZone,"NECROMANCER",new Color(.0314f,.498f,.357f));
            _enemyText=Label(combat.Find("EnemySpawnZone") as RectTransform,"GUARD",new Color(.788f,.165f,.165f));
            _armyText=Label(combat.Find("AlliedSpawnZone") as RectTransform,"RAISED ARMY",new Color(.094f,.392f,.67f));
            }
            var command=new GameObject("NextEncounter",typeof(RectTransform),typeof(Image),typeof(Button));
            command.transform.SetParent(combat,false);
            var r=command.GetComponent<RectTransform>(); r.anchorMin=new Vector2(.47f,.97f-44f/Mathf.Max(44f,combat.rect.height));
            r.anchorMax=new Vector2(.94f,.97f);r.offsetMin=r.offsetMax=Vector2.zero;
            command.GetComponent<Image>().color=new Color(.0314f,.498f,.357f);
            _nextSprite=FirstPlayableCombatHudUnityView.RoundedSprite(12f);
            command.GetComponent<Image>().sprite=_nextSprite;command.GetComponent<Image>().type=Image.Type.Sliced;
            _nextButton=command.GetComponent<Button>();_nextButton.transition=Selectable.Transition.None;_nextButton.onClick.AddListener(StartNextEncounter);
            var nextLabel=Label(r,visual!=null?"다음 전투":"NEXT ENCOUNTER",Color.clear);
            if(visual!=null && ReviewFontMedium!=null) nextLabel.font=ReviewFontMedium;
            if(visual!=null)
            {
                nextLabel.fontSize=12f;
                nextLabel.alignment=TextAlignmentOptions.Center;
                nextLabel.color=new Color(56f/255f,51f/255f,79f/255f,0f);
                command.GetComponent<Image>().color=new Color(1f,1f,1f,0f);
                command.GetComponent<Image>().raycastTarget=true;
            }
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