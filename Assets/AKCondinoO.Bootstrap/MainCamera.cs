using UnityEngine;
namespace AKCondinoO.Bootstrap{
    internal class MainCamera:MonoSingleton<MainCamera>{
     internal Camera mainCamera;
        public override void Initialize(){
         base.Initialize();
         mainCamera=GetComponent<Camera>();
        }
    }
}