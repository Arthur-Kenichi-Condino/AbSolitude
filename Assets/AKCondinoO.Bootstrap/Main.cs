using UnityEngine;
namespace AKCondinoO.Bootstrap{
    [DefaultExecutionOrder(-100000)]
    internal class Main:MonoBehaviour{
        void Awake(){
         Log.Message("...hello, World!",Log.LogType.Debug);
         ThreadDispatcher.Initialize();
        }
        void OnDestroy(){
         ThreadDispatcher.Shutdown();
         ThreadDispatcher.FlushCompleted();
         Log.Message("...good night, World. :)",Log.LogType.Debug);
        }
    }
}