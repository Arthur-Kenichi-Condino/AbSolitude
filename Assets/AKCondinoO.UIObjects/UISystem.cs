using AKCondinoO.Bootstrap;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.UIObjects{
    internal class UISystem:MonoSingleton<UISystem>{
     internal int uiLayer{get;private set;}
     internal bool initialized;
     internal UIGameEventHandler gameEventHandler;
     internal WindowDockManager windowDockManager;
     internal WindowsRoot windowsRoot;
     private readonly HashSet<UIWindowRoot>windows=new();
        public override void Initialize(){
         base.Initialize();
         uiLayer=LayerMask.NameToLayer("UI");
         gameEventHandler=new(this);
         windowDockManager=new(this);
         windowsRoot=GetComponentInChildren<WindowsRoot>();
         initialized=true;
         UIWindowRoot[]existingWindows=GetComponentsInChildren<UIWindowRoot>();
         foreach(UIWindowRoot windowRoot in existingWindows){
          AddWindow(windowRoot);
         }
        }
        internal void AddWindow(UIWindowRoot windowRoot){
         Logs.Debug(()=>"try add:"+windowRoot.name,windowRoot);
         if(!initialized){
          Logs.Debug(()=>"'!initialized':can't add:"+windowRoot.name,windowRoot);
          return;
         }
         if(windows.Add(windowRoot)){
          Logs.Debug(()=>"added:"+windowRoot.name,windowRoot);
          windowRoot.OnAddWindow();
          RegisterWindow(windowRoot);
         }else{
          Logs.Debug(()=>"already added:"+windowRoot.name,windowRoot);
         }
        }
        internal void RegisterWindow(UIWindowRoot windowRoot){
         windowDockManager.OnRegistration(windowRoot);
        }
        public override void ManualUpdate(){
         base.ManualUpdate();
         windowDockManager.OnManualUpdate();
         foreach(var windowRoot in windows){
          windowRoot.ManualUpdate();
         }
        }
    }
}