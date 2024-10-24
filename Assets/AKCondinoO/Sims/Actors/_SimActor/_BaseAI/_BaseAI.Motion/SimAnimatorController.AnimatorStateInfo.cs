#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
namespace AKCondinoO.Sims.Actors{
    internal partial class SimAnimatorController{
     internal Dictionary<int,float>animationTime{get;private set;}
      internal Dictionary<int,float>animationTimeInCurrentLoop{get;private set;}
     Dictionary<int,float>normalizedTime;
      Dictionary<int,float>normalizedTimeInCurrentLoop;
     Dictionary<int,int>loopCount;//  use integer part of normalizedTime [https://answers.unity.com/questions/1317841/how-to-find-the-normalised-time-of-a-looping-anima.html]
      Dictionary<int,int>lastLoopCount;
       Dictionary<int,bool>looped;
     Dictionary<int,List<AnimatorClipInfo>>animatorClip;
     Dictionary<int,int>currentClipInstanceID;
      Dictionary<int,string>currentClipName;
        protected virtual void GetAnimatorStateInfo(){
         //  [https://answers.unity.com/questions/1035587/how-to-get-current-time-of-an-animator.html]
         foreach(var layer in animatorClip){
          int layerIndex=layer.Key;
          List<AnimatorClipInfo>clipList=layer.Value;
          clipList.Clear();
          AnimatorStateInfo animatorState=animator.GetCurrentAnimatorStateInfo(layerIndex);
                                          animator.GetCurrentAnimatorClipInfo (layerIndex,clipList);
          if(clipList.Count>0){
           if(currentClipInstanceID[layerIndex]!=(currentClipInstanceID[layerIndex]=clipList[0].clip.GetInstanceID())||currentClipName[layerIndex]!=clipList[0].clip.name){
            //Log.DebugMessage("changed to new clipList[0].clip.name:"+clipList[0].clip.name+";clipList[0].clip.GetInstanceID():"+clipList[0].clip.GetInstanceID());
            OnAnimationChanged(animatorState:animatorState,layerIndex:layerIndex,lastClipName:currentClipName[layerIndex],currentClipName:clipList[0].clip.name);
            currentClipName[layerIndex]=clipList[0].clip.name;
            looped[layerIndex]=false;
           }
           lastLoopCount[layerIndex]=loopCount[layerIndex];
           if(loopCount[layerIndex]<(loopCount[layerIndex]=Mathf.FloorToInt(animatorState.normalizedTime))){
            //Log.DebugMessage("current animation (layerIndex:"+layerIndex+") looped:"+loopCount[layerIndex]);
            looped[layerIndex]=true;
            OnAnimationLooped(animatorState:animatorState,layerIndex:layerIndex,currentClipName:currentClipName[layerIndex]);
           }
           normalizedTime[layerIndex]=animatorState.normalizedTime;
            normalizedTimeInCurrentLoop[layerIndex]=Mathf.Repeat(animatorState.normalizedTime,1.0f);
           //Log.DebugMessage("current clipList[0].clip.name:"+clipList[0].clip.name);
           animationTime[layerIndex]=clipList[0].clip.length*normalizedTime[layerIndex];
            animationTimeInCurrentLoop[layerIndex]=clipList[0].clip.length*normalizedTimeInCurrentLoop[layerIndex];
           //Log.DebugMessage("current animationTime:"+animationTime[layerIndex]);
           OnAnimationIsPlaying(animatorState:animatorState,layerIndex:layerIndex,currentClipName:currentClipName[layerIndex]);
          }
         }
        }
        protected void OnAnimationLooped(AnimatorStateInfo animatorState,int layerIndex,string currentClipName){
         actor.OnShouldSetNextMotionAnimatorAnimationLooped(animatorState:animatorState,layerIndex:layerIndex,currentClipName:currentClipName);
        }
        protected void OnAnimationChanged(AnimatorStateInfo animatorState,int layerIndex,string lastClipName,string currentClipName){
         actor.OnShouldSetNextMotionAnimatorAnimationChanged(animatorState:animatorState,layerIndex:layerIndex,lastClipName:lastClipName,currentClipName:currentClipName);
        }
        protected void OnAnimationIsPlaying(AnimatorStateInfo animatorState,int layerIndex,string currentClipName){
         actor.OnShouldSetNextMotionAnimatorAnimationIsPlaying(animatorState:animatorState,layerIndex:layerIndex,currentClipName:currentClipName);
        }
        /// <summary>
        ///  Check if animation locks another motion beforehand
        /// </summary>
        /// <param name="motion"></param>
        /// <returns></returns>
        internal bool CurrentAnimationAllowsMotionChangeTo(BaseAI.ActorMotion motion){
         return true;
        }
     bool synced=true;
        /// <summary>
        ///  Wait for the end of the currently running animation
        /// </summary>
        /// <returns></returns>
        internal bool Sync(){
         return synced;
        }
    }
}