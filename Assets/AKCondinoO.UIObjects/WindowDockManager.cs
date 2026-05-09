using System.Collections.Generic;
using UnityEngine;
namespace AKCondinoO.UIObjects{
    internal class WindowDockManager:MonoBehaviour{
     internal readonly UISystem uiSystem;
        internal WindowDockManager(UISystem uiSystem){
         this.uiSystem=uiSystem;
        }
     private readonly Dictionary<Window,Minimized>docked=new();
        internal void Minimize(Minimized minimizedBtn,Window window,Vector2 pos=default){
         if(docked.ContainsKey(window)){
          return;
         }
         window.gameObject.SetActive(false);
         var btnGO=minimizedBtn.gameObject;
         btnGO.SetActive(true);
         ((RectTransform)btnGO.transform).position=pos;
         docked.Add(window,minimizedBtn);
        }
        internal void Restore(Minimized minimizedBtn,Window window){
         if(!docked.TryGetValue(window,out var button)){
          return;
         }
         window.gameObject.SetActive(true);
         minimizedBtn.gameObject.SetActive(false);
         docked.Remove(window);
        }
        internal void OnManualUpdate(){
        }
        internal static bool IsNearScreenEdge(Vector2 pos,Canvas canvas){
         float edge=8f*canvas.scaleFactor;
         return
          pos.x<=edge||
          pos.x>=Screen.width -edge||
          pos.y<=edge||
          pos.y>=Screen.height-edge;
        }
    }
}