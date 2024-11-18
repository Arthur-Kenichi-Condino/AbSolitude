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
     internal AudioSource[]audioSources;
     internal readonly Dictionary<ParticleSystem,(float duration,float startLifetimeConstantMin,float startLifetimeConstant,float startLifetimeConstantMax)>totalDuration=new Dictionary<ParticleSystem,(float,float,float,float)>();
     ParticleSystem highestDurationParticleSystem;
     internal SkillAoE aoe;
        void Awake(){
         //Log.DebugMessage("SkillVisualEffect Awake, Type:"+this.GetType());
         particleSystemParent=GetComponent<ParticleSystem>();
         particleSystem=GetComponentsInChildren<ParticleSystem>();
         float maxDuration=0f;
         foreach(ParticleSystem particleSys in particleSystem){
          //Log.DebugMessage("particleSys:"+particleSys.name);
          var main=particleSys.main;
          totalDuration.Add(particleSys,(main.duration,main.startLifetime.constantMin,main.startLifetime.constant,main.startLifetime.constantMax));
          main.loop=false;
          if(maxDuration<=(maxDuration=Mathf.Max(maxDuration,main.duration+main.startLifetime.constantMax))){
           highestDurationParticleSystem=particleSys;
          }
         }
         audioSources=GetComponentsInChildren<AudioSource>();
         waitForParticleSystemParentPlay=new WaitUntil(()=>{return playSFX;});
         fadeTimeInterval=new WaitForSeconds(0.005f);
         StartCoroutine(PlaySkillSFX());
        }
        internal virtual void OnSpawned(){
        }
        internal virtual void OnPool(){
         if(aoe!=null){
          if(aoe.skillVFXs.TryGetValue(this.GetType(),out List<SkillVisualEffect>skillVFXsOfThisType)){
           skillVFXsOfThisType.Remove(this);
          }
          aoe=null;
         }
         this.target=null;
         this.targetPos=null;
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
         if(duration>0f){
          this.duration=delay+duration;
         }else{
          this.duration=0f;
         }
         this.active=true;
         enabled=true;
        }
     internal Vector3?targetPos;
        internal virtual void ActivateAt(Vector3 targetPos,SimObject target,float delay,int loops,float duration=0f){
         this.targetPos=targetPos;
         Activate(target,delay,loops,duration);
        }
        protected void SetParticleSysDuration(){
         float duration=(this.duration-timer);
         float maxDuration=totalDuration[highestDurationParticleSystem].duration;
         float durationProportion=duration/maxDuration;
         //Log.DebugMessage("SkillVisualEffect:SetParticleSysDuration:duration:"+duration+";maxDuration:"+maxDuration+";durationProportion:"+durationProportion,this);
         foreach(ParticleSystem particleSys in particleSystem){
          var main=particleSys.main;
          if(duration>0f){
           float particleSysDuration=totalDuration[particleSys].duration;
           float particleSysStartLifetimeConstantMax=totalDuration[particleSys].startLifetimeConstantMax;
           if(particleSysDuration+particleSysStartLifetimeConstantMax>duration){
            SetDuration(duration/(particleSysDuration+particleSysStartLifetimeConstantMax));
           }else{
            SetDuration(durationProportion);
           }
          }else{
           SetDuration(1f);
          }
          void SetDuration(float proportion){
           if(proportion<1f){
            if(totalDuration[particleSys].duration*proportion-totalDuration[particleSys].startLifetimeConstantMax<=0f){
             main.duration=totalDuration[particleSys].duration*proportion;
             var startLifetime=main.startLifetime;
             startLifetime.constantMin=totalDuration[particleSys].startLifetimeConstantMin*proportion;
             startLifetime.constant   =totalDuration[particleSys].startLifetimeConstant   *proportion;
             startLifetime.constantMax=totalDuration[particleSys].startLifetimeConstantMax*proportion;
             main.startLifetime=startLifetime;
            }else{
             main.duration=totalDuration[particleSys].duration*proportion-totalDuration[particleSys].startLifetimeConstantMax;
             var startLifetime=main.startLifetime;
             startLifetime.constantMin=totalDuration[particleSys].startLifetimeConstantMin;
             startLifetime.constant   =totalDuration[particleSys].startLifetimeConstant   ;
             startLifetime.constantMax=totalDuration[particleSys].startLifetimeConstantMax;
             main.startLifetime=startLifetime;
            }
           }else{
            if(totalDuration[particleSys].startLifetimeConstantMax>=totalDuration[particleSys].duration*proportion){
             main.duration=totalDuration[particleSys].duration*proportion;
             var startLifetime=main.startLifetime;
             startLifetime.constantMin=totalDuration[particleSys].startLifetimeConstantMin*proportion;
             startLifetime.constant   =totalDuration[particleSys].startLifetimeConstant   *proportion;
             startLifetime.constantMax=totalDuration[particleSys].startLifetimeConstantMax*proportion;
             main.startLifetime=startLifetime;
            }else{
             main.duration=totalDuration[particleSys].duration*proportion-totalDuration[particleSys].startLifetimeConstantMax;
             var startLifetime=main.startLifetime;
             startLifetime.constantMin=totalDuration[particleSys].startLifetimeConstantMin;
             startLifetime.constant   =totalDuration[particleSys].startLifetimeConstant   ;
             startLifetime.constantMax=totalDuration[particleSys].startLifetimeConstantMax;
             main.startLifetime=startLifetime;
            }
           }
           Log.DebugMessage("SkillVisualEffect:SetDuration:proportion:"+proportion+";main.duration:"+main.duration+";main.startLifetime.constantMax:"+main.startLifetime.constantMax,this);
          }
         }
        }
        internal virtual void OnDeactivate(){
         Log.DebugMessage("SkillVisualEffect:OnDeactivate:timer:"+timer+";duration:"+duration+";particleSystemParent.main.duration:"+particleSystemParent.main.duration,this);
         particleSystemParent.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
         stopSFX=true;
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
          if(targetPos!=null){
           transform.position=targetPos.Value;
          }else if(target!=null){
           transform.position=target.transform.position;
          }
          if(duration>0f){
           if(timer>=duration){
            if(!audioSources.Any(audioSource=>audioSource.isPlaying)){
             OnDeactivate();
             return;
            }
           }else{
            if(duration-timer>=1f){
             if(!particleSystem.Any(particleSys=>particleSys.isPlaying)){
              SetParticleSysDuration();
              Log.DebugMessage("SkillVisualEffect:particleSystemParent.Play(true):timer:"+timer+";duration:"+duration+";particleSystemParent.main.duration:"+particleSystemParent.main.duration,this);
              particleSystemParent.Play(true);
              playSFX=true;
             }
            }
           }
          }else if(!particleSystem.Any(particleSys=>particleSys.isPlaying)&&!audioSources.Any(audioSource=>audioSource.isPlaying)){
           loopCount++;
           if(loopCount>=loops){
            OnDeactivate();
            return;
           }else{
            SetParticleSysDuration();
            particleSystemParent.Play(true);
            playSFX=true;
           }
          }
         }
         timer+=Time.deltaTime;
        }
     protected bool playSFX=false;
     protected bool playingSFX=false;
     protected bool stopSFX=false;
     protected WaitUntil waitForParticleSystemParentPlay;
      protected WaitForSeconds fadeTimeInterval;
        protected virtual IEnumerator PlaySkillSFX(){
         Loop:{
          yield return waitForParticleSystemParentPlay;
          playSFX=false;
          stopSFX=false;
          playingSFX=true;
          //Log.DebugMessage("PlaySkillSFX:playingSFX");
          foreach(AudioSource audioSource in audioSources){
           audioSource.volume=1f;
           audioSource.Play();
          }
          PlayingLoop:{}
          if(stopSFX){
           goto StopPlaying;
          }
          if(duration>0f){
           if(timer>=duration-.05f){
            bool anyPlaying=false;
            foreach(AudioSource audioSource in audioSources){
             if(audioSource.isPlaying){
              audioSource.volume-=.1f;
              if(audioSource.volume<=0f){
               audioSource.Stop();
              }else{
               anyPlaying|=true;
              }
             }
            }
            if(anyPlaying){
             yield return fadeTimeInterval;
             goto PlayingLoop;
            }
           }else if(timer<duration){
            yield return null;
            goto PlayingLoop;
           }
          }else{
           bool anyPlaying=false;
           foreach(AudioSource audioSource in audioSources){
            if(audioSource.isPlaying){
             anyPlaying|=true;
            }
           }
           if(anyPlaying){
            yield return null;
            goto PlayingLoop;
           }
          }
          StopPlaying:{}
          foreach(AudioSource audioSource in audioSources){
           audioSource.Stop();
          }
          playingSFX=false;
         }
         goto Loop;
        }
    }
}