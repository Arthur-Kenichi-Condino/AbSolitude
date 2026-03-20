using UnityEngine;
namespace AKCondinoO.Bootstrap{
    ///<summary>
    ///  AI: software não é uma escultura em pedra. É mais parecido com:
    /// um jardim que você mantém ao longo do tempo. Você poda, ajusta, melhora.
    ///  Main:
    ///  - Entry point real do runtime
    ///  - Responsável por inicializar sistemas globais
    ///  - Controla ciclo de vida de threads
    ///  - Coordena Singletons
    ///  - Atualiza manualmente sistemas no Update
    ///</summary>
    [DefaultExecutionOrder(-100000)]
    internal class Main:MonoSingleton<Main>{
     [SerializeField]private int workerCount=8;
     [SerializeField]ActiveZone activeZonePrefab;
     [SerializeField]private bool debugToggleLogsDebugMessages=false;
        protected override void Awake(){
         base.Awake();
         if(singleton==this){
         }
        }
        void Start(){
         if(singleton==this){
          Logs.Debug("...hello, World!");
          ThreadDispatcher.Initialize(workerCount<=0?null:workerCount);
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
         Logs.Debug("...good night, World. :)");
         base.Shutdown();
        }
        void Update(){
         if(debugToggleLogsDebugMessages){
          Logs.enableAll=!Logs.enableAll;
          debugToggleLogsDebugMessages=false;
         }
         ThreadDispatcher.FlushCompleted();
         ActiveZone.ManualUpdateTransformAll();
         SingletonManager.ManualUpdateAll();
        }
    }
}