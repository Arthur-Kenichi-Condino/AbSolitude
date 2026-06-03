using AKCondinoO.Bootstrap;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static AKCondinoO.UIObjects.UISystem;
namespace AKCondinoO.UIObjects{
    internal class WindowDockManager:MonoBehaviour{
     internal readonly UISystem uiSystem;
        internal WindowDockManager(UISystem uiSystem){
         this.uiSystem=uiSystem;
        }
        internal void OnRegistration(UIObject windowRoot){
         var minimized=windowRoot.minimized;
         var window=windowRoot.window;
         minimized.RegisterWindow(window);
         window.RegisterMinimizedBtn(minimized);
         minimized.gameObject.SetActive(false);
        }
     private readonly Dictionary<Window,Minimized>docked=new();
        internal void Minimize(Minimized minimizedBtn,Window window,bool closeButton,Vector2 rawPosition=default){
         if(docked.ContainsKey(window)){
          return;
         }
         window      .OnMinimize(closeButton);
         minimizedBtn.OnMinimize(closeButton,rawPosition);
         window      .OnMinimized();
         minimizedBtn.OnMinimized();
         docked.Add(window,minimizedBtn);
        }
        internal void Restore(Minimized minimizedBtn,Window window){
         if(!docked.TryGetValue(window,out var button)){
          return;
         }
         minimizedBtn.OnRestore();
         window      .OnRestore();
         minimizedBtn.OnRestored();
         window      .OnRestored();
         docked.Remove(window);
        }
        internal void OnManualUpdate(){
        }
    }
}