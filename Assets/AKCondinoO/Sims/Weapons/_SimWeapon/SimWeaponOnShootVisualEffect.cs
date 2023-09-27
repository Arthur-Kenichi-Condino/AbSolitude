#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using AKCondinoO.Sims.Actors;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace AKCondinoO.Sims.Weapons{
    internal class SimWeaponOnShootVisualEffect:MonoBehaviour{
     [SerializeField]AudioSource audioSourcePrefab;
        void Awake(){
        }
      [SerializeField]internal AudioClip[]onShootSFX;
       [SerializeField]internal ParticleSystem[]onShootVFX;
     readonly List<Coroutine>shotSFXCoroutines=new List<Coroutine>();
      readonly Dictionary<Coroutine,int>shotSFXCoroutinesIndex=new Dictionary<Coroutine,int>();
       readonly Queue<int>shotSFXCoroutinesAvailable=new Queue<int>();
        readonly Dictionary<int,bool>shotSFXCoroutinesRunFlag=new Dictionary<int,bool>();
        internal void OnShot(){
         Log.DebugMessage("OnShot()");
         if(!shotSFXCoroutinesAvailable.TryDequeue(out int coroutineIndex)){
          coroutineIndex=shotSFXCoroutines.Count;
          shotSFXCoroutinesRunFlag.Add(coroutineIndex,false);
          Coroutine coroutine=StartCoroutine(ShotSFXCoroutine(coroutineIndex));
          shotSFXCoroutines.Add(coroutine);
          shotSFXCoroutinesIndex.Add(coroutine,coroutineIndex);
         }else{
          shotSFXCoroutinesRunFlag[coroutineIndex]=true;
         }
        }
        IEnumerator ShotSFXCoroutine(int index){
         WaitUntil waitForNextPlay=new WaitUntil(()=>shotSFXCoroutinesRunFlag[index]);
         AudioSource[]audioSources=new AudioSource[onShootSFX.Length];
         for(int i=0;i<onShootSFX.Length;++i){
          audioSources[i]=Instantiate(audioSourcePrefab,transform,false).GetComponent<AudioSource>();
          audioSources[i].clip=onShootSFX[i];
          Log.DebugMessage("ShotSFXCoroutine("+index+"):"+onShootSFX[i]);
         }
         WaitUntil waitUntilAudioEnds=new WaitUntil(()=>audioSources.All((a)=>a.clip==null||!a.isPlaying));
         Loop:{
          for(int i=0;i<audioSources.Length;++i){
           if(audioSources[i].clip==null){
            continue;
           }
           audioSources[i].Play();
          }
          yield return waitUntilAudioEnds;
         }
         shotSFXCoroutinesRunFlag[index]=false;
         shotSFXCoroutinesAvailable.Enqueue(index);
         yield return waitForNextPlay;
         goto Loop;
        }
      [SerializeField]internal AudioClip[]onShootDrySFX;
       [SerializeField]internal ParticleSystem[]onShootDryVFX;
     readonly List<Coroutine>shotDrySFXCoroutines=new List<Coroutine>();
      readonly Dictionary<Coroutine,int>shotDrySFXCoroutinesIndex=new Dictionary<Coroutine,int>();
       readonly Queue<int>shotDrySFXCoroutinesAvailable=new Queue<int>();
        readonly Dictionary<int,bool>shotDrySFXCoroutinesRunFlag=new Dictionary<int,bool>();
        internal void OnShotDry(){
         Log.DebugMessage("OnShotDry()");
         if(!shotDrySFXCoroutinesAvailable.TryDequeue(out int coroutineIndex)){
          coroutineIndex=shotDrySFXCoroutines.Count;
          shotDrySFXCoroutinesRunFlag.Add(coroutineIndex,false);
          Coroutine coroutine=StartCoroutine(ShotDrySFXCoroutine(coroutineIndex));
          shotDrySFXCoroutines.Add(coroutine);
          shotDrySFXCoroutinesIndex.Add(coroutine,coroutineIndex);
         }else{
          shotDrySFXCoroutinesRunFlag[coroutineIndex]=true;
         }
        }
        IEnumerator ShotDrySFXCoroutine(int index){
         WaitUntil waitForNextPlay=new WaitUntil(()=>shotDrySFXCoroutinesRunFlag[index]);
         AudioSource[]audioSources=new AudioSource[onShootDrySFX.Length];
         for(int i=0;i<onShootDrySFX.Length;++i){
          audioSources[i]=Instantiate(audioSourcePrefab,transform,false).GetComponent<AudioSource>();
          audioSources[i].clip=onShootDrySFX[i];
          Log.DebugMessage("ShotDrySFXCoroutine("+index+"):"+onShootDrySFX[i]);
         }
         WaitUntil waitUntilAudioEnds=new WaitUntil(()=>audioSources.All((a)=>a.clip==null||!a.isPlaying));
         Loop:{
          for(int i=0;i<audioSources.Length;++i){
           if(audioSources[i].clip==null){
            continue;
           }
           audioSources[i].Play();
          }
          yield return waitUntilAudioEnds;
         }
         shotDrySFXCoroutinesRunFlag[index]=false;
         shotDrySFXCoroutinesAvailable.Enqueue(index);
         yield return waitForNextPlay;
         goto Loop;
        }
    }
}