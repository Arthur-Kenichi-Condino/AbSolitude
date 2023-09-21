#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Actors.Combat;
using AKCondinoO.Sims.Actors.Homunculi.Vanilmirth;
using AKCondinoO.Sims.Actors.Skills;
using AKCondinoO.Sims.Inventory;
using AKCondinoO.Sims.Weapons.Rifle.SniperRifle;
using System;
using System.Collections;
using System.Collections.Generic;
using UMA;
using UMA.CharacterSystem;
using UnityEngine;
using UnityEngine.AI;
using static AKCondinoO.GameMode;
using static AKCondinoO.Sims.Actors.SimActor.PersistentSimActorData;
using static AKCondinoO.Voxels.VoxelSystem;
namespace AKCondinoO.Sims.Actors{
    internal partial class BaseAI:SimActor{
     internal SimActorCharacterController characterController;
      internal float height;
       internal float heightCrouching;
     internal SimAnimatorController animatorController;
     internal AISensor aiSensor;
        protected override void Awake(){
         InitTargets();
         base.Awake();
         aiSensor=GetComponentInChildren<AISensor>();
         if(aiSensor){
          aiSensor.actor=this;
          aiSensor.Deactivate();
          Log.DebugMessage("aiSensor found, search for actor's \"head\" to add sight");
         }
         navMeshAgent=GetComponent<NavMeshAgent>();
         navMeshQueryFilter=new NavMeshQueryFilter(){
          agentTypeID=navMeshAgent.agentTypeID,
             areaMask=navMeshAgent.areaMask,
         };
         characterController=GetComponent<SimActorCharacterController>();
         if(characterController!=null){
            characterController.actor=this;
          height=characterController.character.height;
         }
         heightCrouching=navMeshAgent.height;
         if(characterController==null){
          height=heightCrouching;
         }
         Log.DebugMessage("height:"+height+";heightCrouching:"+heightCrouching);
         animatorController=GetComponent<SimAnimatorController>();
         animatorController.actor=this;
        }
     protected bool canSense;
        protected override void OnUMACharacterUpdated(UMAData simUMAData){
         if(aiSensor){
          if(head||leftEye||rightEye){
           Log.DebugMessage("aiSensor found, sync with actor's \"head's\" and/or \"eyes'\" transforms for providing eyesight to AI");
           canSense=true;
          }
         }
         OnCreateHitHurtBoxes(simUMA,simUMAData);
         base.OnUMACharacterUpdated(simUMAData);
        }
     [SerializeField]protected HitboxesPrefabsList hitboxesPrefabs;
      [SerializeField]protected HurtboxesPrefabsList hurtboxesPrefabs;
     protected readonly List<Hitboxes>hitboxes=new List<Hitboxes>();
      protected readonly List<Hurtboxes>hurtboxes=new List<Hurtboxes>();
     internal readonly Dictionary<string,Hitboxes>nameToHitboxes=new Dictionary<string,Hitboxes>();
        protected virtual void OnCreateHitHurtBoxes(DynamicCharacterAvatar simUMA,UMAData simUMAData){
         foreach(Hitboxes hitbox in hitboxes){
          if(hitbox!=null){
           DestroyImmediate(hitbox);
          }
         }
         hitboxes.Clear();
         nameToHitboxes.Clear();
         foreach(Hurtboxes hurtbox in hurtboxes){
          if(hurtbox!=null){
           DestroyImmediate(hurtbox);
          }
         }
         hurtboxes.Clear();
         if(hitboxesPrefabs!=null&&hitboxesPrefabs.prefabs.Length>0){
          foreach(Hitboxes hitboxPrefab in hitboxesPrefabs.prefabs){
           if(nameToBodyPart.TryGetValue(hitboxPrefab.name,out Transform bodyPart)){
            Log.DebugMessage("create Hitbox from hitboxPrefab.name:"+hitboxPrefab.name);
            Vector3 offset=hitboxPrefab.transform.localPosition;
            Quaternion rotation=hitboxPrefab.transform.localRotation;
            Hitboxes hitbox=Instantiate(hitboxPrefab);
            hitbox.transform.SetParent(bodyPart,false);
            offset=Vector3.Scale(offset,hitbox.transform.lossyScale);
            hitbox.transform.localPosition=offset;
            hitbox.transform.localRotation=rotation;
            hitbox.kinematicRigidbody=hitbox.gameObject.AddComponent<Rigidbody>();
            hitbox.kinematicRigidbody.isKinematic=true;
            hitbox.actor=this;
            hitboxes.Add(hitbox);
            nameToHitboxes.Add(hitboxPrefab.name,hitbox);
           }
          }
         }
         if(hurtboxesPrefabs!=null&&hurtboxesPrefabs.prefabs.Length>0){
          foreach(Hurtboxes hurtboxPrefab in hurtboxesPrefabs.prefabs){
           if(nameToBodyPart.TryGetValue(hurtboxPrefab.name,out Transform bodyPart)){
            Log.DebugMessage("create Hurtbox from hurtboxPrefab.name:"+hurtboxPrefab.name);
            Vector3 offset=hurtboxPrefab.transform.localPosition;
            Quaternion rotation=hurtboxPrefab.transform.localRotation;
            Hurtboxes hurtbox=Instantiate(hurtboxPrefab);
            hurtbox.transform.SetParent(bodyPart,false);
            offset=Vector3.Scale(offset,hurtbox.transform.lossyScale);
            hurtbox.transform.localPosition=offset;
            hurtbox.transform.localRotation=rotation;
            hurtbox.kinematicRigidbody=hurtbox.gameObject.AddComponent<Rigidbody>();
            hurtbox.kinematicRigidbody.isKinematic=true;
            hurtbox.actor=this;
            hurtboxes.Add(hurtbox);
           }
          }
         }
        }
     internal readonly Dictionary<Hitboxes,float>hitGracePeriod=new Dictionary<Hitboxes,float>();
      readonly List<Hitboxes>hits=new List<Hitboxes>();
        internal void HitHurtBoxesUpdate(){
         hits.AddRange(hitGracePeriod.Keys);
         foreach(Hitboxes hit in hits){
          float gracePeriod=hitGracePeriod[hit];
          gracePeriod-=Time.deltaTime;
          if(gracePeriod<=0f){
           hitGracePeriod.Remove(hit);
          }else{
           hitGracePeriod[hit]=gracePeriod;
          }
         }
         hits.Clear();
        }
        internal override bool IsMonster(){
         return MyAggressionMode==AggressionMode.AggressiveToAll;
        }
     internal readonly Dictionary<Type,SkillData>requiredSkills=new Dictionary<Type,SkillData>();
      internal readonly Dictionary<Type,Skill>skills=new Dictionary<Type,Skill>();
     internal readonly Dictionary<Type,List<SlaveData>>requiredSlaves=new Dictionary<Type,List<SlaveData>>();
      internal readonly HashSet<(Type simObjectType,ulong idNumber)>slaves=new HashSet<(Type,ulong)>();
        internal override void OnActivated(){
         base.OnActivated();
         requiredSkills.Add(typeof(OnHitGracePeriod),new SkillData(){skill=typeof(OnHitGracePeriod),level=10,});
         //  load skills from file here:
         persistentSimActorData.skills.Reset();
         while(persistentSimActorData.skills.MoveNext()){
          SkillData skillData=persistentSimActorData.skills.Current;
          if(!ReflectionUtil.IsTypeDerivedFrom(skillData.skill,typeof(Skill))){
           Log.Warning("invalid skill type:"+skillData.skill);
           continue;
          }
          (GameObject skillGameObject,Skill skill)spawnedSkill=SkillsManager.singleton.SpawnSkillGameObject(skillData.skill,skillData.level,this);
          skills.Add(skillData.skill,spawnedSkill.skill);
         }
         foreach(var skill in skills){
          if(requiredSkills.TryGetValue(skill.Key,out SkillData requiredSkill)){
           if(skill.Value.level<requiredSkill.level){
            skill.Value.level=requiredSkill.level;
           }
          }
          requiredSkills.Remove(skill.Key);
         }
         if(requiredSkills.Count>0){
          Log.DebugMessage("required skills missing");
          foreach(var requiredSkill in requiredSkills){
           if(!ReflectionUtil.IsTypeDerivedFrom(requiredSkill.Key,typeof(Skill))){
            Log.Warning("invalid skill type:"+requiredSkill.Key);
            continue;
           }
           (GameObject skillGameObject,Skill skill)spawnedSkill=SkillsManager.singleton.SpawnSkillGameObject(requiredSkill.Key,requiredSkill.Value.level,this);
           skills.Add(requiredSkill.Key,spawnedSkill.skill);
          }
         }
         requiredSkills.Clear();
         slaves.Clear();
         //  load slaves from file here:
         persistentSimActorData.slaves.Reset();
         while(persistentSimActorData.slaves.MoveNext()){
          SlaveData slaveData=persistentSimActorData.slaves.Current;
          slaves.Add((slaveData.simObjectType,slaveData.idNumber));
         }
         foreach(var slave in slaves){
          if(requiredSlaves.TryGetValue(slave.simObjectType,out List<SlaveData>requiredSlavesForType)){
           //  TO DO: do some checks and set variables here
           requiredSlaves.Remove(slave.simObjectType);
          }
         }
         persistentSimActorData.UpdateData(this);
         lastForward=transform.forward;
         OnResetMotion();
         if(onChaseGetDataCoroutine!=null){
          StopCoroutine(onChaseGetDataCoroutine);onChaseGetDataCoroutine=null;
         }
         onChaseGetDataCoroutine=StartCoroutine(OnChaseGetDataCoroutine());
        }
        internal override void OnDeactivated(){
         if(onChaseGetDataCoroutine!=null){
          StopCoroutine(onChaseGetDataCoroutine);onChaseGetDataCoroutine=null;
         }
         if(aiSensor){
          aiSensor.Deactivate();
         }
         Log.DebugMessage("sim actor:OnDeactivated:id:"+id);
         foreach(var skill in skills){
          SkillsManager.singleton.Pool(skill.Key,skill.Value);
         }
         skills.Clear();//  to do: pool skills before clearing the list
         base.OnDeactivated();
         ReleaseTargets();
        }
        protected override void SetSlave(SimObject slave){
         slaves.Add(slave.id.Value);
         base.SetSlave(slave);
         persistentSimActorData.UpdateData(this);
        }
        internal void OnThirdPersonCamFollow(){
         Log.DebugMessage("OnThirdPersonCamFollow()");
         MainCamera.singleton.toFollowActor=this;
         GameMode.singleton.OnGameModeChangeTo(GameModesEnum.ThirdPerson);
        }
     internal bool isUsingAI=true;
     protected Vector3 lastForward=Vector3.forward;
     internal bool crouching{
      get{
       return crouching_v;
      }
     }protected bool crouching_v=false;
        protected virtual void OnToggleCrouching(){
         if(height>heightCrouching){//  can crouch
          if(!crouching_v){
           crouching_v=true;
           characterController.character.height=heightCrouching;
           characterController.character.center=new Vector3(0,-((height/2f)-(heightCrouching/2f)),0);
          }else{
           crouching_v=false;
           characterController.character.height=height;
           characterController.character.center=new Vector3(0,0,0);
          }
         }
        }
        internal Vector3 GetHeadPosition(bool fromAnimator){
         Vector3 headPos;
         if(fromAnimator&&animatorController!=null&&animatorController.animator!=null){
          headPos=animatorController.animator.transform.position+animatorController.animator.transform.rotation*(new Vector3(0f,characterController.character.height/2f+characterController.character.radius,0f)+characterController.headOffset);
         }else{
          headPos=characterController.character.transform.position+characterController.character.transform.rotation*characterController.headOffset;
         }
         return headPos;
        }
     protected Collider[]collidersTouchingUpper=new Collider[8];
      protected int collidersTouchingUpperCount=0;
     internal SimCollisionsChildTrigger simCollisionsTouchingUpper;
     protected Collider[]collidersTouchingMiddle=new Collider[8];
      protected int collidersTouchingMiddleCount=0;
     internal SimCollisionsChildTrigger simCollisionsTouchingMiddle;
        protected override void GetCollidersTouchingNonAlloc(bool instantCheck){
         gotCollidersTouchingFromInstantCheck=false;
         if(!instantCheck){
          if(
           simCollisions&&
           simCollisionsTouchingUpper &&
           simCollisionsTouchingMiddle
          ){
           return;
          }
         }
         if(characterController!=null){
          gotCollidersTouchingFromInstantCheck=true;
          var upperMiddleLowerValues=simCollisions.GetCapsuleValuesForUpperMiddleLowerCollisionTesting(characterController.character,transform.root,height,characterController.center);
          if(upperMiddleLowerValues!=null){
           var point0=upperMiddleLowerValues.Value.upperValues.point0;
           var point1=upperMiddleLowerValues.Value.upperValues.point1;
           float radius=upperMiddleLowerValues.Value.upperValues.radius;
           _GetUpperColliders:{
            collidersTouchingUpperCount=Physics.OverlapCapsuleNonAlloc(
             point0,
             point1,
             radius,
             collidersTouchingUpper
            );
           }
           if(collidersTouchingUpperCount>0){
            if(collidersTouchingUpperCount>=collidersTouchingUpper.Length){
             Array.Resize(ref collidersTouchingUpper,collidersTouchingUpperCount*2);
             goto _GetUpperColliders;
            }
           }
           point0=upperMiddleLowerValues.Value.middleValues.point0;
           point1=upperMiddleLowerValues.Value.middleValues.point1;
           _GetMiddleColliders:{
            collidersTouchingMiddleCount=Physics.OverlapCapsuleNonAlloc(
             point0,
             point1,
             radius,
             collidersTouchingMiddle
            );
           }
           if(collidersTouchingMiddleCount>0){
            if(collidersTouchingMiddleCount>=collidersTouchingMiddle.Length){
             Array.Resize(ref collidersTouchingMiddle,collidersTouchingMiddleCount*2);
             goto _GetMiddleColliders;
            }
           }
          }
         }
        }
     protected State MyState=State.IDLE_ST;internal State state{get{return MyState;}}
     protected Vector3 MyDest;internal Vector3 dest{get{return MyDest;}}
     protected PathfindingResult MyPathfinding=PathfindingResult.IDLE;internal PathfindingResult pathfinding{get{return MyPathfinding;}}
     protected WeaponTypes MyWeaponType=WeaponTypes.None;internal WeaponTypes weaponType{get{return MyWeaponType;}}
        protected virtual void AI(){
         RenewTargets();
         MyPathfinding=GetPathfindingResult();
         stopPathfindingOnTimeout=true;
         //Log.DebugMessage("MyPathfinding is:"+MyPathfinding);
         if(MyEnemy!=null){
          if(IsInAttackRange(MyEnemy)){
           MyState=State.ATTACK_ST;
           goto _MyStateSet;
          }else{
           if(MyState!=State.CHASE_ST){
            OnCHASE_ST_START();
           }
           MyState=State.CHASE_ST;
           goto _MyStateSet;
          }
         }else{
          if(masterId!=null){
           float disToMaster=GetDistance(this,masterSimObject);
           if(disToMaster>=0f){
            if(disToMaster>8f){
             //Log.DebugMessage("I should follow my master:"+masterSimObject+";this:"+this);
             MyState=State.FOLLOW_ST;
             goto _MyStateSet;
            }
           }
          }
          if(MyState!=State.IDLE_ST){
           OnIDLE_ST_START();
          }
          MyState=State.IDLE_ST;
          goto _MyStateSet;
         }
         _MyStateSet:{}
         SetBestSkillToUse(Skill.SkillUseContext.OnCallSlaves);
         if(MyState==State.IDLE_ST){SetBestSkillToUse(Skill.SkillUseContext.OnIdle);}
         if(MySkill!=null){
          DoSkill();
         }
         if      (MyState==State.ATTACK_ST){
          OnATTACK_ST();
         }else if(MyState==State. CHASE_ST){
           OnCHASE_ST();
         }else if(MyState==State.FOLLOW_ST){
          OnFOLLOW_ST();
         }else{
            OnIDLE_ST();
         }
         UpdateMotion(true);
        }
        protected virtual void OnCharacterControllerUpdated(){
         UpdateMotion(false);
        }
        protected virtual void OnIDLE_ST_START(){
         characterController.character.transform.localRotation=Quaternion.identity;
        }
     [SerializeField]protected bool doIdleMove=true;
     [SerializeField]protected float useRunSpeedChance=0.5f;
     [SerializeField]protected float delayToRandomMove=8.0f;
     protected float timerToRandomMove=2.0f;
        protected virtual void OnIDLE_ST(){
         if(
          !IsTraversingPath()
         ){
          if(timerToRandomMove>0.0f){
             timerToRandomMove-=Time.deltaTime;
          }else if(doIdleMove){
             timerToRandomMove=delayToRandomMove;
           //Log.DebugMessage("can do random movement");
           if(GetRandomPosition(transform.position,8.0f,out Vector3 result)){
            //Log.DebugMessage("got random position:"+result);
            bool run=Mathf.Clamp01((float)math_random.NextDouble())<useRunSpeedChance;
            if(navMeshAgentShouldUseRunSpeed||run){
             navMeshAgent.speed=navMeshAgentRunSpeed;
            }else{
             navMeshAgent.speed=navMeshAgentWalkSpeed;
            }
            navMeshAgent.destination=result;
           }
          }
         }
        }
        protected virtual void OnFOLLOW_ST(){
         //Log.DebugMessage("OnFOLLOW_ST()");
         stopPathfindingOnTimeout=false;//
         if(
          !IsTraversingPath()
         ){
          if(masterSimObject is BaseAI masterAI){
           if(masterAI.isUsingAI){
            if(masterAI.state==State.IDLE_ST){
             MoveToMasterRandom(masterAI,4f);
            }else{
             MoveToMaster      (masterAI,0f);
            }
           }else{
            if(!masterAI.IsMoving()){
             MoveToMasterRandom(masterAI,4f);
            }else{
             MoveToMaster      (masterAI,0f);
            }
           }
          }else{
          }
         }
        }
        protected virtual void OnCHASE_ST_START(){
         characterController.character.transform.localRotation=Quaternion.identity;
         onChaseMyEnemyPos=onChaseMyEnemyPos_Last=MyEnemy.transform.position;
         onChaseRenewDestinationTimer=onChaseRenewDestinationTimeInterval;
         onChaseMyEnemyMovedSoChangeDestinationTimer=0f;
         onChaseMyEnemyMovedSoChangeDestination=true;
        }
     protected Coroutine onChaseGetDataCoroutine;
     protected WaitUntil onChaseGetDataThrottler;
      protected float onChaseGetDataThrottlerInterval=.125f;
       protected float onChaseGetDataThrottlerTimer;
     protected RaycastHit[]onChaseInTheWayColliderHits=new RaycastHit[8];
      protected int onChaseInTheWayColliderHitsCount=0;
        protected virtual IEnumerator OnChaseGetDataCoroutine(){
         onChaseGetDataThrottler=new WaitUntil(
          ()=>{
           if(onChaseGetDataThrottlerTimer>0f){
            onChaseGetDataThrottlerTimer-=Time.deltaTime;
           }
           if(MyState==State.CHASE_ST){
            if(MyEnemy==null){
             return false;
            }
            if(onChaseGetDataThrottlerTimer<=0f){
             onChaseGetDataThrottlerTimer=onChaseGetDataThrottlerInterval;
             return true;
            }
           }
           return false;
          }
         );
         Loop:{
          yield return onChaseGetDataThrottler;
          //Log.DebugMessage("OnChaseGetDataCoroutine");
          if(characterController!=null){
           var values=simCollisions.GetCapsuleValuesForCollisionTesting(characterController.character,transform.root);
           float maxDis=Vector3.Distance(MyEnemy.transform.position,transform.root.position);
           int inTheWayLength=0;
           _GetInTheWayColliderHits:{
            inTheWayLength=Physics.CapsuleCastNonAlloc(
             values.point0,
             values.point1,
             values.radius,
             (MyEnemy.transform.position-transform.root.position).normalized,
             onChaseInTheWayColliderHits,
             maxDis,
             PhysUtil.physObstaclesLayer
            );
           }
           if(inTheWayLength>0){
            if(inTheWayLength>=onChaseInTheWayColliderHits.Length){
             Array.Resize(ref onChaseInTheWayColliderHits,inTheWayLength*2);
             goto _GetInTheWayColliderHits;
            }
           }
           onChaseInTheWayColliderHitsCount=inTheWayLength;
           if(onChaseInTheWayColliderHitsCount>0){
            for(int i=onChaseInTheWayColliderHits.Length-1;i>=0;--i){
             if(i>=onChaseInTheWayColliderHitsCount){
              onChaseInTheWayColliderHits[i]=default(RaycastHit);
              continue;
             }
             RaycastHit hit=onChaseInTheWayColliderHits[i];
             if(hit.collider.transform.root==this.transform.root){
              onChaseInTheWayColliderHits[i]=default(RaycastHit);
              onChaseInTheWayColliderHitsCount--;
             }
            }
            Array.Sort(onChaseInTheWayColliderHits,OnChaseInTheWayColliderHitsArraySortComparer);
           }
          }
         }
         goto Loop;
        }
        private int OnChaseInTheWayColliderHitsArraySortComparer(RaycastHit a,RaycastHit b){
         if(a.collider==null&&b.collider==null){
          return 0;
         }
         if(a.collider==null&&b.collider!=null){
          return 1;
         }
         if(a.collider!=null&&b.collider==null){
          return -1;
         }
         return Vector3.Distance(transform.root.position,a.point).CompareTo(Vector3.Distance(transform.root.position,b.point));
        }
        internal enum OnChaseTimeoutReactionCodes:int{
         Random=8,
         GoLeft=16,
         GoRight=24,
         ResetCounter=32,
        }
     protected int onChaseTimeoutFailCount=0;
      protected OnChaseTimeoutReactionCodes onChaseTimeoutReactionCode;
        protected virtual void OnChaseTimeoutFail(){
         if(++onChaseTimeoutFailCount>=(int)OnChaseTimeoutReactionCodes.ResetCounter){
          onChaseTimeoutFailCount=0;
         }
         if      (onChaseTimeoutFailCount>=(int)OnChaseTimeoutReactionCodes.GoRight){
          onChaseTimeoutReactionCode=OnChaseTimeoutReactionCodes.GoRight;
         }else if(onChaseTimeoutFailCount>=(int)OnChaseTimeoutReactionCodes.GoLeft){
          onChaseTimeoutReactionCode=OnChaseTimeoutReactionCodes.GoLeft;
         }else if(onChaseTimeoutFailCount>=(int)OnChaseTimeoutReactionCodes.Random){
          onChaseTimeoutReactionCode=OnChaseTimeoutReactionCodes.Random;
         }
        }
     protected Vector3 onChaseMyEnemyPos,onChaseMyEnemyPos_Last;
     protected float onChaseRenewDestinationTimeInterval=4f;
      protected float onChaseRenewDestinationTimer=4f;
     protected float onChaseMyEnemyMovedSoChangeDestinationTimeInterval=.2f;
      protected float onChaseMyEnemyMovedSoChangeDestinationTimer=0f;
       protected bool onChaseMyEnemyMovedSoChangeDestination=true;
        protected virtual void OnCHASE_ST(){
         stopPathfindingOnTimeout=false;//
         if((onChaseMyEnemyPos_Last=onChaseMyEnemyPos)!=(onChaseMyEnemyPos=MyEnemy.transform.position)){
          onChaseMyEnemyMovedSoChangeDestination=true;
         }
         bool moveToDestination=false;
         if(onChaseRenewDestinationTimer>0f){
          onChaseRenewDestinationTimer-=Time.deltaTime;
         }
         if(onChaseRenewDestinationTimer<=0f){
          onChaseRenewDestinationTimer=onChaseRenewDestinationTimeInterval;
          moveToDestination|=true;
         }
         if(
          !IsTraversingPath()
         ){
          if(MyPathfinding==PathfindingResult.TIMEOUT){
           OnChaseTimeoutFail();
          }
          moveToDestination|=true;
         }
         if(onChaseMyEnemyMovedSoChangeDestinationTimer>0f){
          onChaseMyEnemyMovedSoChangeDestinationTimer-=Time.deltaTime;
         }
         if(onChaseMyEnemyMovedSoChangeDestination){
          if(onChaseMyEnemyMovedSoChangeDestinationTimer<=0f){
           onChaseMyEnemyMovedSoChangeDestinationTimer=onChaseMyEnemyMovedSoChangeDestinationTimeInterval;
           onChaseMyEnemyMovedSoChangeDestination=false;
           moveToDestination|=true;
          }
         }
         if(moveToDestination){
          MyDest=MyEnemy.transform.position;
          if(onChaseInTheWayColliderHitsCount>0){
           if(characterController!=null){
            for(int i=0;i<onChaseInTheWayColliderHitsCount;++i){
             RaycastHit hit=onChaseInTheWayColliderHits[i];
             if(hit.collider.transform.root.GetComponentInChildren<SimObject>()is BaseAI actorHit&&actorHit.characterController!=null&&(actorHit.transform.root.position-transform.root.position).sqrMagnitude<(MyEnemy.transform.root.position-transform.root.position).sqrMagnitude){
              Vector3 cross=Vector3.Cross(transform.root.position,actorHit.transform.root.position);
              //Debug.DrawLine(actorHit.transform.root.position,transform.root.position,Color.blue,1f);
              //Debug.DrawRay(actorHit.transform.root.position,cross,Color.cyan,1f);
              Vector3 right=cross;
              right.y=actorHit.transform.root.position.y;
              right.Normalize();
              //Debug.DrawRay(actorHit.transform.root.position,right,Color.cyan,1f);
              Vector3 cross2=Vector3.Cross(actorHit.transform.root.position+right,actorHit.transform.root.position+Vector3.up);
              Vector3 forward=cross2;
              forward.y=actorHit.transform.root.position.y;
              forward.Normalize();
              //Debug.DrawRay(actorHit.transform.root.position,forward,Color.cyan,1f);
              int rightSign=1;
              float rightDis=3.0f;
              float forwardDis=1.5f;
              if      (onChaseTimeoutReactionCode==OnChaseTimeoutReactionCodes.Random){
               rightSign=math_random.CoinFlip()?-1:1;
               rightDis=(float)math_random.NextDouble(2.0d,6d);
               forwardDis=(float)math_random.NextDouble(1.0d,6d);
              }else if(onChaseTimeoutReactionCode==OnChaseTimeoutReactionCodes.GoLeft){
               rightSign=-1;
               rightDis=(float)math_random.NextDouble(3.0d,6.0d);
               forwardDis=(float)math_random.NextDouble(1.5d,3.0d);
              }else if(onChaseTimeoutReactionCode==OnChaseTimeoutReactionCodes.GoRight){
               rightSign=1;
               rightDis=(float)math_random.NextDouble(3.0d,6.0d);
               forwardDis=(float)math_random.NextDouble(1.5d,3.0d);
              }
              MyDest=actorHit.transform.root.position+((right*rightSign)*rightDis-forward*forwardDis)*(actorHit.characterController.character.radius+characterController.character.radius)+Vector3.down*(height/2f);
              break;
             }
            }
           }
          }
          navMeshAgent.destination=MyDest;
         }
        }
     internal QuaternionRotLerpHelper onAttackPlanarLookRotLerpForCharacterControllerToAimAtMyEnemy=new QuaternionRotLerpHelper(38,.0005f);
        protected virtual void OnATTACK_ST(){
         if(
          IsTraversingPath()
         ){
          navMeshAgent.destination=navMeshAgent.transform.position;
         }else{
          if(characterController!=null){
           Vector3 lookDir=MyEnemy.transform.position-transform.position;
           Vector3 planarLookDir=lookDir;
           planarLookDir.y=0f;
           onAttackPlanarLookRotLerpForCharacterControllerToAimAtMyEnemy.tgtRot=Quaternion.LookRotation(planarLookDir);
           characterController.character.transform.rotation=onAttackPlanarLookRotLerpForCharacterControllerToAimAtMyEnemy.UpdateRotation(characterController.character.transform.rotation,Core.magicDeltaTimeNumber);
           Debug.DrawRay(characterController.character.transform.position,characterController.character.transform.forward,Color.gray);
           if(simUMA!=null){
            Vector3 animatorLookDir=-simUMA.transform.parent.forward;
            Vector3 animatorLookEuler=simUMA.transform.parent.eulerAngles;
            animatorLookEuler.y+=180f;
            Vector3 animatorPlanarLookEuler=animatorLookEuler;
            animatorPlanarLookEuler.x=0f;
            animatorPlanarLookEuler.z=0f;
            Vector3 animatorPlanarLookDir=Quaternion.Euler(animatorPlanarLookEuler)*Vector3.forward;
            Debug.DrawRay(characterController.character.transform.position,animatorPlanarLookDir,Color.white);
            if(Vector3.Angle(characterController.character.transform.forward,animatorPlanarLookDir)<=5f){
             DoAttack();
            }
           }
          }
         }
        }
        protected virtual void OnResetMotion(){
         onHitSetMotion=false;
          onHitResetMotion=false;
         onDoAttackSetMotion=false;
         MyMotion=ActorMotion.MOTION_STAND;
         navMeshAgentShouldBeStopped=false;
         if(characterController!=null){
          characterController.isStopped=false;
         }
        }
        protected virtual void OnMotionHitSet(){
         navMeshAgentShouldBeStopped=true;
         if(characterController!=null){
          characterController.isStopped=true;
         }
        }
        protected virtual void OnMotionHitReset(){
         navMeshAgentShouldBeStopped=true;
         if(characterController!=null){
          characterController.isStopped=true;
         }
        }
        protected virtual void OnMotionHitAnimationEnd(){
         navMeshAgentShouldBeStopped=false;
         if(characterController!=null){
          characterController.isStopped=false;
         }
        }
        internal virtual void UpdateGetters(){
         float velocityFlattened=0f;
         if(isUsingAI){
          float velocityMagnitude=moveVelocity.magnitude;
          //Log.DebugMessage("navMeshAgent velocityMagnitude:"+velocityMagnitude);
          velocityFlattened=velocityMagnitude/navMeshAgentRunSpeed;
         }else if(characterController!=null){
          velocityFlattened=moveVelocity.z;
          //Log.DebugMessage("characterController velocityFlattened:"+velocityFlattened);
         }
         moveVelocityFlattenedLerp.tgtVal=Math.Clamp(velocityFlattened,-1f,1f);
         moveVelocityFlattened_value=moveVelocityFlattenedLerp.UpdateFloat(moveVelocityFlattened_value,Core.magicDeltaTimeNumber);
         float strafeVelocityFlattened=0f;
         if(isUsingAI){
         }else if(characterController!=null){
          strafeVelocityFlattened=moveVelocity.x;
          //Log.DebugMessage("characterController strafeVelocityFlattened:"+strafeVelocityFlattened);
         }
         moveStrafeVelocityFlattenedLerp.tgtVal=Math.Clamp(strafeVelocityFlattened,-1f,1f);
         moveStrafeVelocityFlattened_value=moveStrafeVelocityFlattenedLerp.UpdateFloat(moveStrafeVelocityFlattened_value,Core.magicDeltaTimeNumber);
         float angle=0f;
         if(isUsingAI){
          if(!Mathf.Approximately(moveVelocity.magnitude,0f)){
           angle=Vector3.SignedAngle(transform.forward,moveVelocity.normalized,transform.up)/180f;
           //Log.DebugMessage("angle:"+angle);
          }
         }else if(characterController!=null){
          angle=Vector3.SignedAngle(characterController.lastBodyRotation*Vector3.forward,characterController.bodyRotation*Vector3.forward,transform.up);
         }
         turnAngleLerp.tgtVal=Math.Clamp(angle,-.5f,.5f);
         turnAngle_value=turnAngleLerp.UpdateFloat(turnAngle_value,Core.magicDeltaTimeNumber);
        }
     internal virtual Vector3 moveVelocity{
      get{
       if(isUsingAI){
        return navMeshAgent.velocity;
       }else if(characterController!=null){
        float divideBy=
         (characterController.inputMoveVelocity.z!=0f?(Mathf.Abs(characterController.inputMoveVelocity.z)/(characterController.maxMoveSpeed.z*characterController.isRunningMoveSpeedMultiplier)):0f)+
         (characterController.inputMoveVelocity.x!=0f?(Mathf.Abs(characterController.inputMoveVelocity.x)/(characterController.maxMoveSpeed.x*characterController.isRunningMoveSpeedMultiplier)):0f);
        Vector3 velocity=
         Vector3.Scale(
          characterController.inputMoveVelocity,
          new Vector3(
           1f/((divideBy==0f?1f:divideBy)*characterController.walkSpeedAverage*2f),
           0f,
           1f/((divideBy==0f?1f:divideBy)*characterController.walkSpeedAverage*2f)
          )
         );
        //Log.DebugMessage("characterController velocity:"+velocity);
        return velocity;
       }
       return Vector3.zero;
      }
     }
     internal virtual bool isMovingBackwards{
      get{
       return moveVelocityFlattened<0f;
      }
     }
     [SerializeField]internal FloatLerpHelper moveVelocityFlattenedLerp=new FloatLerpHelper();
      protected float moveVelocityFlattened_value;
     internal virtual float moveVelocityFlattened{
      get{
       return moveVelocityFlattened_value;
      }
     }
     [SerializeField]internal FloatLerpHelper moveStrafeVelocityFlattenedLerp=new FloatLerpHelper();
      protected float moveStrafeVelocityFlattened_value;
     internal virtual float moveStrafeVelocityFlattened{
      get{
       return moveStrafeVelocityFlattened_value;
      }
     }
     [SerializeField]internal FloatLerpHelper turnAngleLerp=new FloatLerpHelper();
      protected float turnAngle_value;
     internal float turnAngle{
      get{
       return turnAngle_value;
      }
     }
     internal bool isAiming{
      get{
       if(characterController!=null){
        return characterController.isAiming;
       }
       return false;
      }
     }
     internal bool isShooting{
      get{
       if(characterController!=null){
        if(this is BaseAI baseAI){
         return baseAI.onDoShootingSetMotion;
        }
       }
       return false;
      }
     }
    }
}