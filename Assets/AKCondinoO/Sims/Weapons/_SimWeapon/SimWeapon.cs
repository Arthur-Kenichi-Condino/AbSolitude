#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Weapons{
    internal class SimWeapon:SimObject{
        protected override void Awake(){
         base.Awake();
        }
        internal override void OnActivated(){
         base.OnActivated();
         ammo=startingAmmo;
        }
     [SerializeField]internal float startingAmmo=0f;
     internal float ammo=0f;
      [SerializeField]internal float ammoPerMagazine=0f;
       internal float ammoLoaded=0f;
        internal bool Reload(){
         float ammoToLoad=ammoPerMagazine-ammoLoaded;
         if(ammoToLoad>0f){
          Log.DebugMessage("ammoToLoad:"+ammoToLoad);
          ammoToLoad=Math.Min(ammoToLoad,ammo);
          if(ammoToLoad>0f){
           Log.DebugMessage("reloading ammo:"+ammoToLoad);
           ammoLoaded=ammoToLoad;
           ammo-=ammoToLoad;
           Log.DebugMessage("remaining ammo:"+ammo);
           return true;
          }
         }
         return false;
        }
     [SerializeField]internal Transform muzzle;
        internal bool Shoot(){
         return false;
        }
    }
}