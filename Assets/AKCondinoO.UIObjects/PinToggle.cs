using System;
using UnityEngine;
using UnityEngine.UI;
namespace AKCondinoO.UIObjects{
    internal class PinToggle:MonoBehaviour{
     private Window window;
     private Toggle toggle;
        internal void OnAwake(Window window){
         this.window=window;
         toggle=GetComponent<Toggle>();
         toggle.onValueChanged.AddListener(OnToggleChanged);
        }
        private void OnToggleChanged(bool isOn){
         if(window==null)return;
         if(isOn){
          UISystem.singleton.windowDockManager.Pin  (window.minimizedBtn,window,((RectTransform)transform).position);
         }else{
          UISystem.singleton.windowDockManager.Unpin(window.minimizedBtn,window,((RectTransform)transform).position);
         }
        }
    }
}