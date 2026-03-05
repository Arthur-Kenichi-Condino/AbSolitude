using UnityEngine;
namespace AKCondinoO.Bootstrap{
    ///<summary>
    ///  - Entry point real do runtime
    ///  - Responsável por inicializar sistemas globais
    ///  - Controla ciclo de vida de threads
    ///  - Coordena Singletons
    ///  - Atualiza manualmente sistemas no Update
    ///</summary>
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
          ActiveZone.InitializeAny();
         }
        }
        protected override void OnDestroy(){
         if(singleton==this){
          SingletonManager.PreShutdownAll();
          ThreadDispatcher.Shutdown();
          ThreadDispatcher.FlushCompleted(true);
          SingletonManager.ShutdownAll();
          ActiveZone.ShutdownAny();
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