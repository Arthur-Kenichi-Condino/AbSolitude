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
        #if UNITY_EDITOR
        [ContextMenu("AKCondinoO/On Editor/Build UI")]
        private void OnEditorInspectorBuildUI(){
         OnAddWindow(true);
        }
        #endif
        internal virtual void OnAddWindow(bool editor=false){
         canvas=GetComponentInParent<Canvas>(true);
         minimized=GetComponentInChildren<Minimized>(true);
         minimized.OnAwake(this);
         if(!editor)modules.Add(minimized);
         window=GetComponentInChildren<Window>(true);
         window.OnAwake(this);
         if(!editor)modules.Add(window);
        }
        internal virtual void ManualUpdate(){
         foreach(var module in modules){
          module.OnManualUpdate();
         }
        }
    }
}