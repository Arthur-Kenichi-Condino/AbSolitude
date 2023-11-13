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
     [SerializeField]int maxParticles=10;
     private ParticleSystem ps;
     ParticleSystem.EmitParams emitParams=new ParticleSystem.EmitParams();
        void Awake(){
         ps=GetComponent<ParticleSystem>();
        }
        void Update(){
         if(ps.particleCount<=0){
          emitParams.velocity=WindZoneControl.singleton.transform.forward*ps.main.startSpeed.constant;
          ps.Emit(emitParams,1);
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