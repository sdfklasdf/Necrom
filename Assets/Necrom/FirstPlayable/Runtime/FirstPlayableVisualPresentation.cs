using System;
using Necrom.Core.Domain;
using EntityId = Necrom.Core.Domain.EntityId;
using UnityEngine;
using UnityEngine.UI;

namespace Necrom.FirstPlayable.Runtime
{
    // Q3 review art + procedural motion. No gameplay state adapter or fabricated contribution.
    [DisallowMultipleComponent]
    public sealed class FirstPlayableVisualPresentation : MonoBehaviour
    {
        public Texture2D NecromancerArt, GuardArt, RaisedGuardArt, BackgroundArt;
        public int LoadedArtCount => (NecromancerArt?1:0)+(GuardArt?1:0)+(RaisedGuardArt?1:0)+(BackgroundArt?1:0);
        public int PlayerAttackCueCount { get; private set; }
        public int HitCueCount { get; private set; }
        public int DefeatCueCount { get; private set; }
        public int RaiseCueCount { get; private set; }
        public int AlliedContributionCueCount { get; private set; }
        public int VisibleAllyCount { get; private set; }
        public string LastContributionUnitId { get; private set; }
        FirstPlayableGameplayComposition _game;
        FirstPlayableAutoCombatLoop _playerLoop;
        FirstPlayableAlliedAutoCombatLoop _alliedLoop;
        RawImage _player, _enemy, _background;
        readonly RawImage[] _allies=new RawImage[Formation.Capacity];
        readonly float[] _allyPulse=new float[Formation.Capacity];
        RectTransform _root;
        string _enemyId;
        float _playerPulse,_hitPulse,_defeatPulse;
        bool _defeated;
        bool _initialized;
        public void Initialize(FirstPlayableGameplayComposition game,RectTransform combat,
            FirstPlayableAutoCombatLoop playerLoop,FirstPlayableAlliedAutoCombatLoop alliedLoop)
        {
            if(_initialized)throw new InvalidOperationException("Visual presentation already initialized.");
            if(LoadedArtCount!=4)throw new InvalidOperationException("Q3 art references unresolved.");
            _game=game;_playerLoop=playerLoop;_alliedLoop=alliedLoop;
            var host=new GameObject("Q3VisualPresentation",typeof(RectTransform));host.transform.SetParent(combat,false);
            _root=(RectTransform)host.transform;_root.anchorMin=Vector2.zero;_root.anchorMax=Vector2.one;_root.offsetMin=_root.offsetMax=Vector2.zero;
            _background=Art("Background",BackgroundArt);_background.rectTransform.anchorMin=Vector2.zero;
            _background.rectTransform.anchorMax=Vector2.one;_background.rectTransform.offsetMin=_background.rectTransform.offsetMax=Vector2.zero;
            _player=Art("Necromancer",NecromancerArt);_enemy=Art("Guard",GuardArt);
            for(int i=0;i<Formation.Capacity;i++){_allies[i]=Art("RaisedGuardSlot"+i,RaisedGuardArt);_allies[i].gameObject.SetActive(false);}
            _playerLoop.AttackApplied+=PlayerAttack;
            _alliedLoop.AttackApplied+=AlliedAttack;
            _initialized=true;
            Refresh();
        }
        RawImage Art(string name,Texture texture)
        {
            var o=new GameObject(name,typeof(RectTransform),typeof(RawImage));o.transform.SetParent(_root,false);
            var image=o.GetComponent<RawImage>();image.texture=texture;image.raycastTarget=false;
            image.rectTransform.anchorMin=image.rectTransform.anchorMax=Vector2.zero;
            image.rectTransform.pivot=Vector2.zero;return image;
        }
        void PlayerAttack(EntityId actor,DamageDeathResult result)
        {
            if(!result.Changed)return;
            PlayerAttackCueCount++;_playerPulse=.20f;
            Hit(result);
        }
        void AlliedAttack(EntityId actor,DamageDeathResult result)
        {
            if(!result.Changed)return;
            AlliedContributionCueCount++;LastContributionUnitId=actor.Value;
            for(int i=0;i<Formation.Capacity;i++)
            {
                var ally=_game.Roster.GetSlot(i);
                if(ally!=null&&ally.Model.Id.Equals(actor))_allyPulse[i]=.28f;
            }
            Hit(result);
        }
        void Hit(DamageDeathResult result)
        {
            HitCueCount++;_hitPulse=.14f;
            if(result.BecameDefeated){DefeatCueCount++;_defeated=true;_defeatPulse=.32f;}
        }
        void Update()
        {
            if(!_initialized)return;
            _playerPulse=Mathf.Max(0,_playerPulse-Time.deltaTime);
            _hitPulse=Mathf.Max(0,_hitPulse-Time.deltaTime);
            _defeatPulse=Mathf.Max(0,_defeatPulse-Time.deltaTime);
            for(int i=0;i<Formation.Capacity;i++)_allyPulse[i]=Mathf.Max(0,_allyPulse[i]-Time.deltaTime);
            Refresh();
        }
        void LateUpdate(){if(_initialized)Refresh();}
        public void Refresh()
        {
            if(!_initialized)return;
            var target=_game.Enemies.CurrentTarget;
            var id=target?.Model?.Id.Value;
            if(id!=_enemyId){_enemyId=id;_defeated=false;_defeatPulse=0;_hitPulse=0;}
            var h=_root.rect.height;var w=_root.rect.width;
            var protectedHeight=((RectTransform)_game.transform.Find("SafeArea/ProtectedCombatReadabilityZone")).rect.height;
            var scale=Mathf.Min(1f,h/312.48f,protectedHeight/218f);
            Place(_player,42f/390f*w+(_playerPulse>0?6f*scale:0),32f*scale,92f*scale,138f*scale);
            Place(_enemy,246f/390f*w,75f*scale,90f*scale,135f*scale);
            _enemy.rectTransform.localScale=_defeated?new Vector3(1,Mathf.Lerp(.35f,1,_defeatPulse/.32f),1):Vector3.one;
            _enemy.color=_hitPulse>0?new Color(1,.48f,.48f):_defeated?new Color(.65f,.75f,.8f,.65f):Color.white;
            var before=VisibleAllyCount;VisibleAllyCount=0;
            for(int i=0;i<Formation.Capacity;i++)
            {
                var exists=_game.Roster.GetSlot(i)!=null;
                _allies[i].gameObject.SetActive(exists);
                if(exists)VisibleAllyCount++;
                Place(_allies[i],(142f+34f*i)/390f*w+(_allyPulse[i]>0?4f*scale:0),15f*scale,42f*scale,63f*scale);
                _allies[i].color=_allyPulse[i]>0?new Color(.65f,1,.75f):Color.white;
                _allies[i].rectTransform.localScale=_allyPulse[i]>0?Vector3.one*1.10f:Vector3.one;
            }
            if(VisibleAllyCount>before){RaiseCueCount+=VisibleAllyCount-before;for(int i=before;i<VisibleAllyCount;i++)_allyPulse[i]=.45f;}
            // Same FILL crop as Figma; preserve background aspect ratio.
            var textureAspect=(float)BackgroundArt.width/BackgroundArt.height;
            var viewportAspect=w/Mathf.Max(1,h);
            _background.uvRect=textureAspect>viewportAspect
                ?new Rect((1-viewportAspect/textureAspect)/2,0,viewportAspect/textureAspect,1)
                :new Rect(0,(1-textureAspect/viewportAspect)/2,1,textureAspect/viewportAspect);
        }
        static void Place(RawImage image,float x,float y,float width,float height)
        { image.rectTransform.anchoredPosition=new Vector2(x,y);image.rectTransform.sizeDelta=new Vector2(width,height); }
        void OnDestroy()
        {
            if(_playerLoop!=null)_playerLoop.AttackApplied-=PlayerAttack;
            if(_alliedLoop!=null)_alliedLoop.AttackApplied-=AlliedAttack;
        }
    }
}
