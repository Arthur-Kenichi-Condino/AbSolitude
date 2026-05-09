using System;
using UnityEngine;
using UnityEngine.UI;
namespace AKCondinoO.UIObjects{
    internal class Minimized:MonoBehaviour{
     internal UIWindowRoot root;
     private Button btn;
        internal void OnAwake(UIWindowRoot root){
         this.root=root;
         btn=GetComponent<Button>();
        }
     internal Window window;
        internal void RegisterWindow(Window window){
         this.window=window;
         btn.onClick.AddListener(()=>UISystem.singleton.windowDockManager.Restore(this,window));
        }
    }
}