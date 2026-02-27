using UnityEngine;
namespace AKCondinoO.Bootstrap{
    [DefaultExecutionOrder(-100000)]
    internal class Main:Singleton<Main>{
     public override int initOrder{get{return 0;}}
        protected override void Awake(){
         base.Awake();
         if(singleton==this){
         }
        }
        void Start(){
         if(singleton==this){
          SingletonManager.InitializeAll();
         }
        }
        protected override void OnDestroy(){
         if(singleton==this){
          SingletonManager.ShutdownAll();
         }
         base.OnDestroy();
        }
        public override void Initialize(){
         base.Initialize();
         if(this!=null){
          Log.Message(Log.LogType.Debug,"...hello, World!");
          ThreadDispatcher.Initialize();
         }
        }
        public override void Shutdown(){
         if(this!=null){
          ThreadDispatcher.Shutdown();
          ThreadDispatcher.FlushCompleted();
          Log.Message(Log.LogType.Debug,"...good night, World. :)");
         }
         base.Shutdown();
        }
    }
}