#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Actors.Skills;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.AI;
using static AKCondinoO.Sims.Actors.SimActor.PersistentSimActorData;
using static AKCondinoO.Voxels.VoxelSystem;
namespace AKCondinoO.Sims.Actors{
    internal class SimActor:SimObject{
     internal PersistentSimActorData persistentSimActorData;
        //  [https://stackoverflow.com/questions/945664/can-structs-contain-fields-of-reference-types]
        internal struct PersistentSimActorData{
         public ListWrapper<SkillData>skills;
            public struct SkillData{
             public Type skill;public int level;
            }
         public ListWrapper<SlaveData>slaves;
            public struct SlaveData{
             public Type simType;public ulong number;
            }
         public float timerToRandomMove;
            internal void UpdateData(SimActor simActor){
             skills=new ListWrapper<SkillData>(simActor.skills.Select(kvp=>{return new SkillData{skill=kvp.Key,level=kvp.Value.level};}).ToList());
             slaves=new ListWrapper<SlaveData>(simActor.slaves.Select(v  =>{return new SlaveData{simType=v.simType,number=v.number  };}).ToList());
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
              stringBuilder.AppendFormat(CultureInfoUtil.en_US,"[{0},{1}],",skill.skill,skill.level);
             }
             stringBuilder.AppendFormat(CultureInfoUtil.en_US," }} , ");
             stringBuilder.AppendFormat(CultureInfoUtil.en_US,"slaves={{ ");
             slaves.Reset();
             while(slaves.MoveNext()){
              SlaveData slave=slaves.Current;
              stringBuilder.AppendFormat(CultureInfoUtil.en_US,"[{0},{1}],",slave.simType,slave.number);
             }
             stringBuilder.AppendFormat(CultureInfoUtil.en_US," }} , ");
             string result=string.Format(CultureInfoUtil.en_US,"persistentSimActorData={{ {0}, }}",stringBuilder.ToString());
             stringBuilderPool.Enqueue(stringBuilder);
             return result;
            }
        }
     internal NavMeshAgent navMeshAgent;
      internal NavMeshQueryFilter navMeshQueryFilter;
       [SerializeField]protected float navMeshAgentWalkSpeed=2f;
        [SerializeField]protected float navMeshAgentRunSpeed=4f;
         protected bool navMeshAgentShouldUseRunSpeed=false;
     internal SimActorCharacterController simActorCharacterController;
      internal float height;
       internal float heightCrouching;
     internal SimActorAnimatorController simActorAnimatorController;
        protected override void Awake(){
         base.Awake();
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
        internal override void OnLoadingPool(){
         base.OnLoadingPool();
        }
     internal readonly Dictionary<Type,SkillData>requiredSkills=new Dictionary<Type,SkillData>();
      internal readonly Dictionary<Type,Skill>skills=new Dictionary<Type,Skill>();
     internal readonly Dictionary<Type,List<SlaveData>>requiredSlaves=new Dictionary<Type,List<SlaveData>>();
      internal readonly List<(Type simType,ulong number)>slaves=new List<(Type,ulong)>();
        internal override void OnActivated(){
         base.OnActivated();
         lastForward=transform.forward;
         skills.Clear();
         //  load skills from file here
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
          GameObject skillGameObject=Instantiate(SkillsManager.singleton.skillPrefabs[requiredSkill.Key]);
          Skill skill=skillGameObject.GetComponent<Skill>();
          skill.level=requiredSkill.Value.level;
          skills.Add(skill.GetType(),skill);
         }
         requiredSkills.Clear();
         if(this is BaseAI actorAI){
          foreach(var skill in skills){
           skill.Value.actor=actorAI;
          }
         }
         slaves.Clear();
         //  load slaves from file here
         foreach(var slave in slaves){
          if(requiredSlaves.TryGetValue(slave.simType,out List<SlaveData>requiredSlavesForType)){
           //  TO DO: do some checks and set variables here
          }
         }
         persistentSimActorData.UpdateData(this);
        }
        protected override void EnableInteractions(){
         interactionsEnabled=true;
        }
        protected override void DisableInteractions(){
         interactionsEnabled=false;
        }
        void EnableNavMeshAgent(){
         if(!navMeshAgent.enabled){
          if(NavMesh.SamplePosition(transform.position,out NavMeshHit hitResult,Height,navMeshQueryFilter)){
           transform.position=hitResult.position+Vector3.up*navMeshAgent.height/2f;
           navMeshAgent.enabled=true;
           Log.DebugMessage("navMeshAgent is enabled");
          }
         }
        }
        void DisableNavMeshAgent(){
         navMeshAgent.enabled=false;
        }
     internal bool isUsingAI=true;
     internal virtual float moveVelocity{
      get{
       if(isUsingAI){
        float velocityMagnitude=navMeshAgent.velocity.magnitude;
        //Log.DebugMessage("navMeshAgent velocityMagnitude:"+velocityMagnitude);
        return velocityMagnitude/navMeshAgentRunSpeed;
       }
       return 0f;
      }
     }
     protected Vector3 lastForward=Vector3.forward;
     internal float turnAngle{
      get{
       if(isUsingAI){
        if(!Mathf.Approximately(navMeshAgent.velocity.magnitude,0f)){
         float angle=Vector3.SignedAngle(transform.forward,navMeshAgent.velocity.normalized,transform.up);
         //Log.DebugMessage("angle:"+angle);
         return angle;
        }
       }
       return 0f;
      }
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
           simActorCharacterController.characterController.height=heightCrouching;
           simActorCharacterController.characterController.center=new Vector3(0,-((height/2f)-(heightCrouching/2f)),0);
          }else{
           crouching_v=false;
           simActorCharacterController.characterController.height=height;
           simActorCharacterController.characterController.center=new Vector3(0,0,0);
          }
         }
        }
     [SerializeField]bool DEBUG_TOGGLE_CROUCHING=false;
     bool?wasCrouchingBeforeShouldCrouch;
        internal override int ManualUpdate(bool doValidationChecks){
         int result=0;
         if((result=base.ManualUpdate(doValidationChecks))!=0){
          DisableNavMeshAgent();
          return result;
         }
         bool shouldCrouch=false;//  is crouching required?
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
          }
         }
         if(transform.hasChanged){
          GetCollidersTouchingNonAlloc();
         }
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
         lastForward=transform.forward;
         return result;
        }
        protected virtual void AI(){
        }
     protected Collider[]collidersTouchingUpper=new Collider[8];
      protected int collidersTouchingUpperCount=0;
     protected Collider[]collidersTouchingMiddle=new Collider[8];
      protected int collidersTouchingMiddleCount=0;
        protected override void GetCollidersTouchingNonAlloc(){
         if(simActorCharacterController!=null){
          var section=height/3f;
          if((section/2f)>simActorCharacterController.characterController.radius){
           var direction=Vector3.up;
           direction=transform.rotation*direction;
           var offset=(section/2f)-simActorCharacterController.characterController.radius;
           var center=simActorCharacterController.center;
           center.y+=(height/2f)-(section/2f);
           var localPoint0=center-direction*offset;
           var localPoint1=center+direction*offset;
           var point0=transform.TransformPoint(localPoint0);
           var point1=transform.TransformPoint(localPoint1);
           _GetUpperColliders:{
            collidersTouchingUpperCount=Physics.OverlapCapsuleNonAlloc(
             point0,
             point1,
             simActorCharacterController.characterController.radius,
             collidersTouchingUpper
            );
           }
           if(collidersTouchingUpperCount>0){
            if(collidersTouchingUpperCount>=collidersTouchingUpper.Length){
             Array.Resize(ref collidersTouchingUpper,collidersTouchingUpperCount*2);
             goto _GetUpperColliders;
            }
           }
           center=simActorCharacterController.center;
           localPoint0=center-direction*offset;
           localPoint1=center+direction*offset;
           point0=transform.TransformPoint(localPoint0);
           point1=transform.TransformPoint(localPoint1);
           _GetMiddleColliders:{
            collidersTouchingMiddleCount=Physics.OverlapCapsuleNonAlloc(
             point0,
             point1,
             simActorCharacterController.characterController.radius,
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