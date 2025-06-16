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
     internal readonly System.Random random=new System.Random();
     internal CloudsCamera cloudsCamera;
     [SerializeField]CloudParticle cloudParticlePrefab;
      internal Material sharedMaterial;
       internal Color sharedColor;
      [SerializeField]int maxParticles=10;
       internal readonly List<CloudParticle>activeParticles=new List<CloudParticle>();
        internal readonly Queue<CloudParticle>cachedParticles=new Queue<CloudParticle>();
        void Awake(){
         if(singleton==null){singleton=this;}else{DestroyImmediate(this);return;}
         cloudsCamera=GetComponentInChildren<CloudsCamera>();
         //Log.DebugMessage("cloudsCamera:"+cloudsCamera);
         MeshRenderer prefabRenderer=cloudParticlePrefab.GetComponentInChildren<MeshRenderer>();
         sharedMaterial=prefabRenderer.sharedMaterial;
         sharedColor=sharedMaterial.GetColor("_TintColor");
        }
        public void Init(){
        }
        public void OnDestroyingCoreEvent(object sender,EventArgs e){
         //Log.DebugMessage("CloudParticleSystem:OnDestroyingCoreEvent");
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
         //Log.DebugMessage("OnSpawnParticle()");
         if(activeParticles.Count<maxParticles){
          CloudParticle cloudParticle;
          cloudParticle=Instantiate(cloudParticlePrefab);
          cloudParticle.meshRenderer.enabled=false;
          cloudParticle.alpha.value=0f;
          cloudParticle.fadeIn=true;
          activeParticles.Add(cloudParticle);
         }
        }
     [SerializeField]internal CloudParticleAlphaSettings alphaSettings;
        [Serializable]internal class CloudParticleAlphaSettings{
         [SerializeField]internal float min=0.05f;
         [SerializeField]internal float max=0.4f;
         [SerializeField]internal float minIncrementSpeed=.0125f;
         [SerializeField]internal float maxIncrementSpeed=.025f;
         [SerializeField]internal float reverseChance=0.125f;
         [SerializeField]internal float reverseInterval=10f;
         [SerializeField]internal float changeIncrementSpeedValueChance=.125f;
         [SerializeField]internal float changeIncrementSpeedValueInterval=10f;
        }
     [SerializeField]internal CloudParticleAngleSettings angleSettings;
        [Serializable]internal class CloudParticleAngleSettings{
         [SerializeField]internal Vector3 minIncrementSpeed=new Vector3(0.0f,0.0f,-1.0f);
         [SerializeField]internal Vector3 maxIncrementSpeed=new Vector3(0.0f,0.0f,1.0f);
         [SerializeField]internal float reverseChance=.125f;
         [SerializeField]internal float reverseInterval=10f;
         [SerializeField]internal float changeIncrementSpeedValueChance=.125f;
         [SerializeField]internal float changeIncrementSpeedValueInterval=10f;
        }
     [SerializeField]internal CloudParticleOrbitSettings orbitSettings;
        [Serializable]internal class CloudParticleOrbitSettings{
         [SerializeField]internal Vector3 minIncrementSpeed=new Vector3(-2.0f,-2.0f,0.0f);
         [SerializeField]internal Vector3 maxIncrementSpeed=new Vector3(2.0f,2.0f,0.0f);
         [SerializeField]internal float reverseChance=.125f;
         [SerializeField]internal float reverseInterval=5f;
         [SerializeField]internal float changeIncrementSpeedValueChance=.125f;
         [SerializeField]internal float changeIncrementSpeedValueInterval=5f;
        }
     [SerializeField]internal CloudParticleDistanceSettings distanceSettings;
        [Serializable]internal class CloudParticleDistanceSettings{
         [SerializeField]internal float min=1f;
         [SerializeField]internal float max=2f;
         [SerializeField]internal float minIncrementSpeed=0.05f;
         [SerializeField]internal float maxIncrementSpeed=0.1f;
         [SerializeField]internal float reverseChance=.125f;
         [SerializeField]internal float reverseInterval=10f;
         [SerializeField]internal float changeIncrementSpeedValueChance=.125f;
         [SerializeField]internal float changeIncrementSpeedValueInterval=10f;
        }
     [SerializeField]internal CloudParticleScaleSettings scaleSettings;
        [Serializable]internal class CloudParticleScaleSettings{
         [SerializeField]internal Vector3 min=new Vector3(2.0f,2.0f,2.0f);
         [SerializeField]internal Vector3 max=new Vector3(3.0f,3.0f,3.0f);
         [SerializeField]internal Vector3 minIncrementSpeed=new Vector3(.0125f,.0125f,.0125f);
         [SerializeField]internal Vector3 maxIncrementSpeed=new Vector3(.125f,.125f,.125f);
         [SerializeField]internal float reverseChance=.125f;
         [SerializeField]internal float reverseInterval=10f;
         [SerializeField]internal float changeIncrementSpeedValueChance=.125f;
         [SerializeField]internal float changeIncrementSpeedValueInterval=10f;
        }
    }
}