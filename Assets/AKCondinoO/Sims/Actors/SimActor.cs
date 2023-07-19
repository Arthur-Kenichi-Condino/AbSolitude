#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Actors.Skills;
using AKCondinoO.Sims.Actors.Combat;
using AKCondinoO.Sims.Inventory;
using AKCondinoO.Sims.Weapons.Rifle.SniperRifle;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UMA;
using UMA.CharacterSystem;
using UnityEngine;
using UnityEngine.AI;
using static AKCondinoO.GameMode;
using static AKCondinoO.InputHandler;
using static AKCondinoO.Sims.Actors.SimActor.PersistentSimActorData;
using static AKCondinoO.Voxels.VoxelSystem;
namespace AKCondinoO.Sims.Actors{
    internal partial class SimActor:SimObject{
     [SerializeField]GameObject simUMAPrefab;
     internal PersistentSimActorData persistentSimActorData;
        //  [https://stackoverflow.com/questions/945664/can-structs-contain-fields-of-reference-types]
        internal struct PersistentSimActorData{
         public ListWrapper<SkillData>skills;
            public struct SkillData{
             public Type skill;public int level;
            }
         public ListWrapper<SlaveData>slaves;
            public struct SlaveData{
             public Type simObjectType;public ulong idNumber;
            }
         public float timerToRandomMove;
            internal void UpdateData(SimActor simActor){
             skills=new ListWrapper<SkillData>(simActor.skills.Select(kvp=>{return new SkillData{skill=kvp.Key,level=kvp.Value.level};}).ToList());
             slaves=new ListWrapper<SlaveData>(simActor.slaves.Select(v  =>{return new SlaveData{simObjectType=v.simObjectType,idNumber=v.idNumber};}).ToList());
            }
         private static readonly ConcurrentQueue<StringBuilder>stringBuilderPool=new ConcurrentQueue<StringBuilder>();
            public override string ToString(){
             if(!stringBuilderPool.TryDequeue(out StringBuilder stringBuilder)){
              stringBuilder=new StringBuilder();
             }
             stringBuilder.Clear();
             stringBuilder.AppendFormat(CultureInfoUtil.en_US,"skills={{ ");
             skills.Reset();
             while(skills.MoveNext()){
              SkillData skill=skills.Current;
              stringBuilder.AppendFormat(CultureInfoUtil.en_US,"[{0},{1}], ",skill.skill,skill.level);
             }
             stringBuilder.AppendFormat(CultureInfoUtil.en_US,"}} , ");
             stringBuilder.AppendFormat(CultureInfoUtil.en_US,"slaves={{ ");
             slaves.Reset();
             while(slaves.MoveNext()){
              SlaveData slave=slaves.Current;
              stringBuilder.AppendFormat(CultureInfoUtil.en_US,"[{0},{1}], ",slave.simObjectType,slave.idNumber);
             }
             stringBuilder.AppendFormat(CultureInfoUtil.en_US,"}} , ");
             string result=string.Format(CultureInfoUtil.en_US,"persistentSimActorData={{ {0}, }}",stringBuilder.ToString());
             stringBuilderPool.Enqueue(stringBuilder);
             return result;
            }
         private static readonly ConcurrentQueue<List<SkillData>>parsingSkillListPool=new ConcurrentQueue<List<SkillData>>();
         private static readonly ConcurrentQueue<List<SlaveData>>parsingSlaveListPool=new ConcurrentQueue<List<SlaveData>>();
            internal static PersistentSimActorData Parse(string s){
             PersistentSimActorData persistentSimActorData=new PersistentSimActorData();
             if(!parsingSkillListPool.TryDequeue(out List<SkillData>skillList)){
              skillList=new List<SkillData>();
             }
             skillList.Clear();
             if(!parsingSlaveListPool.TryDequeue(out List<SlaveData>slaveList)){
              slaveList=new List<SlaveData>();
             }
             slaveList.Clear();
             //Log.DebugMessage("s:"+s);
             int skillsStringStart=s.IndexOf("skills={");
             if(skillsStringStart>=0){
                skillsStringStart+=8;
              int skillsStringEnd=s.IndexOf("} , ",skillsStringStart);
              string skillsString=s.Substring(skillsStringStart,skillsStringEnd-skillsStringStart);
              int skillStringStart=0;
              while((skillStringStart=skillsString.IndexOf("[",skillStringStart))>=0){
               int skillAssetTypeStringStart=skillStringStart+1;
               int skillAssetTypeStringEnd  =skillsString.IndexOf(",",skillAssetTypeStringStart);
               Type skillAssetType=Type.GetType(skillsString.Substring(skillAssetTypeStringStart,skillAssetTypeStringEnd-skillAssetTypeStringStart));
               int skillLevelStringStart=skillAssetTypeStringEnd+1;
               int skillLevelStringEnd  =skillsString.IndexOf("],",skillLevelStringStart);
               int skillLevel=int.Parse(skillsString.Substring(skillLevelStringStart,skillLevelStringEnd-skillLevelStringStart));
               //Log.DebugMessage("skillType:"+skillType+";skillLevel:"+skillLevel);
               SkillData skill=new SkillData(){
                skill=skillAssetType,
                level=skillLevel,
               };
               skillList.Add(skill);
               skillStringStart=skillLevelStringEnd+2;
              }
             }
             int slavesStringStart=s.IndexOf("slaves={");
             if(slavesStringStart>=0){
                slavesStringStart+=8;
              int slavesStringEnd=s.IndexOf("} , ",slavesStringStart);
              string slavesString=s.Substring(slavesStringStart,slavesStringEnd-slavesStringStart);
              //Log.DebugMessage("slavesString:"+slavesString);
              int slaveStringStart=0;
              while((slaveStringStart=slavesString.IndexOf("[",slaveStringStart))>=0){
               int slaveSimObjectTypeStringStart=slaveStringStart+1;
               int slaveSimObjectTypeStringEnd  =slavesString.IndexOf(",",slaveSimObjectTypeStringStart);
               Type slaveSimObjectType=Type.GetType(slavesString.Substring(slaveSimObjectTypeStringStart,slaveSimObjectTypeStringEnd-slaveSimObjectTypeStringStart));
               int slaveIdNumberStringStart=slaveSimObjectTypeStringEnd+1;
               int slaveIdNumberStringEnd  =slavesString.IndexOf("],",slaveIdNumberStringStart);
               ulong slaveIdNumber=ulong.Parse(slavesString.Substring(slaveIdNumberStringStart,slaveIdNumberStringEnd-slaveIdNumberStringStart));
               SlaveData slave=new SlaveData(){
                simObjectType=slaveSimObjectType,
                idNumber=slaveIdNumber,
               };
               slaveList.Add(slave);
               slaveStringStart=slaveIdNumberStringEnd+2;
              }
             }
             persistentSimActorData.skills=new ListWrapper<SkillData>(skillList);
             persistentSimActorData.slaves=new ListWrapper<SlaveData>(slaveList);
             parsingSkillListPool.Enqueue(skillList);
             parsingSlaveListPool.Enqueue(slaveList);
             return persistentSimActorData;
            }
        }
     internal DynamicCharacterAvatar simUMA;
      internal Vector3 simUMAPosOffset;
     internal NavMeshAgent navMeshAgent;
      internal NavMeshQueryFilter navMeshQueryFilter;
       [SerializeField]protected float navMeshAgentWalkSpeed=2f;
        [SerializeField]protected float navMeshAgentRunSpeed=4f;
         protected bool navMeshAgentShouldUseRunSpeed=false;
     internal SimActorCharacterController simActorCharacterController;
      internal float height;
       internal float heightCrouching;
     internal SimActorAnimatorController simActorAnimatorController;
     internal AISensor aiSensor;
        protected override void Awake(){
         if(simUMAPrefab!=null){
          simUMAPosOffset=simUMAPrefab.transform.localPosition;
          simUMA=Instantiate(simUMAPrefab,this.transform).GetComponentInChildren<DynamicCharacterAvatar>();
          Log.DebugMessage("simUMAPosOffset:"+simUMAPosOffset);
          simUMA.CharacterUpdated.AddAction(OnUMACharacterUpdated);
         }
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
         simActorCharacterController=GetComponent<SimActorCharacterController>();
         if(simActorCharacterController!=null){
            simActorCharacterController.actor=this;
          height=simActorCharacterController.characterController.height;
         }
         heightCrouching=navMeshAgent.height;
         if(simActorCharacterController==null){
          height=heightCrouching;
         }
         Log.DebugMessage("height:"+height+";heightCrouching:"+heightCrouching);
         simActorAnimatorController=GetComponent<SimActorAnimatorController>();
         simActorAnimatorController.actor=this;
        }
     protected bool canSense;
        void OnUMACharacterUpdated(UMAData simUMAData){
         Log.DebugMessage("OnUMACharacterUpdated");
         if(head==null){
          head=Util.FindChildRecursively(simUMA.transform,"head");
          if(head==null){
           head=Util.FindChildRecursively(simUMA.transform,"face");
          }
          Log.DebugMessage("head:"+head);
         }
         if( leftEye==null){
           leftEye=Util.FindChildRecursively(simUMA.transform,"lEye");
          Log.DebugMessage("lEye:"+ leftEye);
         }
         if(rightEye==null){
          rightEye=Util.FindChildRecursively(simUMA.transform,"rEye");
          Log.DebugMessage("rEye:"+rightEye);
         }
         if(aiSensor){
          if(head||leftEye||rightEye){
           Log.DebugMessage("aiSensor found, sync with actor's \"head's\" and/or \"eyes'\" transforms for providing eyesight to AI");
           canSense=true;
          }
         }
         if( leftHand==null){
           leftHand=Util.FindChildRecursively(simUMA.transform,"lHand");
          Log.DebugMessage("lHand:"+ leftHand);
         }
         if(rightHand==null){
          rightHand=Util.FindChildRecursively(simUMA.transform,"rHand");
          Log.DebugMessage("rHand:"+rightHand);
         }
         OnCreateHitHurtBoxes(simUMA,simUMAData);
        }
        public override void OnDestroy(){
         if(simUMA!=null){
          DestroyImmediate(simUMA.gameObject);
         }
         base.OnDestroy();
        }
        internal override void OnLoadingPool(){
         base.OnLoadingPool();
        }
     internal readonly Dictionary<Type,SkillData>requiredSkills=new Dictionary<Type,SkillData>();
      internal readonly Dictionary<Type,Skill>skills=new Dictionary<Type,Skill>();
     internal readonly Dictionary<Type,List<SlaveData>>requiredSlaves=new Dictionary<Type,List<SlaveData>>();
      internal readonly HashSet<(Type simObjectType,ulong idNumber)>slaves=new HashSet<(Type,ulong)>();
        internal override void OnActivated(){
         base.OnActivated();
         lastForward=transform.forward;
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
         }
         foreach(var requiredSkill in requiredSkills){
          if(!ReflectionUtil.IsTypeDerivedFrom(requiredSkill.Key,typeof(Skill))){
           Log.Warning("invalid skill type:"+requiredSkill.Key);
           continue;
          }
          (GameObject skillGameObject,Skill skill)spawnedSkill=SkillsManager.singleton.SpawnSkillGameObject(requiredSkill.Key,requiredSkill.Value.level,this);
          skills.Add(requiredSkill.Key,spawnedSkill.skill);
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
        }
        internal override void OnDeactivated(){
         Log.DebugMessage("sim actor:OnDeactivated:id:"+id);
         if(aiSensor){
          aiSensor.Deactivate();
         }
         foreach(var skill in skills){
          SkillsManager.singleton.Pool(skill.Key,skill.Value);
         }
         skills.Clear();//  to do: pool skills before clearing the list
         base.OnDeactivated();
        }
        protected override void SetSlave(SimObject slave){
         slaves.Add(slave.id.Value);
         base.SetSlave(slave);
         persistentSimActorData.UpdateData(this);
        }
        protected override void EnableInteractions(){
         interactionsEnabled=true;
        }
        protected override void DisableInteractions(){
         interactionsEnabled=false;
        }
     protected float onEnableNavMeshAgentProximityTimeout=10f;
      protected float onEnableNavMeshAgentProximityTimer=10f;
        void EnableNavMeshAgent(){
         if(!navMeshAgent.enabled){
          if(NavMesh.SamplePosition(transform.position,out NavMeshHit hitResult,Height,navMeshQueryFilter)){
           if(onEnableNavMeshAgentProximityTimer>0f){
            onEnableNavMeshAgentProximityTimer-=Time.deltaTime;
           }
           if(onEnableNavMeshAgentProximityTimer<=0f||(new Vector3(hitResult.position.x,0f,hitResult.position.z)-new Vector3(transform.position.x,0f,transform.position.z)).magnitude<=2f){
            transform.position=hitResult.position+Vector3.up*navMeshAgent.height/2f;
            navMeshAgent.enabled=true;
            onEnableNavMeshAgentProximityTimer=onEnableNavMeshAgentProximityTimeout;
            //Log.DebugMessage("navMeshAgent is enabled");
           }
          }
         }
        }
        void DisableNavMeshAgent(){
         navMeshAgent.enabled=false;
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
           simActorCharacterController.characterController.height=heightCrouching;
           simActorCharacterController.characterController.center=new Vector3(0,-((height/2f)-(heightCrouching/2f)),0);
          }else{
           crouching_v=false;
           simActorCharacterController.characterController.height=height;
           simActorCharacterController.characterController.center=new Vector3(0,0,0);
          }
         }
        }
     [SerializeField]bool DEBUG_ACTIVATE_THIRD_PERSON_CAM_TO_FOLLOW_THIS=false;
     [SerializeField]bool DEBUG_TOGGLE_CROUCHING=false;
     [SerializeField]bool        DEBUG_TOGGLE_HOLSTER_WEAPON=false;
     [SerializeField]WeaponTypes DEBUG_TOGGLE_HOLSTER_WEAPON_TYPE=WeaponTypes.SniperRifle;
     [SerializeField]float AFKTimeToUseAI=30f;
      float AFKTimerToUseAI;
     bool?wasCrouchingBeforeShouldCrouch;
        internal override int ManualUpdate(bool doValidationChecks){
         int result=0;
         if((result=base.ManualUpdate(doValidationChecks))!=0){
          DisableNavMeshAgent();
          return result;
         }
         bool shouldCrouch=false;//  is crouching required?
         if(Core.singleton.isServer){
          if(IsOwner){
           if(DEBUG_ACTIVATE_THIRD_PERSON_CAM_TO_FOLLOW_THIS){
              DEBUG_ACTIVATE_THIRD_PERSON_CAM_TO_FOLLOW_THIS=false;
            OnThirdPersonCamFollow();
           }
           if(MainCamera.singleton.toFollowActor==this){
            //Log.DebugMessage("following this:"+this);
            if(InputHandler.singleton.activityDetected&&!Enabled.RELEASE_MOUSE.curState){
             isUsingAI=false;
             AFKTimerToUseAI=AFKTimeToUseAI;
             //Log.DebugMessage("start using manual control:"+this);
            }
           }else{
            if(!isUsingAI){
             isUsingAI=true;
             AFKTimerToUseAI=0f;
             Log.DebugMessage("camera stopped following, use AI:"+this);
            }
           }
           if(!isUsingAI){
            if(AFKTimerToUseAI>0f){
             AFKTimerToUseAI-=Time.deltaTime;
            }
            if(AFKTimerToUseAI<=0f){
             isUsingAI=true;
             Log.DebugMessage("AFK for too long, use AI:"+this);
            }
           }
           if(DEBUG_TOGGLE_HOLSTER_WEAPON){
              DEBUG_TOGGLE_HOLSTER_WEAPON=false;
            if(DEBUG_TOGGLE_HOLSTER_WEAPON_TYPE==WeaponTypes.SniperRifle){
             if(SimObjectSpawner.singleton.simInventoryItemsInContainerSettings.allSettings.TryGetValue(typeof(RemingtonModel700BDL),out SimInventoryItemsInContainerSettings.SimObjectSettings simInventoryItemSettings)){
              if(inventoryItemsSpawnData!=null&&inventoryItemsSpawnData.dequeued){
               inventoryItemsSpawnData.at.Add((Vector3.zero,Vector3.zero,Vector3.one,typeof(RemingtonModel700BDL),null,new PersistentData()));
               inventoryItemsSpawnData.asInventoryItemOwnerIds[inventoryItemsSpawnData.at.Count-1]=id.Value;
               inventoryItemsSpawnData.dequeued=false;
               SimObjectSpawner.singleton.OnSpecificSpawnRequestAt(inventoryItemsSpawnData);
              }
             }
            }else if(DEBUG_TOGGLE_HOLSTER_WEAPON_TYPE==WeaponTypes.None){
             //  TO DO: release items
            }
           }
           if(aiSensor){
            if(canSense){
             if(!aiSensor.isActiveAndEnabled){
              aiSensor.Activate();
             }
             if(aiSensor.isActiveAndEnabled){
              if(rightEye){
               if(leftEye){
                aiSensor.transform.position=(leftEye.transform.position+rightEye.transform.position)/2f;
                aiSensor.transform.rotation=leftEye.transform.rotation;
               }else{
                aiSensor.transform.position=rightEye.transform.position;
                aiSensor.transform.rotation=rightEye.transform.rotation;
               }
              }else if(leftEye){
               aiSensor.transform.position=leftEye.transform.position;
               aiSensor.transform.rotation=leftEye.transform.rotation;
              }else if(head){
               if(aiSensor.zIsUp){
                aiSensor.transform.position=head.transform.position;
                aiSensor.transform.rotation=Quaternion.LookRotation(head.up,head.forward);
               }else{
                aiSensor.transform.position=head.transform.position;
                aiSensor.transform.rotation=Quaternion.Euler(0f,head.transform.eulerAngles.y,0f);
               }
              }
             }
            }
           }
           if(isUsingAI){
            EnableNavMeshAgent();
            if(!navMeshAgent.isOnNavMesh){
             DisableNavMeshAgent();
            }
            if(navMeshAgent.enabled){
             AI();
            }
           }else{
            DisableNavMeshAgent();
            if(simActorCharacterController!=null){
               simActorCharacterController.ManualUpdate();
             transform.position+=simActorCharacterController.moveDelta;
             simActorCharacterController.characterController.transform.position-=simActorCharacterController.moveDelta;
             OnCharacterControllerUpdated();
            }
           }
          }else{
           DisableNavMeshAgent();
          }
         }
         if(transform.hasChanged){
          GetCollidersTouchingNonAlloc(instantCheck:true);
         }
         if(gotCollidersTouchingFromInstantCheck){
          for(int i=0;i<collidersTouchingUpperCount;++i){
           Collider colliderTouchingUpper=collidersTouchingUpper[i];
           if(colliderTouchingUpper.transform.root!=transform.root){//  it's not myself
            shouldCrouch=true;
           }
          }
          for(int i=0;i<collidersTouchingMiddleCount;++i){
           Collider colliderTouchingMiddle=collidersTouchingMiddle[i];
           if(colliderTouchingMiddle.transform.root!=transform.root){//  it's not myself
            shouldCrouch=true;
           }
          }
         }else{
          if(simCollisionsTouchingUpper !=null){
           foreach(Collider colliderTouchingUpper  in simCollisionsTouchingUpper .simObjectColliders){
            shouldCrouch=true;
           }
          }
          if(simCollisionsTouchingMiddle!=null){
           foreach(Collider colliderTouchingMiddle in simCollisionsTouchingMiddle.simObjectColliders){
            shouldCrouch=true;
           }
          }
         }
         if(Core.singleton.isServer){
          if(IsOwner){
           if(shouldCrouch){
            if(wasCrouchingBeforeShouldCrouch==null){
               wasCrouchingBeforeShouldCrouch=crouching;
            }
            if(!crouching){
             OnToggleCrouching();
            }
           }else{
            if(wasCrouchingBeforeShouldCrouch!=null){
             if(!wasCrouchingBeforeShouldCrouch.Value){
              if(crouching){
               OnToggleCrouching();
              }
             }else{
              if(!crouching){
               OnToggleCrouching();
              }
             }
               wasCrouchingBeforeShouldCrouch=null;
            }
            if(DEBUG_TOGGLE_CROUCHING){
               DEBUG_TOGGLE_CROUCHING=false;
             OnToggleCrouching();
            }
           }
          }
         }
         if(simActorAnimatorController!=null){
            simActorAnimatorController.ManualUpdate();
         }
         lastForward=transform.forward;
         return result;
        }
        protected virtual void AI(){
        }
        protected virtual void OnCharacterControllerUpdated(){
        }
        internal Vector3 GetHeadPosition(bool fromAnimator){
         Vector3 headPos;
         if(fromAnimator&&simActorAnimatorController!=null&&simActorAnimatorController.animator!=null){
          headPos=simActorAnimatorController.animator.transform.position+simActorAnimatorController.animator.transform.rotation*(new Vector3(0f,simActorCharacterController.characterController.height/2f+simActorCharacterController.characterController.radius,0f)+simActorCharacterController.headOffset);
         }else{
          headPos=simActorCharacterController.characterController.transform.position+simActorCharacterController.characterController.transform.rotation*simActorCharacterController.headOffset;
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
         if(simActorCharacterController!=null){
          gotCollidersTouchingFromInstantCheck=true;
          var upperMiddleLowerValues=simCollisions.GetCapsuleValuesForUpperMiddleLowerCollisionTesting(simActorCharacterController.characterController,transform.root,height,simActorCharacterController.center);
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
    }
}