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
      [SerializeField]bool mute=false;
      [SerializeField]internal AudioClip GoodMorningMusic;
      [SerializeField]internal AudioClip RushingNoonMusic;
      [SerializeField]internal AudioClip SpookyNightMusic;
        private void Awake(){
         if(singleton==null){singleton=this;}else{DestroyImmediate(this);return;}
         audioSource=GetComponent<AudioSource>();
        }
        public void Init(){
         waitForNewMusic=new WaitUntil(()=>{return newMusic!=null;});
         fadeTimeInterval=new WaitForSeconds(0.05f);
         BGMusicCoroutine=StartCoroutine(PlayBGMusic());
        }
        public void OnDestroyingCoreEvent(object sender,EventArgs e){
         //Log.DebugMessage("BGM:OnDestroyingCoreEvent");
         if(this!=null){
          StopCoroutine(BGMusicCoroutine);
         }
        }
     Coroutine BGMusicCoroutine;
     internal AudioClip newMusic=null;
      WaitUntil waitForNewMusic;
       WaitForSeconds fadeTimeInterval;
        private IEnumerator PlayBGMusic(){
            Loop:{
             yield return waitForNewMusic;
             Log.DebugMessage("PlayBGMusic:newMusic:"+newMusic);
             if(audioSource.isPlaying){
              for(float vol;audioSource.volume>0f;vol=audioSource.volume-.1f,audioSource.volume=vol<=0f?0f:vol){
               yield return fadeTimeInterval;
              }
              audioSource.Stop();
             }
             audioSource.clip=newMusic;
             audioSource.volume=1f;
             if(!mute){
              audioSource.Play();
             }
             newMusic=null;
            }
            goto Loop;
        }
    }
}