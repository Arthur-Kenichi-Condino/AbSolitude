using UnityEngine;
namespace AKCondinoO.UIObjects{
    internal class UIWindow:MonoBehaviour{
     [SerializeField]internal Window window;
        void Awake(){
         window.root=this;
         window.OnAwake();
        }
    }
}