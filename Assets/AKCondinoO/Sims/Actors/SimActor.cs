#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Actors.Skills;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static AKCondinoO.Voxels.VoxelSystem;
namespace AKCondinoO.Sims.Actors{
    internal class SimActor:SimObject{
     internal PersistentSimActorData persistentSimActorData;
        //  [https://stackoverflow.com/questions/945664/can-structs-contain-fields-of-reference-types]
        internal struct PersistentSimActorData{
         public SkillsListWrapper skills;
            internal struct SkillsListWrapper:IEnumerator<(Type skill,int level)>{
             private List<(Type skill,int level)>.Enumerator m_Enumerator;
                public SkillsListWrapper(List<(Type skill,int level)>list){
                 m_Enumerator=list.GetEnumerator();
                }
                public(Type skill,int level)Current=>m_Enumerator.Current;
                object IEnumerator.Current=>Current;
                public bool MoveNext()=>m_Enumerator.MoveNext();
                public void Reset()=>((IEnumerator)m_Enumerator).Reset();
                public void Dispose()=>m_Enumerator.Dispose();
            }
         public SlavesListWrapper slaves;
            internal struct SlavesListWrapper:IEnumerator<(Type simType,ulong number)>{
             private List<(Type simType,ulong number)>.Enumerator m_Enumerator;
                public SlavesListWrapper(List<(Type simType,ulong number)>list){
                 m_Enumerator=list.GetEnumerator();
                }
                public(Type simType,ulong number)Current=>m_Enumerator.Current;
                object IEnumerator.Current=>Current;
                public bool MoveNext()=>m_Enumerator.MoveNext();
                public void Reset()=>((IEnumerator)m_Enumerator).Reset();
                public void Dispose()=>m_Enumerator.Dispose();
            }
         public float timerToRandomMove;
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
     protected readonly Dictionary<Type,int>requiredSkills=new Dictionary<Type,int>();
     internal readonly Dictionary<Type,Skill>skills=new Dictionary<Type,Skill>();
     internal readonly List<(Type simType,ulong number)>slaves=new List<(Type,ulong)>();
        internal override void OnActivated(){
         base.OnActivated();
         lastForward=transform.forward;
         skills.Clear();
         //  load skills from file here
         foreach(var skill in skills){
          //  TO DO: test skill level
          requiredSkills.Remove(skill.Key);
         }
         if(requiredSkills.Count>0){
          Log.DebugMessage("required skills missing");
         }
         foreach(var requiredSkill in requiredSkills){
          GameObject skillGameObject=Instantiate(SkillsManager.singleton.skillPrefabs[requiredSkill.Key]);
          Skill skill=skillGameObject.GetComponent<Skill>();
          skill.level=requiredSkill.Value;
          skills.Add(skill.GetType(),skill);
         }
         requiredSkills.Clear();
         slaves.Clear();
         //  load slaves from file here
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
     bool wasCrouchingOnShouldCrouch;
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
         if(shouldCrouch){
          if(!crouching){
           OnToggleCrouching();
          }
         }else{
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
        protected override void GetCollidersTouchingNonAlloc(){
         if(simActorCharacterController!=null){
          var section=height/3f;
          if((section/2f)>simActorCharacterController.characterController.radius){
           var direction=Vector3.up;
           direction=transform.rotation*direction;
           var offset=(section/2f)-simActorCharacterController.characterController.radius;
           var center=simActorCharacterController.characterController.center;
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
          }
         }
        }
    }
}