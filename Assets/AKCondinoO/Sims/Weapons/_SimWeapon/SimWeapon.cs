#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Weapons{
    internal class SimWeapon:SimObject{
     internal float ammo=0f;
      [SerializeField]internal float ammoPerMagazine=0f;
       internal float ammoLoaded=0f;
        internal void Reload(){
         
        }
    }
}