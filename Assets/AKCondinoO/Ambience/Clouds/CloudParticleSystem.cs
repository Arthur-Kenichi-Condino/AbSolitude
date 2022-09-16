#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Ambience.Clouds{
    internal class CloudParticleSystem:MonoBehaviour,ISingletonInitialization{
     internal static CloudParticleSystem singleton{get;set;}
     readonly System.Random random=new System.Random();
     [SerializeField]CloudParticle cloudParticlePrefab;
      internal Material sharedMaterial;
       internal Color sharedColor;
      [SerializeField]int maxParticles=10;
       internal readonly List<CloudParticle>activeParticles=new List<CloudParticle>();
        internal readonly Queue<CloudParticle>cachedParticles=new Queue<CloudParticle>();
        void Awake(){
         if(singleton==null){singleton=this;}else{DestroyImmediate(this);return;}
         MeshRenderer prefabRenderer=cloudParticlePrefab.GetComponent<MeshRenderer>();
         sharedMaterial=prefabRenderer.sharedMaterial;
         sharedColor=sharedMaterial.GetColor("_TintColor");
        }
        public void Init(){
        }
        public void OnDestroyingCoreEvent(object sender,EventArgs e){
         Log.DebugMessage("CloudParticleSystem:OnDestroyingCoreEvent");
        }
     [SerializeField]float spawnInterval=20f;
      float spawnTimer;
        void Update(){
         spawnTimer+=Time.deltaTime;
         if(spawnTimer>=spawnInterval){
            spawnTimer=0f;
          OnSpawnParticle();
         }
        }
        void OnSpawnParticle(){
         Log.DebugMessage("OnSpawnParticle()");
         if(activeParticles.Count<maxParticles){
          CloudParticle cloudParticle;
          cloudParticle=Instantiate(cloudParticlePrefab);
          cloudParticle.fadeIn=true;
          activeParticles.Add(cloudParticle);
         }
        }
    }
}