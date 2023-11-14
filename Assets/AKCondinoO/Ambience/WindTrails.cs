#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.ParticleSystemJobs;
namespace AKCondinoO{
    internal class WindTrails:MonoBehaviour{
     private ParticleSystem ps;
     ParticleSystem.EmitParams emitParams=new ParticleSystem.EmitParams();
        void Awake(){
         ps=GetComponent<ParticleSystem>();
         lastEmitTime=Time.time;
        }
     float lastEmitTime;
        void Update(){
         var main=ps.main;
         float emissionRate=ps.emission.rateOverTimeMultiplier;
         int maxParticles=main.maxParticles;
         if(ps.particleCount<maxParticles){
          if(Time.time-lastEmitTime>emissionRate){
           lastEmitTime=Time.time;
           emitParams.velocity=WindZoneControl.singleton.transform.forward*main.startSpeed.constant;
           emitParams.position=MainCamera.singleton.transform.position-(WindZoneControl.singleton.transform.forward*((main.startSpeed.constant*main.startLifetime.constant)/4f));
           emitParams.applyShapeToPosition=true;
           ps.Emit(emitParams,1);
          }
         }
         job.windDirection=WindZoneControl.singleton.transform.forward;
         Log.DebugMessage("ps.particleCount:"+ps.particleCount);
        }
     private UpdateParticlesJob job=new UpdateParticlesJob();
        void OnParticleUpdateJobScheduled(){
         job.Schedule(ps);
        }
        struct UpdateParticlesJob:IJobParticleSystem{
         [ReadOnly]public Vector3 windDirection;
            public void Execute(ParticleSystemJobData particles){
             var randomSeeds=particles.randomSeeds;
             var velocitiesX=particles.velocities.x;
             var velocitiesY=particles.velocities.y;
             var velocitiesZ=particles.velocities.z;
             var positionsX=particles.positions.x;
             var positionsY=particles.positions.y;
             var positionsZ=particles.positions.z;
             for(int i=0;i<particles.count;i++){
              Vector3 curVelocity=new Vector3(
               velocitiesX[i],
               velocitiesY[i],
               velocitiesZ[i]
              );
              Vector3 curDir=curVelocity.normalized;
              Vector3 newVelocity=CalculateVelocity(ref this,curVelocity,curDir);
              velocitiesX[i]=newVelocity.x;
              velocitiesY[i]=newVelocity.y;
              velocitiesZ[i]=newVelocity.z;
             }
            }
        }
        //  [https://forum.unity.com/threads/change-velocity-direction-and-not-magnatude.368272/]
        private static Vector3 CalculateVelocity(ref UpdateParticlesJob job,Vector3 curVelocity,Vector3 curDir){
         Vector3 smoothDampVelocity=Vector3.zero;
         Vector3 newDir=curDir+(job.windDirection-curDir)*.1f;
         return newDir.normalized*curVelocity.magnitude;
        }
    }
}