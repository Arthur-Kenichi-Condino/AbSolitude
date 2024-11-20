#if DEVELOPMENT_BUILD
    #define ENABLE_LOG_DEBUG
#else
    #if UNITY_EDITOR
        #define ENABLE_LOG_DEBUG
    #endif
#endif
using AKCondinoO.Sims.Actors.Combat;
using AKCondinoO.Sims.Actors.Homunculi.Vanilmirth;
using AKCondinoO.Sims.Actors.Pathfinding;
using AKCondinoO.Sims.Actors.Skills;
using AKCondinoO.Sims.Actors.Skills.SkillBuffs;
using AKCondinoO.Sims.Inventory;
using AKCondinoO.Sims.Weapons;
using AKCondinoO.Sims.Weapons.Rifle.SniperRifle;
using System;
using System.Collections;
using System.Collections.Generic;
using UMA;
using UMA.CharacterSystem;
using Unity.Collections;
using UnityEngine;
using UnityEngine.AI;
using static AKCondinoO.GameMode;
using static AKCondinoO.InputHandler;
using static AKCondinoO.Sims.Actors.SimActor.PersistentSimActorData;
using static AKCondinoO.Voxels.VoxelSystem;
namespace AKCondinoO.Sims.Actors{
    ///  [https://www.youtube.com/watch?v=t9e2XBQY4Og]
    ///  [https://www.youtube.com/watch?v=FbM4CkqtOuA]
    ///  [https://www.youtube.com/watch?v=znZXmmyBF-o]
    internal partial class BaseAI:SimActor{
     internal AStarPathfindingBackgroundContainer aStarPathfindingBG;
     internal SimCharacterController characterController;
      internal float height;
       internal float heightCrouching;
     internal SimAnimatorController animatorController;
     internal AISensor aiSensor;
     protected AI ai;
        protected override void Awake(){
         if(ai==null){
          ai=new AI(this);
         }
         InitTargets();
         base.Awake();
         aiSensor=GetComponentInChildren<AISensor>();
         if(aiSensor){
          aiSensor.actor=this;
          aiSensor.Deactivate();
          //Log.DebugMessage("aiSensor found, search for actor's \"head\" to add sight");
         }
         navMeshAgent=GetComponent<NavMeshAgent>();
         navMeshQueryFilter=new NavMeshQueryFilter(){
          agentTypeID=navMeshAgent.agentTypeID,
             areaMask=navMeshAgent.areaMask,
         };
         characterController=GetComponent<SimCharacterController>();
         if(characterController!=null){
            characterController.actor=this;
          height=characterController.character.height;
         }
         heightCrouching=navMeshAgent.height;
         if(characterController==null){
          height=heightCrouching;
         }
         aiRotTurnTo.tgtRot=aiRotTurnTo.tgtRot_Last=transform.rotation;
         //Log.DebugMessage("height:"+height+";heightCrouching:"+heightCrouching);
         animatorController=GetComponent<SimAnimatorController>();
         animatorController.actor=this;
         aStarPathfindingBG=new AStarPathfindingBackgroundContainer(aStarPathfindingWidth,aStarPathfindingDepth,aStarPathfindingHeight);
         aStarPathfindingBG.GetGroundRays=new NativeList<RaycastCommand>(aStarPathfindingWidth*aStarPathfindingDepth*aStarPathfindingHeight,Allocator.Persistent);
         aStarPathfindingBG.GetGroundHits=new NativeList<RaycastHit    >(aStarPathfindingWidth*aStarPathfindingDepth*aStarPathfindingHeight,Allocator.Persistent);
         aStarPathfindingBG.GetObstaclesCommands=new NativeList<OverlapBoxCommand>(aStarPathfindingWidth*aStarPathfindingDepth*aStarPathfindingHeight                                       ,Allocator.Persistent);
         aStarPathfindingBG.GetObstaclesOverlaps=new NativeList<ColliderHit      >(aStarPathfindingWidth*aStarPathfindingDepth*aStarPathfindingHeight*aStarPathfindingBG.getObstaclesMaxHits,Allocator.Persistent);
         AStarPathfinding.singleton.aStarPathfindingContainers.Add((this,aStarPathfindingBG));
         nativeToManagedCoroutine=StartCoroutine(NativeToManagedCoroutine());
        }
     protected bool canSense;
        protected override void OnUMACharacterUpdated(UMAData simUMAData){
         if(aiSensor){
          if(head||leftEye||rightEye){
           //Log.DebugMessage("aiSensor found, sync with actor's \"head's\" and/or \"eyes'\" transforms for providing eyesight to AI");
           canSense=true;
          }
         }
         OnCreateHitboxes(simUMA,simUMAData);
         OnCreateHurtboxes(simUMA,simUMAData);
         base.OnUMACharacterUpdated(simUMAData);
        }
     internal readonly Dictionary<Type,SkillData>requiredSkills=new Dictionary<Type,SkillData>();
      internal readonly Dictionary<Type,Skill>skills=new Dictionary<Type,Skill>();
       internal readonly HashSet<Skill>passiveSkills=new HashSet<Skill>();
        protected bool isAllPassiveSkillsInEffectFlag;
     internal readonly Dictionary<Type,List<SlaveData>>requiredSlaves=new Dictionary<Type,List<SlaveData>>();
      internal readonly HashSet<(Type simObjectType,ulong idNumber)>slaves=new HashSet<(Type,ulong)>();
        internal override void OnActivated(){
         base.OnActivated();
         attackRange=new Vector3(0.125f/8f,0.125f/8f,0.0625f/8f);
         requiredSkills.Add(typeof(OnHitGracePeriod),new SkillData(){skill=typeof(OnHitGracePeriod),level=10,});
         requiredSkills.Add(typeof(Teleport        ),new SkillData(){skill=typeof(Teleport        ),level=1 ,});
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
          if(spawnedSkill.skill is PassiveSkill passiveSkill){
           passiveSkills.Add(passiveSkill);
          }
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
           if(spawnedSkill.skill is PassiveSkill passiveSkill){
            passiveSkills.Add(passiveSkill);
           }
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
         if(ai.attackSt.getDataCoroutine!=null){
          StopCoroutine(ai.attackSt.getDataCoroutine);ai.attackSt.getDataCoroutine=null;
         }
         ai.attackSt.getDataCoroutine=StartCoroutine(ai.attackSt.GetDataCoroutine());
         if(ai.chaseSt.getDataCoroutine!=null){
          StopCoroutine(ai.chaseSt.getDataCoroutine);ai.chaseSt.getDataCoroutine=null;
         }
         ai.chaseSt.getDataCoroutine=StartCoroutine(ai.chaseSt.GetDataCoroutine());
         if(animatorController!=null){
          if(animatorController.animationEventsHandler!=null){
           animatorController.animationEventsHandler.CancelAllEvents();
          }
         }
         isAllPassiveSkillsInEffectFlag=false;
        }
        internal override void OnDeactivated(){
         if(characterController!=null){
          characterController.weaponsReloading.Clear();
         }
         if(ai.attackSt.getDataCoroutine!=null){
          StopCoroutine(ai.attackSt.getDataCoroutine);ai.attackSt.getDataCoroutine=null;
         }
         if(ai.chaseSt.getDataCoroutine!=null){
          StopCoroutine(ai.chaseSt.getDataCoroutine);ai.chaseSt.getDataCoroutine=null;
         }
         if(aiSensor){
          aiSensor.Deactivate();
         }
         Log.DebugMessage("sim actor:OnDeactivated:id:"+id);
         foreach(var skill in skills){
          SkillsManager.singleton.Pool(skill.Key,skill.Value);
         }
         skills.Clear();//  to do: pool skills before clearing the list
         passiveSkills.Clear();
         base.OnDeactivated();
         ReleaseTargets();
        }
        protected override void SetSlave(SimObject slave){
         slaves.Add(slave.id.Value);
         base.SetSlave(slave);
         persistentSimActorData.UpdateData(this);
        }
     internal bool isUsingAI=true;
      internal bool wasUsingAI=true;
     protected Vector3 lastForward=Vector3.forward;
     [SerializeField]bool DEBUG_ACTIVATE_THIRD_PERSON_CAM_TO_FOLLOW_THIS=false;
     [SerializeField]bool DEBUG_TOGGLE_CROUCHING=false;
     [SerializeField]bool        DEBUG_TOGGLE_HOLSTER_WEAPON=false;
     [SerializeField]WeaponTypes DEBUG_TOGGLE_HOLSTER_WEAPON_TYPE=WeaponTypes.SniperRifle;
     [SerializeField]float AFKTimeToUseAI=5f;
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
           ManualUpdateAStarPathfinding();
           if(DEBUG_ACTIVATE_THIRD_PERSON_CAM_TO_FOLLOW_THIS){
              DEBUG_ACTIVATE_THIRD_PERSON_CAM_TO_FOLLOW_THIS=false;
            OnThirdPersonCamFollow();
           }
           wasUsingAI=isUsingAI;
           if(MainCamera.singleton.toFollowActor==this){
            //Log.DebugMessage("following this:"+this);
            if(InputHandler.singleton.activityDetected&&!Enabled.RELEASE_MOUSE.curState){
             isUsingAI=false;
             AFKTimerToUseAI=AFKTimeToUseAI;
             //Log.DebugMessage("start using manual control:"+this);
            }
           }else{
            if(!isUsingAI){
             OnStartUsingAI();
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
             OnStartUsingAI();
             isUsingAI=true;
             //Log.DebugMessage("'AFK for too long, use AI':"+this);
            }
           }
           if(DEBUG_TOGGLE_HOLSTER_WEAPON){
              DEBUG_TOGGLE_HOLSTER_WEAPON=false;
            if(DEBUG_TOGGLE_HOLSTER_WEAPON_TYPE==WeaponTypes.SniperRifle){
             if(SimObjectSpawner.singleton.simInventoryItemsInContainerSettings.allSettings.TryGetValue(typeof(RemingtonModel700BDL),out SimInventoryItemsInContainerSettings.InContainerSettings simInventoryItemSettings)){
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
               if(aiSensor.zIsUp){
                aiSensor.transform.rotation*=(Quaternion.Euler(90f,0f,0f)*Quaternion.Euler(0f,180f,0f));
               }
              }else if(leftEye){
               aiSensor.transform.position=leftEye.transform.position;
               aiSensor.transform.rotation=leftEye.transform.rotation;
               if(aiSensor.zIsUp){
                aiSensor.transform.rotation*=(Quaternion.Euler(90f,0f,0f)*Quaternion.Euler(0f,180f,0f));
               }
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
           if(!isAllPassiveSkillsInEffectFlag){
            isAllPassiveSkillsInEffectFlag=true;
            foreach(PassiveSkill passiveSkill in passiveSkills){
             if(!passiveSkill.DoSkillImmediate(this,passiveSkill.level)&&!passiveSkill.doing){
              isAllPassiveSkillsInEffectFlag=false;
             }
            }
           }
           UpdateHitboxesGracePeriod();
           UpdateWeaponsGracePeriod();
           if(isUsingAI){
            EnableNavMeshAgent();
            if(!navMeshAgent.isOnNavMesh){
             DisableNavMeshAgent();
            }
            if(navMeshAgent.enabled){
             if(characterController!=null){
                characterController.ManualUpdateUsingAI();
             }
             ai.MyPathfinding=GetPathfindingResult();
             ai.Main();
             characterController.character.transform.rotation=aiRotTurnTo.UpdateRotation(characterController.character.transform.rotation,Core.magicDeltaTimeNumber);
             if(movePauseDelay>0f){
              movePauseDelay-=Time.deltaTime;
             }
             if(
              IsTraversingPath()
             ){
              if(
               IsAttacking()
              ){
               MoveStop();
               if(ai.MyEnemy!=null){
                TurnToMyEnemy();
               }
              }else{
               if(!TurnToMoveDest()){
                if(movePauseDelay<=0f){
                 MovePause();
                }
               }else{
                MoveResume();
               }
              }
             }
             UpdateMotion(true);
             bool stopNavMesh=navMeshAgentShouldBeStopped;
             stopNavMesh|=movePaused;
             if(navMeshAgent.isStopped!=stopNavMesh){
              //Log.DebugMessage("navMeshAgentShouldBeStopped:"+navMeshAgentShouldBeStopped);
              navMeshAgent.isStopped=stopNavMesh;
             }
            }
           }else{
            if(wasUsingAI){
             OnStopUsingAI();
            }
            DisableNavMeshAgent();
            if(characterController!=null){
               characterController.ManualUpdate();
             transform.position+=characterController.moveDelta;
             characterController.character.transform.position-=characterController.moveDelta;
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
         UpdateGetters();
         //Log.DebugMessage("animatorController:"+animatorController);
         if(animatorController!=null){
            animatorController.ManualUpdate();
         }
         lastForward=transform.forward;
         teleportedMove=false;
         return result;
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
     internal virtual Vector3 moveMaxVelocity{
      get{//  TO DO: add flying or swimming speed
       if(isUsingAI){
        return new Vector3(navMeshAgent.speed,navMeshAgent.speed,navMeshAgent.speed);
       }else if(characterController!=null){
        return characterController.maxMoveSpeed;
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
        protected virtual void OnCharacterControllerUpdated(){
         UpdateMotion(false);
        }
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
     [SerializeField]internal bool canBeThirdPersonCamFollowed=false;
        internal void OnThirdPersonCamFollow(){
         Log.DebugMessage("OnThirdPersonCamFollow()",this);
         if(!canBeThirdPersonCamFollowed){
          Log.DebugMessage("!canBeThirdPersonCamFollowed",this);
          return;
         }
         MainCamera.singleton.toFollowActor=this;
         GameMode.singleton.OnGameModeChangeTo(GameModesEnum.ThirdPerson);
        }
        internal Vector3 GetHeadPosition(bool fromAnimator,bool forShooting=false){
         Vector3 headPos;
         if(head){
          headPos=head.position;
         }else{
          if(fromAnimator&&animatorController!=null&&animatorController.animator!=null){
           headPos=animatorController.animator.transform.position+animatorController.animator.transform.rotation*(new Vector3(0f,characterController.character.height/2f+characterController.character.radius,0f)+characterController.headOffset);
          }else{
           headPos=characterController.character.transform.position+characterController.character.transform.rotation*characterController.headOffset;
          }
          if(forShooting){
           headPos-=new Vector3(0f,.125f,0f);
          }
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
    }
}