#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Actors.Humanoid.Human.ArthurCondino;
using AKCondinoO.Sims.Actors.Skills.SkillVisualEffects;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Actors.Skills{
    internal partial class SkillAoE:MonoBehaviour{
     internal LinkedListNode<SkillAoE>pooled=null;
     internal Skill skill;
     [SerializeField]internal int skillVFXsToSpawn=1;
     internal readonly List<SkillVisualEffect>skillVFXs=new List<SkillVisualEffect>();
        internal virtual void OnSpawned(){
        }
        internal virtual void OnPool(){
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
         this.targetPos=targetPos;
         this.target=target;
         this.delay=delay;
         timer=0f;
         this.loops=loops;
         loopCount=-1;
         this.duration=delay+duration;
         this.active=true;
         enabled=true;
        }
        internal virtual void OnDeactivate(){
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
          }else{
           transform.position=target.transform.position;
          }
          if(duration>0f){
           if(timer>=duration){
            OnDeactivate();
            return;
           }else{
            if(!GetComponent<ParticleSystem>().Any(particleSys=>particleSys.isPlaying)){
             particleSystemParent.Play(true);
             playSFX=true;
            }
           }
          }else if(!GetComponent<ParticleSystem>().Any(particleSys=>particleSys.isPlaying)&&!audioSources.Any(audioSource=>audioSource.isPlaying)){
           loopCount++;
           if(loopCount>=loops){
            OnDeactivate();
            return;
           }else{
            particleSystemParent.Play(true);
            playSFX=true;
           }
          }
         }
         timer+=Time.deltaTime;
        }
    }
}