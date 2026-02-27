using UnityEngine;
namespace AKCondinoO.Main{
    internal class Main:MonoBehaviour{
        void Awake(){
         ThreadDispatcher.Initialize();
        }
        void OnDestroy(){
         ThreadDispatcher.Shutdown();
         ThreadDispatcher.FlushCompleted();
        }
 }
}