#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System;
using System.Collections;
using System.Reflection;
using UnityEngine;
namespace AKCondinoO.Ambience.Clouds{
    public class CloudParticle:MonoBehaviour{
     internal MeshRenderer meshRenderer;
        void Awake(){
         meshRenderer=GetComponent<MeshRenderer>();
         color=CloudParticleSystem.singleton.sharedColor;
        }
     internal bool fadeIn;
      Color color;
        void Update(){
         meshRenderer.material.SetColor("_TintColor",color);
        }
    }
}