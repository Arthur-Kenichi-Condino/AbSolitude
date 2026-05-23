using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.UIObjects{
    internal class UIObject:MonoBehaviour{
     internal Canvas canvas;
     internal Minimized minimized;
     internal Window window;
     internal readonly HashSet<UIObjectModule>modules=new();
        void Awake(){
         if(UISystem.singleton!=null){
          UISystem.singleton.AddWindow(this);
         }
        }
        internal virtual void OnAddWindow(){
         canvas=GetComponentInParent<Canvas>();
         minimized=GetComponentInChildren<Minimized>();
         minimized.OnAwake(this);
         modules.Add(minimized);
         window=GetComponentInChildren<Window>();
         window.OnAwake(this);
         modules.Add(window);
        }
        internal virtual void ManualUpdate(){
         foreach(var module in modules){
          module.OnManualUpdate();
         }
        }
    }
}