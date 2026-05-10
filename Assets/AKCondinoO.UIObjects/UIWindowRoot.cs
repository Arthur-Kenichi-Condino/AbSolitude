using UnityEngine;
namespace AKCondinoO.UIObjects{
    internal class UIWindowRoot:MonoBehaviour{
     internal Canvas canvas;
     internal Minimized minimized;
     internal Window window;
        void Awake(){
         if(UISystem.singleton!=null){
          UISystem.singleton.AddWindow(this);
         }
        }
        internal virtual void OnAddWindow(){
         canvas=GetComponentInParent<Canvas>();
         minimized=GetComponentInChildren<Minimized>();
         minimized.OnAwake(this);
         window=GetComponentInChildren<Window>();
         window.OnAwake(this);
        }
        internal virtual void ManualUpdate(){
         window.OnManualUpdate();
        }
    }
}