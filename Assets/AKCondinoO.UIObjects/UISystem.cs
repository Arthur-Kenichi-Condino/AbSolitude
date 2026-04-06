using AKCondinoO.Bootstrap;
using UnityEngine;
namespace AKCondinoO.UIObjects{
    internal class UISystem:MonoSingleton<UISystem>{
     internal int uiLayer{get;private set;}
        public override void Initialize(){
         base.Initialize();
         uiLayer=LayerMask.NameToLayer("UI");
        }
    }
}