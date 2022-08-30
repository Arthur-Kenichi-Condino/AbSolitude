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
     internal SimActorAnimatorController simActorAnimatorController;
        protected override void Awake(){
         base.Awake();
         navMeshAgent=GetComponent<NavMeshAgent>();
         navMeshQueryFilter=new NavMeshQueryFilter(){
          agentTypeID=navMeshAgent.agentTypeID,
             areaMask=navMeshAgent.areaMask,
         };
         simActorAnimatorController=GetComponent<SimActorAnimatorController>();
        }
        internal override void OnLoadingPool(){
         base.OnLoadingPool();
        }
     protected readonly Dictionary<Type,int>requiredSkills=new Dictionary<Type,int>();
     internal readonly Dictionary<Type,Skill>skills=new Dictionary<Type,Skill>();
     internal readonly List<(Type simType,ulong number)>slaves=new List<(Type,ulong)>();
        internal override void OnActivated(){
         base.OnActivated();
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
        internal override int ManualUpdate(bool doValidationChecks){
         int result=0;
         if((result=base.ManualUpdate(doValidationChecks))!=0){
          DisableNavMeshAgent();
          return result;
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
         }
         return result;
        }
        protected virtual void AI(){
        }
    }
}