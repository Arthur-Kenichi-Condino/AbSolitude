#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Actors.Humanoid.Human.ArthurCondino;
using AKCondinoO.Sims.Actors.Skills.SkillVisualEffects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace AKCondinoO.Sims.Actors.Skills{
    internal partial class SkillAoE:MonoBehaviour{
     internal LinkedListNode<SkillAoE>pooled=null;
     internal Skill skill;
     [SerializeField]internal int skillVFXsToSpawn=1;
     internal readonly Dictionary<Type,List<SkillVisualEffect>>skillVFXs=new Dictionary<Type,List<SkillVisualEffect>>();
        internal virtual void OnSpawned(){
        }
        internal virtual void OnPool(){
         foreach(var skillVFXList in skillVFXs){
          foreach(var skillVFX in skillVFXList.Value){
           skillVFX.aoe=null;
          }
          skillVFXList.Value.Clear();
         }
         if(skill!=null){
          if(skill is AreaSkill areaSkill){
           areaSkill.activeAoE.Remove(this);
          }
          skill=null;
         }
         this.target=null;
         this.targetPos=null;
        }
     internal Vector3?targetPos;
     internal SimObject target;
     internal float delay;
      internal float timer;
     internal int loops;
      internal int loopCount=-1;
     internal float duration;
     internal bool active;
        internal virtual void ActivateAt(Vector3 targetPos,SimObject target,float delay,int loops,float duration=0f){
         Log.DebugMessage("SkillAoE:ActivateAt",this);
         this.targetPos=targetPos;
         this.target=target;
         this.delay=delay;
         timer=0f;
         this.loops=loops;
         loopCount=-1;
         if(duration>0f){
          this.duration=delay+duration;
         }
         this.active=true;
         enabled=true;
        }
        internal virtual void OnDeactivate(){
         Log.DebugMessage("SkillAoE:OnDeactivate:timer:"+timer+";duration:"+duration,this);
         this.active=false;
         enabled=false;
         SkillAoEManager.singleton.Pool(this.GetType(),this);
        }
        protected virtual void Update(){
         if(!this.active){
          OnDeactivate();
          return;
         }
         if(timer>=delay){
          if(targetPos!=null){
           transform.position=targetPos.Value;
          }else if(target!=null){
           transform.position=target.transform.position;
          }
          if(duration>0f){
           if(timer>=duration){
            OnDeactivate();
            return;
           }else{
            if(skillVFXs.All(kvp=>{return kvp.Value.Count<skillVFXsToSpawn;})){
             //
             SpawnSkillVFXs(ValidateSkillVFXsDuration(duration-timer));
            }
           }
          }else{
           if(skillVFXs.All(kvp=>{return kvp.Value.Count<=0;})){
            loopCount++;
            if(loopCount>=loops){
             OnDeactivate();
             return;
            }else{
             //
             SpawnSkillVFXs(ValidateSkillVFXsDuration(-1f));
            }
           }
          }
         }
         timer+=Time.deltaTime;
        }
        protected virtual void SpawnSkillVFXs(float skillVFXsDuration){
        }
        protected virtual float ValidateSkillVFXsDuration(float skillVFXsDuration){
         float duration=skillVFXsDuration;
         if(duration<0f){
          duration=0f;
         }
         return duration;
        }
     internal Vector3 size;
        protected virtual void OnTriggerEnter(Collider other){
         Log.DebugMessage("SkillAoE:OnTriggerEnter:other:"+other,other);
        }
    }
}