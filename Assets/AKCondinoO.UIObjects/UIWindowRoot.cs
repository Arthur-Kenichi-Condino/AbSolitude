using UnityEditor.PackageManager.UI;
using UnityEngine;
namespace AKCondinoO.UIObjects{
    internal class UIWindowRoot:MonoBehaviour{
     internal Window window;
        void Awake(){
         if(UISystem.singleton!=null){
          UISystem.singleton.AddWindow(this);
         }
        }
        internal virtual void OnAddWindow(){
         window=GetComponentInChildren<Window>();
         window.OnAwake(this);
        }
        internal virtual void ManualUpdate(){
         window.OnManualUpdate();
        }
    }
}