using UnityEngine;
using UnityEngine.UI;
namespace AKCondinoO.UIObjects{
    internal class CloseButton:MonoBehaviour{
     private Window window;
     private Button button;
        internal void OnAwake(Window window){
         this.window=window;
         button=GetComponent<Button>();
         button.onClick.AddListener(OnClick);
        }
        private void OnClick(){
         if(window==null)return;
         UISystem.singleton.windowDockManager.Minimize(window.minimizedBtn,window,true,((RectTransform)transform).position);
        }
    }
}