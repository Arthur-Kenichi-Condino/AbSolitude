#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.Music{
    internal class BGM:MonoBehaviour,ISingletonInitialization{
     internal static BGM singleton{get;set;}
     internal AudioSource audioSource;
        private void Awake(){
         if(singleton==null){singleton=this;}else{DestroyImmediate(this);return;}
         audioSource=GetComponent<AudioSource>();
        }
        public void Init(){
         waitForNewMusic=new WaitUntil(()=>{return newMusic!=null;});
         BGMusicCoroutine=StartCoroutine(PlayBGMusic());
        }
        public void OnDestroyingCoreEvent(object sender,EventArgs e){
         Log.DebugMessage("BGM:OnDestroyingCoreEvent");
         if(this!=null){
          StopCoroutine(BGMusicCoroutine);
         }
        }
     Coroutine BGMusicCoroutine;
     internal AudioClip newMusic=null;
      WaitUntil waitForNewMusic;
        private IEnumerator PlayBGMusic(){
            Loop:{
             yield return waitForNewMusic;
             Log.DebugMessage("PlayBGMusic:newMusic:"+newMusic);
             newMusic=null;
            }
            goto Loop;
        }
    }
}