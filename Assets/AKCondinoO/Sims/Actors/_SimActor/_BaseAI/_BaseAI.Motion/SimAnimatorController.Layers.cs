#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static AKCondinoO.Sims.Actors.BaseAI;
namespace AKCondinoO.Sims.Actors{
    internal partial class SimAnimatorController{
     [SerializeField]internal WeaponLayer[]weaponLayerNames=new WeaponLayer[]{
      new WeaponLayer{weaponType=WeaponTypes.None,layerName="Base Layer"},
      new WeaponLayer{weaponType=WeaponTypes.SniperRifle,layerName="Carrying Rifle"},
     };
        [Serializable]internal class WeaponLayer{
         [SerializeField]internal WeaponTypes weaponType;
         [SerializeField]internal string layerName;
        }
     [SerializeField]internal WeaponAimLayer[]weaponAimLayerNames=new WeaponAimLayer[]{
      new WeaponAimLayer{weaponType=WeaponTypes.None,layerName="Base Layer"},
      new WeaponAimLayer{weaponType=WeaponTypes.SniperRifle,layerName="Aiming with Rifle"},
     };
        [Serializable]internal class WeaponAimLayer{
         [SerializeField]internal WeaponTypes weaponType;
         [SerializeField]internal string layerName;
        }
     internal readonly Dictionary<(int layer,string clipName),string>layerClipToFullPath=new Dictionary<(int,string),string>();
        internal string GetFullPath(int layerIndex,string currentClipName){
         (int layer,string clipName)layerClip=(layerIndex,currentClipName);
         if(layerClipToFullPath.TryGetValue(layerClip,out string fullPath)){
          return fullPath;
         }
         string layerName=animator.GetLayerName(layerIndex);
         fullPath=layerName+"."+currentClipName;
         layerClipToFullPath.Add(layerClip,fullPath);
         return fullPath;
        }
    }
}