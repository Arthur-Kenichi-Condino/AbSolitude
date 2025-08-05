#if UNITY_EDITOR
    #define ENABLE_LOG_DEBUG
#endif
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using static AKCondinoO.Sims.SimObject;
namespace AKCondinoO.UI{
    internal class StatBar:MonoBehaviour{
     [SerializeField]internal Image backgroundBar;
     [SerializeField]internal Image bar;
     [SerializeField]internal Color[]colorsFromFullToEmpty;
     [SerializeField]internal float[]colorsFromFullToEmptyWeight;
     [SerializeField]internal string[]statsEventsToTrack;
     internal MethodInfo setMaxValueMethod;
     internal MethodInfo setValueMethod;
        internal void Awake(){
         setMaxValueMethod=GetType().GetMethod("SetMaxValue",BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance);
         setValueMethod   =GetType().GetMethod("SetValue"   ,BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance);
        }
        internal void OnEnable(){
          ScreenInput.singleton.ActiveSimSetEvent+=OnCurrentActiveSimChanged;
        }
        internal void OnDisable(){
         if(ScreenInput.singleton!=null){
          ScreenInput.singleton.ActiveSimSetEvent-=OnCurrentActiveSimChanged;
         }
        }
     internal Stats currentActiveSimStatsTracked;
     internal readonly Dictionary<string,(EventInfo eventInfo,object[]handler,MethodInfo addMethod,MethodInfo removeMethod)>statsEventsTracked=new();
      internal readonly Dictionary<string,(MethodInfo raiseEvent,object[]param)>raiseEventMethods=new();
        internal void OnCurrentActiveSimChanged(object sender,EventArgs args){
         Log.DebugMessage("OnCurrentActiveSimChanged:"+ScreenInput.singleton.currentActiveSim);
         if(currentActiveSimStatsTracked!=null){
          foreach(var kvp in statsEventsTracked){
           UnsubscribeFromEvent(kvp.Key,kvp.Value);
          }
          statsEventsTracked.Clear();
           raiseEventMethods.Clear();
          currentActiveSimStatsTracked=null;
         }
         if(ScreenInput.singleton.currentActiveSim!=null&&ScreenInput.singleton.currentActiveSim.stats!=null){
          currentActiveSimStatsTracked=ScreenInput.singleton.currentActiveSim.stats;
          foreach(string statEventToTrack in statsEventsToTrack){
           TrySubscribeToEvent(statEventToTrack);
           GetRaiseEventMethod(statEventToTrack);
          }
          foreach(var kvp in raiseEventMethods){
           MethodInfo raiseEventMethod=kvp.Value.raiseEvent;
           object[]param=kvp.Value.param;
           raiseEventMethod.Invoke(currentActiveSimStatsTracked,param);
          }
         }
        }
     internal readonly Dictionary<string,(EventInfo eventInfo,object[]handler,MethodInfo addMethod,MethodInfo removeMethod)>reflectionCache=new();
      internal readonly Dictionary<string,(MethodInfo raiseEvent,object[]param)>reflectionCacheRaiseEventMethods=new();
        void TrySubscribeToEvent(string statName){
         EventInfo eventInfo;
         object[]handler;
         MethodInfo addMethod;
         MethodInfo removeMethod;
         if(reflectionCache.TryGetValue(statName,out(EventInfo eventInfo,object[]handler,MethodInfo addMethod,MethodInfo removeMethod)eventData)){
          eventInfo=eventData.eventInfo;
          handler=eventData.handler;
          addMethod=eventData.addMethod;
          AddEventHandler();
         }else{
          string eventName=statName.EndsWith("Event")?statName:String.Intern(statName+"Event");
          eventInfo=typeof(Stats).GetEvent(eventName,BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance);
          if(eventInfo!=null){
           MethodInfo method;
           if(statName.StartsWith("Max")){
            method=setMaxValueMethod;
           }else{
            method=setValueMethod;
           }
           handler=new[]{Delegate.CreateDelegate(eventInfo.EventHandlerType,this,method)};
           addMethod=eventInfo.GetAddMethod(true);
           removeMethod=eventInfo.GetRemoveMethod(true);
           reflectionCache[statName]=(eventInfo,handler,addMethod,removeMethod);
           AddEventHandler();
          }
         }
         void AddEventHandler(){
          addMethod.Invoke(currentActiveSimStatsTracked,handler);
          statsEventsTracked[statName]=reflectionCache[statName];
          Log.DebugMessage("TrySubscribeToEvent:'success':eventInfo:"+eventInfo);
         }
        }
        void UnsubscribeFromEvent(string statName,(EventInfo eventInfo,object[]handler,MethodInfo addMethod,MethodInfo removeMethod)eventData){
         EventInfo eventInfo=eventData.eventInfo;
         object[]handler=eventData.handler;
         MethodInfo removeMethod=eventData.removeMethod;
         removeMethod.Invoke(currentActiveSimStatsTracked,handler);
         Log.DebugMessage("UnsubscribeFromEvent:'success':eventInfo:"+eventInfo);
        }
        void GetRaiseEventMethod(string statName){
         MethodInfo raiseEventMethod;
         if(reflectionCacheRaiseEventMethods.TryGetValue(statName,out(MethodInfo raiseEvent,object[]param)raiseEventMethodData)){
          raiseEventMethod=raiseEventMethodData.raiseEvent;
         }else{
          string raiseEventMethodName=statName.EndsWith("Event")?String.Intern(statName.Insert(statName.Length-"Event".Length,"RaiseEvent")):String.Intern(statName+"RaiseEvent");
          raiseEventMethod=typeof(Stats).GetMethod(raiseEventMethodName,BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance);
          Log.DebugMessage("GetRaiseEventMethod:raiseEventMethod:"+raiseEventMethod);
          reflectionCacheRaiseEventMethods[statName]=(raiseEventMethod,null);
         }
         raiseEventMethods[statName]=reflectionCacheRaiseEventMethods[statName];
        }
     float maxValue=1f;
        internal void SetMaxValue(object sender,EventArgs args){
         Log.DebugMessage("SetMaxValue:sender:"+sender);
         float maxValue;
         //this.maxValue=maxValue;
        }
     float value=.25f;
        internal void SetValue(object sender,EventArgs args){
         Log.DebugMessage("SetValue:sender:"+sender);
         float value;
         //this.value=value;
        }
     float percentage;
        void OnGUI(){
         if(ScreenInput.singleton.currentActiveSim==null||currentActiveSimStatsTracked==null){
          value=0f;
          maxValue=1f;
         }
         if(maxValue<=value){
          maxValue=value;
         }
         if(maxValue<=0f){
          maxValue=1f;
         }
         percentage=value/maxValue;
         Vector2 sizeDelta=bar.rectTransform.sizeDelta;
         sizeDelta.x=percentage*backgroundBar.rectTransform.sizeDelta.x;
         bar.rectTransform.sizeDelta=sizeDelta;
         Vector2 anchoredPosition=bar.rectTransform.anchoredPosition;
         anchoredPosition.x=(backgroundBar.rectTransform.sizeDelta.x/2f)*percentage;
         bar.rectTransform.anchoredPosition=anchoredPosition;
        }
    }
}