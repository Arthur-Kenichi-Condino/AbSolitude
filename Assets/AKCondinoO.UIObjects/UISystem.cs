using AKCondinoO.Bootstrap;
using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.UIObjects{
    internal class UISystem:MonoSingleton<UISystem>{
     internal int uiLayer{get;private set;}
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
         UIWindowRoot[]existingWindows=GetComponentsInChildren<UIWindowRoot>();
         foreach(UIWindowRoot windowRoot in existingWindows){
          AddWindow(windowRoot);
         }
        }
        internal void AddWindow(UIWindowRoot windowRoot){
         Logs.Debug(()=>"try add:"+windowRoot.name,windowRoot);
         if(windows.Add(windowRoot)){
          Logs.Debug(()=>"added:"+windowRoot.name,windowRoot);
          windowRoot.OnAddWindow();
         }else{
          Logs.Debug(()=>"already added:"+windowRoot.name,windowRoot);
         }
        }
        public override void ManualUpdate(){
         base.ManualUpdate();
         foreach(var windowRoot in windows){
          windowRoot.ManualUpdate();
         }
        }
    }
}