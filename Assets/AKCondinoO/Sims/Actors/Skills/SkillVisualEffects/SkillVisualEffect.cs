#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace AKCondinoO.Sims.Actors.Skills.SkillVisualEffects{
    internal class SkillVisualEffect:MonoBehaviour{
     internal LinkedListNode<SkillVisualEffect>pooled=null;
     internal ParticleSystem particleSystemParent;
     internal new ParticleSystem[]particleSystem;
     internal readonly Dictionary<ParticleSystem,float>totalDuration=new Dictionary<ParticleSystem,float>();
        void Awake(){
         Log.DebugMessage("SkillVisualEffect Awake, Type:"+this.GetType());
         particleSystemParent=GetComponent<ParticleSystem>();
         particleSystem=GetComponentsInChildren<ParticleSystem>();
         foreach(ParticleSystem particleSys in particleSystem){
          Log.DebugMessage("particleSys:"+particleSys.name);
          totalDuration.Add(particleSys,particleSys.main.duration);
         }
        }
        internal virtual void OnSpawned(){
        }
        internal virtual void OnPool(){
         this.target=null;
        }
     internal SimObject target;
     internal float delay;
      internal float timer;
     internal int loops;
      internal int loopCount=-1;
     internal float duration;
     internal bool active;
        internal virtual void Activate(SimObject target,float delay,int loops,float duration=0f){
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
         particleSystemParent.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
         this.active=false;
         enabled=false;
         SkillVisualEffectsManager.singleton.Pool(this.GetType(),this);
        }
        protected virtual void Update(){
         if(!this.active){
          OnDeactivate();
          return;
         }
         if(timer>=delay){
          transform.position=target.transform.position;
          if(duration>0f){
           if(timer>=duration){
            OnDeactivate();
            return;
           }else{
            if(!particleSystem.Any(particleSys=>particleSys.isPlaying)){
             particleSystemParent.Play(true);
            }
           }
          }else if(!particleSystem.Any(particleSys=>particleSys.isPlaying)){
           loopCount++;
           if(loopCount>=loops){
            OnDeactivate();
            return;
           }else{
            particleSystemParent.Play(true);
           }
          }
         }
         timer+=Time.deltaTime;
        }
    }
}