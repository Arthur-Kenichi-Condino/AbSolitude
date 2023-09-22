#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Actors;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Sims.Weapons{
    internal class SimWeaponOnShootVisualEffect:MonoBehaviour{
     internal AudioSource audioSource;
     [SerializeField]internal AudioClip OnShootSFX;
        void Awake(){
         audioSource=GetComponent<AudioSource>();
         Log.DebugMessage("Awake():"+audioSource);
        }
     Coroutine[]shotSFXCoroutines;
    }
}