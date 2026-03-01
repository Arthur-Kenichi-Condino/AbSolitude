using UnityEngine;
namespace AKCondinoO.Bootstrap{
    [DefaultExecutionOrder(-100000)]
    internal class Main:MonoSingleton<Main>{
     public override int initOrder{get{return 0;}}
     [SerializeField]ActiveZone activeZonePrefab;
        protected override void Awake(){
         base.Awake();
         if(singleton==this){
         }
        }
        void Start(){
         if(singleton==this){
          Logs.Message(Logs.LogType.Debug,"...hello, World!");
          ThreadDispatcher.Initialize();
          ActiveZone.EnsureExists(activeZonePrefab);
          SingletonManager.InitializeAll();
         }
        }
        protected override void OnDestroy(){
         if(singleton==this){
          ThreadDispatcher.Shutdown();
          ThreadDispatcher.FlushCompleted();
          SingletonManager.ShutdownAll();
          ActiveZone.Shutdown();
         }
         base.OnDestroy();
        }
        public override void Initialize(){
         base.Initialize();
         if(this!=null){
         }
        }
        public override void Shutdown(){
         if(this!=null){
         }
         Logs.Message(Logs.LogType.Debug,"...good night, World. :)");
         base.Shutdown();
        }
        void Update(){
         ThreadDispatcher.FlushCompleted();
         ActiveZone.ManualUpdateTransformAll();
         SingletonManager.ManualUpdateAll();
        }
    }
}